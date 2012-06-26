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

            foreach (var polygon in s.Polygons)
            {
                Assert.IsTrue(polygon.Plane.D < 0);
            }
        }
    }
}
