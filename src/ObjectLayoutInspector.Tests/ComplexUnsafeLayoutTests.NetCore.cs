using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    // .NET Core (8+): the runtime reorders Sequential struct fields (references before value types),
    // shifting the Guid fields in the Explicit union and introducing a trailing padding entry (8 items total).
    public partial class ComplexUnsafeLayoutTests
    {
        [Test]
        public void MASTER_STRUCTFieldsRecursive()
        {
            var structLayout = UnsafeLayout.GetLayout<MASTER_STRUCT>(recursive: true, new List<Type> { typeof(Guid) });
            Assert.AreEqual(8, structLayout.Count());
        }
    }
}
