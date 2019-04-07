using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class ClassWithExplicitLayoutTests
    {
        [Test]
        public void Print_StructWithExplicitLayout()
        {
            TypeLayout.PrintLayout<ClassWithExplicitLayout>();
        }

        [Test]
        public void Print_StructWithExplicitLayoutAndOffsetForFirstField()
        {
            TypeLayout.PrintLayout<ClassWithExplicitLayoutAndOffsetForFirstField>();
        }
    }
}