using System.Reflection;

namespace ObjectLayoutInspector
{
    public abstract class FieldLayoutBase
    {
        public int Size { get; }
        public int Offset { get; }

        protected FieldLayoutBase(int size, int offset)
        {
            Size = size;
            Offset = offset;
        }

        public override string ToString()
        {
            int fieldSize = Size;
            string byteOrBytes = fieldSize == 1 ? "byte" : "bytes";

            string offsetStr = $"{Offset}-{Offset - 1 + fieldSize}";
            if (fieldSize == 1)
            {
                offsetStr = Offset.ToString();
            }

            return $"{offsetStr,5}: {NameOrDescription} ({fieldSize} {byteOrBytes})";
        }

        protected abstract string NameOrDescription { get; }
    }

    public sealed class FieldLayout : FieldLayoutBase
    {
        public FieldLayout(int offset, FieldInfo fieldInfo)
            : base(InspectorHelper.GetFieldSize(fieldInfo.FieldType), offset)
        {
            FieldInfo = fieldInfo;
        }

        public FieldInfo FieldInfo { get; }

        protected override string NameOrDescription =>
            $"{FieldInfo.FieldType.Name} {FieldInfo.Name}";
    }

    public sealed class Padding : FieldLayoutBase
    {
        public Padding(int size, int offset) : base(size, offset)
        {
        }

        protected override string NameOrDescription => "padding";
    }
}