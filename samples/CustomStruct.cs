#:project ../src/ObjectLayoutInspector/ObjectLayoutInspector.csproj
#:property PublishAot=false

using System.Runtime.InteropServices;
using ObjectLayoutInspector;

Console.WriteLine("=== Custom Struct Layouts ===");
Console.WriteLine();

Console.WriteLine("--- MyPoint (no padding) ---");
TypeLayout.PrintLayout<MyPoint>();
Console.WriteLine();
ArrayLayout.PrintLayout<MyPoint>(3);

Console.WriteLine("--- MyEvent (with padding) ---");
TypeLayout.PrintLayout<MyEvent>();
Console.WriteLine();
ArrayLayout.PrintLayout<MyEvent>(2);

[StructLayout(LayoutKind.Sequential)]
struct MyPoint
{
    public float X;
    public float Y;
    public float Z;
}

#pragma warning disable CS0649 // Fields are intentionally never assigned — used only for layout inspection
struct MyEvent
{
    public long Timestamp;
    public int EventId;
    public byte Priority;
    public bool IsProcessed;
}
#pragma warning restore CS0649
