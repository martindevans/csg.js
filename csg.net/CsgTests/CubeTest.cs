using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xna.Csg.Primitives;

namespace CsgTests
{
    [TestClass]
    public class CubeTest
    {
        [TestMethod]
        public void CreateCube()
        {
            Cube c = new Cube();

            Assert.AreEqual(6, c.Polygons.Count());
            Assert.AreEqual(24, c.Polygons.SelectMany(p => p.Vertices).GroupBy(a => a).Count());

            foreach (var face in c.Polygons)
            {
                foreach (var vertex in face.Vertices)
                {
                    Assert.AreEqual(face.Plane.Normal, vertex.Normal);
                }
            }
        }
    }
}
