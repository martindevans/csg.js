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
    public class SphereTest
    {
        [TestMethod]
        public void CreateSphere()
        {
            Sphere s = new Sphere(0);

            //Since radius is 1, position and normal should be the same
            foreach (var vertex in s.Polygons.SelectMany(a => a.Vertices))
            {
                Assert.AreEqual(vertex.Position.X, vertex.Normal.X, 0.0001f);
                Assert.AreEqual(vertex.Position.Y, vertex.Normal.Y, 0.0001f);
                Assert.AreEqual(vertex.Position.Z, vertex.Normal.Z, 0.0001f);
            }

            foreach (var polygon in s.Polygons)
            {
                Assert.IsTrue(polygon.Plane.D < 0);
            }
        }
    }
}
