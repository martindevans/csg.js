using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xna.Csg.Primitives;
using Microsoft.Xna.Framework;

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
                Assert.IsTrue(c.Bounds.Value.Contains(v.Position) != ContainmentType.Disjoint);
            }
        }
    }
}
