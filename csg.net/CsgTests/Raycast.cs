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
    public class Raycast
    {
        [TestMethod]
        public void CastIntoCube()
        {
            var c = new Cube();

            var f = c.RayCast(new Ray(new Vector3(-10, 0, 0), new Vector3(1, 0, 0)));

            Assert.IsTrue(f.HasValue);
            Assert.AreEqual(9.5, f.Value);

            var f2 = c.RayCast(new Ray(new Vector3(-10, 0, 0), new Vector3(-1, 0, 0)));

            Assert.IsTrue(f.HasValue);
            Assert.AreEqual(9.5, f.Value);
        }

        [TestMethod]
        public void CastIntoHollowCube()
        {
            var c = new Cube().Transform(Matrix.CreateScale(2));
            var c2 = new Cube().Transform(Matrix.CreateScale(2, 1, 1));

            c.Subtract(c2);

            var f = c.RayCast(new Ray(new Vector3(-10, 0, 0), new Vector3(1, 0, 0)));

            Assert.IsFalse(f.HasValue);
        }
    }
}
