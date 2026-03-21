using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    // .NET Framework 4.8: Sequential struct fields are NOT reordered at runtime,
    // so the Explicit union layout has no trailing padding (7 items total).
    public partial class ComplexUnsafeLayoutTests
    {
        [Test]
        public void MASTER_STRUCTFieldsRecursive()
        {
            var structLayout = UnsafeLayout.GetLayout<MASTER_STRUCT>(recursive: true, new List<Type> { typeof(Guid) });
            Assert.AreEqual(7, structLayout.Count());
        }
    }
}
