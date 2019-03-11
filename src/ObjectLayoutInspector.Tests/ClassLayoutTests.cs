using NUnit.Framework;
using NUnit.Framework.Internal;

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