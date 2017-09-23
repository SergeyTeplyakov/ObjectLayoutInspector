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

#pragma warning disable 169 // unused fields
        class Base
        {
            private object o;
        }

        class Derived : Base
        {
            private object o;
#pragma warning restore 169 // unused fields
        }

        [Test]
        public void PrivateBaseMembersShouldBeIncluded()
        {
            var offsets = InspectorHelper.GetFieldOffsets(typeof(Derived));
            Assert.That(offsets.Length, Is.EqualTo(2));
        }
    }
}