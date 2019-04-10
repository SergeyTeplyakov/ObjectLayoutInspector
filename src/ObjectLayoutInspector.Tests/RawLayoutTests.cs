using System;
using System.Linq;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class RawLayoutTests
    {
        [Test]
        public void TestGetFieldOffsets()
        {
            Console.WriteLine(
                string.Join("\r\n",
                    TypeInspector.GetFieldOffsets(typeof(ByteAndInt))
                        .Select(tpl => $"Field {tpl.fieldInfo.Name}: starts at offset {tpl.offset}"))
                );
        }

        [Test]
        public void PrivateBaseMembersShouldBeIncluded()
        {
            var offsets = TypeInspector.GetFieldOffsets(typeof(Derived));
            Assert.That(offsets.Length, Is.EqualTo(2));
        }
    }
}