using System.Reflection;

namespace ObjectLayoutInspector
{
    public abstract class FieldLayoutBase
    {
        public int Size { get; }
        public int Padding { get; }
        public int Offset { get; }

        protected FieldLayoutBase(int offset, int size)
        {
            Offset = offset;
            Size = size;
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
            : base(offset, InspectorHelper.GetFieldSize(fieldInfo.FieldType))
        {
            FieldInfo = fieldInfo;
        }
        
        public FieldLayout(int offset, FieldInfo fieldInfo, int size)
            : base(offset, size)
        {
            FieldInfo = fieldInfo;
        }

        public FieldInfo FieldInfo { get; }

        protected override string NameOrDescription =>
            $"{FieldInfo.FieldType.Name} {FieldInfo.Name}";
    }

    public sealed class Padding : FieldLayoutBase
    {
        public Padding(int offset, int size) : base(offset, size)
        {
        }

        protected override string NameOrDescription => "padding";
    }
}