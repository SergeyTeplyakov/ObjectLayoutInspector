using NUnit.Framework;
using NUnit.Framework.Internal;
using ObjectLayoutInspector.Tests.Objects;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class ClassLayoutTests
    {
        [Test]
        public void Print_NotAlignedClass()
        {
            TypeLayout.PrintLayout<NotAlignedClass>();
        }
    }
}