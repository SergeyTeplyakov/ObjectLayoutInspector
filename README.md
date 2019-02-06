# Getting an instance layout at runtime

![Demo](images/FieldsLayout_Figure1.gif "Demo")

There is no official documentation about fields layout because the CLR authors reserved the right to change it in the future. But knowledge about the layout can be helpful if you're curious or if you're working on a performance critical application. 

How can we inspect the layout? We can look at a raw memory in Visual Studio or use !dumpobj command in [SOS Debugging Extension](https://docs.microsoft.com/en-us/dotnet/framework/tools/sos-dll-sos-debugging-extension). These approaches are tedious and boring, so we'll try to write a tool  that will print an object layout at runtime.

## Getting the field offset at runtime
We're not going to use unmanaged code or Profiling API, instead we'll use the power of [LdFlda](https://msdn.microsoft.com/en-us/library/system.reflection.emit.opcodes.ldflda(v=vs.110).aspx) instruction. This IL instruction returns an address of a field for a given type. Unfortunately, this instruction is not exposed in C# language, so we have to do some light-weight code generation to work around that limitation.

In [Dissecting the new() constraint in C#](https://blogs.msdn.microsoft.com/seteplia/2017/02/01/dissecting-the-new-constraint-in-c-a-perfect-example-of-a-leaky-abstraction/) we already did something similar. We'll generate a [Dynamic Method](https://docs.microsoft.com/en-us/dotnet/framework/reflection-and-codedom/how-to-define-and-execute-dynamic-methods) with `LdFlda` instructions.

The method should do the following:

* Create an array for all field addresses.
* Enumerate over each `FieldInfo` of an object to get the offset by calling `LdFlda` instruction.
* Convert the result of `LdFlda` instruction to `long` and store the result in the array.
* Return the array.

```csharp
private static Func<object, long[]> GenerateFieldOffsetInspectionFunction(FieldInfo[] fields)
{
    var method = new DynamicMethod(
        name: "GetFieldOffsets",
        returnType: typeof(long[]),
        parameterTypes: new[] { typeof(object) },
        m: typeof(InspectorHelper).Module,
        skipVisibility: true);

    ILGenerator ilGen = method.GetILGenerator();

    // Declaring local variable of type long[]
    ilGen.DeclareLocal(typeof(long[]));
    // Loading array size onto evaluation stack
    ilGen.Emit(OpCodes.Ldc_I4, fields.Length);

    // Creating an array and storing it into the local
    ilGen.Emit(OpCodes.Newarr, typeof(long));
    ilGen.Emit(OpCodes.Stloc_0);

    for (int i = 0; i < fields.Length; i++)
    {
        // Loading the local with the array
        ilGen.Emit(OpCodes.Ldloc_0);

        // Loading an index of the array where we're going to store the element
        ilGen.Emit(OpCodes.Ldc_I4, i);

        // Loading object instance onto evaluation stack
        ilGen.Emit(OpCodes.Ldarg_0);

        // Getting the address for a given field
        ilGen.Emit(OpCodes.Ldflda, fields[i]);

        // Converting field offset to long
        ilGen.Emit(OpCodes.Conv_I8);

        // Storing the offset in the array
        ilGen.Emit(OpCodes.Stelem_I8);
    }

    ilGen.Emit(OpCodes.Ldloc_0);
    ilGen.Emit(OpCodes.Ret);

    return (Func<object, long[]>)method.CreateDelegate(typeof(Func<object, long[]>));
}
```

Now we can create a helper function that will provide the offsets for each field for a given type:

```csharp
public static (FieldInfo fieldInfo, int offset)[] GetFieldOffsets(Type t)
{
    var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

    Func<object, long[]> fieldOffsetInspector = GenerateFieldOffsetInspectionFunction(fields);

    var instance = CreateInstance(t);
    var addresses = fieldOffsetInspector(instance);

    if (addresses.Length == 0)
    {
        return Array.Empty<(FieldInfo, int)>();
    }

    var baseLine = addresses.Min();
    
    // Converting field addresses to offsets using the first field as a baseline
    return fields
        .Select((field, index) => (field: field, offset: (int)(addresses[index] - baseLine)))
        .OrderBy(tpl => tpl.offset)
        .ToArray();
}
```

The function is pretty straightforward with one caveat: `LdFlda` instruction expects an object instance on the evaluation stack. For value types and for reference types with a default constructor, the solution is trivial: use `Activator.CreateInstance(Type)`. But what if want to inspect classes that doesn't have a default constructor?

In this case we can use lesser known "generic factory" called [`FormatterServices.GetUninitializedObject(Type)`](https://msdn.microsoft.com/en-us/library/system.runtime.serialization.formatterservices.getuninitializedobject(v=vs.110).aspx):

```csharp
private static object CreateInstance(Type t)
{
    return t.IsValueType ? Activator.CreateInstance(t) : FormatterServices.GetUninitializedObject(t);
}
```

Let's test `GetFieldOffsets` to get the layout for the following type:

```csharp
class ByteAndInt
{
    public byte b;
    public int n;
}

Console.WriteLine(
    string.Join("\r\n",
        InspectorHelper.GetFieldOffsets(typeof(ByteAndInt))
            .Select(tpl => $"Field {tpl.fieldInfo.Name}: starts at offset {tpl.offset}"))
    );
```

The output is:

```
Field n: starts at offset 0
Field b: starts at offset 4
```

Interesting, but not sufficient. We can inspect offsets for each field, but it would be very helpful to know the size of each field to understand how efficient the layout is and how much empty space each instance has.

## Computing the size for a type instance

And again, there is no "official" way to get the size of the object instance. [`sizeof`](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/sizeof) operator works only for primitive types and user-defined structs with no fields of reference types. [`Marshal.SizeOf`](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.marshal.sizeof?view=netframework-4.7) returns a size of an object in unmanaged memory and is not suitable for our needs as well.

We'll compute instance size for value types and object separately. To compute the size of a struct we'll rely on the CLR itself. We'll generate a struct at runtime with the sequential layout with two fields of the same type:

```csharp
// Default layout is sequential
struct SizeComputer
{
    public MyStruct field1;
    public MyStruct field2;
}
```

The offset of the second field will give us the exact size of the struct.

The CLR supports runtime type generation using [`ModuleBuilder.DefineType`]. The `ModuleBuilder` generates a type in a separate assembly that restricts us to `MyStruct`s with public access modifier. Instead we can generate a method that returns an instance of a generic type with a well-known layout.

We'll define a special type:

```csharp
struct SizeComputer<T>
{
    public T field1;
    public T field2;
}
```

And will generate the following method at runtime for a given `FieldType`:

```csharp
object GenerateTypeForSizeComputation()
{
    SizeComputer<FieldType> l1 = default(SizeComputer<FieldType>);
    object l2 = l1; // box the local
    return l2;
}
```

Method [`GenerateSizeComputerOf`](https://github.com/SergeyTeplyakov/ObjectLayoutInspector/blob/master/src/ObjectLayoutInspector/ObjectLayoutInspector/InspectorHelper.cs#L106-L147) calls the generated method and gets the type by calling `result.GetType()`. The rest is very simple:

```csharp
public static int GetSizeOfValueTypeInstance(Type type)
{
    Debug.Assert(type.IsValueType);
    // Generate a struct with two fields of type 'type'
    var generatedType = GenerateSizeComputerOf(type);
    // The offset of the second field is the size of 'type'
    var fieldsOffsets = GetFieldOffsets(generatedType);
    return fieldsOffsets[1].offset;
}
```

To get the size of a reference type instance will use another trick: we'll get the max field offset, then add the size of that field and round that number to a pointer-size boundary. We already know how to compute the size of a value type and we know that every field of a reference type is just of the pointer size. So we've got everything we need:

```csharp
public static int GetSizeOfReferenceTypeInstance(Type type)
{
    var fields = GetFieldOffsets(type);

    if (fields.Length == 0)
    {
        // Special case: the size of an empty class is 1 Ptr size
        return IntPtr.Size;
    }

    var maxValue = fields.MaxBy(tpl => tpl.offset);
    int sizeCandidate = maxValue.offset + GetFieldSize(maxValue.fieldInfo.FieldType);

    // Rounding the candidate to the nearest ptr-size boundary
    int roundTo = IntPtr.Size - 1;
    return (sizeCandidate + roundTo) & (~roundTo);
}

public static int GetFieldSize(Type t)
{
    if (t.IsValueType)
    {
        return GetSizeOfValueTypeInstance(t);
    }

    return IntPtr.Size;
}
```

We have enough information to get a proper layout information for any type instance at runtime.

## Inspecting a value type layout at runtime

Let's start with value types and inspect the following struct:

```csharp
public struct NotAlignedStruct
{
    public byte m_byte1;
    public int m_int;

    public byte m_byte2;
    public short m_short;
}
```

Here is a result of [`TypeLayout.Print<NotAlignedStruct>()`](https://github.com/SergeyTeplyakov/ObjectLayoutInspector/blob/master/src/ObjectLayoutInspector/ObjectLayoutInspector/TypeLayout.cs#L20) method call:

```
Size: 12. Paddings: 4 (%33 of empty space)
|================================|
|     0: Byte m_byte1 (1 byte)   |
|--------------------------------|
|   1-3: padding (3 bytes)       |
|--------------------------------|
|   4-7: Int32 m_int (4 bytes)   |
|--------------------------------|
|     8: Byte m_byte2 (1 byte)   |
|--------------------------------|
|     9: padding (1 byte)        |
|--------------------------------|
| 10-11: Int16 m_short (2 bytes) |
|================================|
```

By default, a user-defined struct has the 'sequential' layout with `Pack` equal to `0`. Here is a rule that [the CLR follows](https://msdn.microsoft.com/en-us/library/system.runtime.interopservices.structlayoutattribute.pack(v=vs.110).aspx):

*Each field must align with fields of its own size (1, 2, 4, 8, etc., bytes) or the alignment of the type, whichever is smaller. Because the default alignment of the type is the size of its largest element, which is greater than or equal to all other field lengths, this usually means that fields are aligned by their size. For example, even if the largest field in a type is a 64-bit (8-byte) integer or the Pack field is set to 8, Byte fields align on 1-byte boundaries, Int16 fields align on 2-byte boundaries, and Int32 fields align on 4-byte boundaries.*

In this case, the alignment is equal to `4` that led to a reasonable overhead. We can change the `Pack` to 1, but we can get a performance degradation due to unaligned memory operations. Instead we can use [`LayoutKind.Auto`](https://msdn.microsoft.com/en-us/library/system.runtime.interopservices.layoutkind(v=vs.110).aspx) to allow the CLR to figure out the best layout:

```csharp
[StructLayout(LayoutKind.Auto)]
public struct NotAlignedStructWithAutoLayout
{
    public byte m_byte1;
    public int m_int;

    public byte m_byte2;
    public short m_short;
}
```

```
Size: 8. Paddings: 0 (%0 of empty space)
|================================|
|   0-3: Int32 m_int (4 bytes)   |
|--------------------------------|
|   4-5: Int16 m_short (2 bytes) |
|--------------------------------|
|     6: Byte m_byte1 (1 byte)   |
|--------------------------------|
|     7: Byte m_byte2 (1 byte)   |
|================================|
```

**Please, keep in mind** that the sequential layout for both value types and reference types is only possible if a type doesn't have "pointers" in it. If a struct or a class has at least one field of a reference type, the layout is automatically changed to `LayoutKind.Auto`.

## Inspecting a reference type layout at runtime

There are two main differences between the layout of a reference type and a value type. First, each "object" instance has a header and a method table pointer. And second, the default layout for "objects" is automatic not sequential. And similar to value types, the sequential layout is possible only for classes that doesn't have any fields of reference types.

Method [`TypeLayout.PrintLayout<T>(bool recursively = true)`](https://github.com/SergeyTeplyakov/ObjectLayoutInspector/blob/master/src/ObjectLayoutInspector/ObjectLayoutInspector/TypeLayout.cs#L20) takes an argument that allows to print the nested types as well. 

```csharp
public class ClassWithNestedCustomStruct
{
    public byte b;
    public NotAlignedStruct sp1;
}

TypeLayout.PrintLayout<ClassWithNestedCustomStruct>(recursively: true);
```

```
Size: 40. Paddings: 11 (%27 of empty space)
|========================================|
| Object Header (8 bytes)                |
|----------------------------------------|
| Method Table Ptr (8 bytes)             |
|========================================|
|     0: Byte b (1 byte)                 |
|----------------------------------------|
|   1-7: padding (7 bytes)               |
|----------------------------------------|
|  8-19: NotAlignedStruct sp1 (12 bytes) |
| |================================|     |
| |     0: Byte m_byte1 (1 byte)   |     |
| |--------------------------------|     |
| |   1-3: padding (3 bytes)       |     |
| |--------------------------------|     |
| |   4-7: Int32 m_int (4 bytes)   |     |
| |--------------------------------|     |
| |     8: Byte m_byte2 (1 byte)   |     |
| |--------------------------------|     |
| |     9: padding (1 byte)        |     |
| |--------------------------------|     |
| | 10-11: Int16 m_short (2 bytes) |     |
| |================================|     |
|----------------------------------------|
| 20-23: padding (4 bytes)               |
|========================================|
```

## The cost of wrapping a struct

Even though the type layout is pretty straightforward, I've found one interesting moment.

I've been investigating a memory issue in my project recently and I've noticed a strange thing: the sum of all fields of a managed object was higher than the size of the instance. I roughly knew the rules how the CLR lays out fields so I was puzzled. I've started working on this tool to understand that issue.

I've narrowed down the issue to the following case:

```csharp
public struct ByteWrapper
{
    public byte b;
}

public class ClassMultipleByteWrappers
{
    public ByteWrapper bw1;
    public ByteWrapper bw2;
    public ByteWrapper bw3;
}
```

```
     --- Automatic Layout ---              --- Sequential Layout ---     
Size: 24 bytes. Paddings: 21 bytes    Size: 8 bytes. Paddings: 5 bytes 
(%87 of empty space)                  (%62 of empty space)
|=================================|   |=================================|
| Object Header (8 bytes)         |   | Object Header (8 bytes)         |
|---------------------------------|   |---------------------------------|
| Method Table Ptr (8 bytes)      |   | Method Table Ptr (8 bytes)      |
|=================================|   |=================================|
|     0: ByteWrapper bw1 (1 byte) |   |     0: ByteWrapper bw1 (1 byte) |
|---------------------------------|   |---------------------------------|
|   1-7: padding (7 bytes)        |   |     1: ByteWrapper bw2 (1 byte) |
|---------------------------------|   |---------------------------------|
|     8: ByteWrapper bw2 (1 byte) |   |     2: ByteWrapper bw3 (1 byte) |
|---------------------------------|   |---------------------------------|
|  9-15: padding (7 bytes)        |   |   3-7: padding (5 bytes)        |
|---------------------------------|   |=================================|
|    16: ByteWrapper bw3 (1 byte) |
|---------------------------------|
| 17-23: padding (7 bytes)        |
|=================================|
```

Even though the size of the `ByteWrapper` is 1 byte, the CLR aligns each field on the pointer boundaries! **If the type layout is `LayoutKind.Auto`** the CLR will pad each field of a **custom value type**! This means that if you have multiple structs that wraps just a single `int` or `byte` and they're widely used in millions of objects, you could have a noticeable memory overhead!
