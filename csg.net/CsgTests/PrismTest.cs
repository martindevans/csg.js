using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Xna.Csg;
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

        [TestMethod]
        public void CreatePrismWithFactory()
        {
            Prism c = new Prism(10, (a, b) => new VertexTest(a, b), new Vector2[]
            {   
                new Vector2(-1, 1),
                new Vector2(1, 1),
                new Vector2(0, 0),
            });

            Assert.IsTrue(c.Polygons.SelectMany(a => a.Vertices).All(a => a is VertexTest));
        }

        private class VertexTest
            : Vertex
        {
            public VertexTest(Vector3 a, Vector3 b)
                :base(a, b)
            {
                
            }
        }
    }
}
