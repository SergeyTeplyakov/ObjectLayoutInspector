using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class StructWithExplicitLayoutTests : TestsBase
    {
        [Test]
        public void Print_NotAlignedStruct()
        {
            AssertNonRecursiveWithPadding<NotAlignedStruct>();
            TypeLayout.PrintLayout<NotAlignedStruct>();
        }

        [Test]
        public void Print_StructWithExplicitLayout()
        {
            AssertNonRecursiveWithPadding<StructWithExplicitLayout>();            
            TypeLayout.PrintLayout<StructWithExplicitLayout>();
        }

        [Test]
        public void Print_StructWithExplicitLayoutAndOffsetForFirstField()
        {
            AssertNonRecursiveWithPadding<StructWithExplicitLayoutAndOffsetForFirstField>();  
            TypeLayout.PrintLayout<StructWithExplicitLayoutAndOffsetForFirstField>();
        }
    }
}
