using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Xna.Csg.Primitives;

namespace CsgTests
{
    [TestClass]
    public class PrismTest
    {
        [TestMethod]
        public void MeasurePrismBounds()
        {
            Prism c = new Prism(1, new Vector2[]
            {   
                new Vector2(-1, 1),
                new Vector2(1, 1),
                new Vector2(0, 0),
            });

            foreach (var v in c.Polygons.SelectMany(a => a.Vertices))
            {
                Debug.Assert(c.Bounds != null, "c.Bounds != null");
                Assert.IsTrue(c.Bounds.Value.Contains(v.Position) != ContainmentType.Disjoint);
            }
        }
    }
}
