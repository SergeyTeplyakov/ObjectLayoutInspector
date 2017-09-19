using System;
using System.Linq;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    class ByteAndInt
    {
        public byte b { get; }
        public int n;
    }

    [TestFixture]
    public class RawLayoutTests
    {
        [Test]
        public void TestGetFieldOffsets()
        {
Console.WriteLine(
    string.Join("\r\n",
        InspectorHelper.GetFieldOffsets(typeof(ByteAndInt))
            .Select(tpl => $"Field {tpl.fieldInfo.Name}: starts at offset {tpl.offset}"))
    );
        }
    }
}