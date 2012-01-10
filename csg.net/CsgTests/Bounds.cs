using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xna.Csg;
using Xna.Csg.Primitives;
using Microsoft.Xna.Framework;

namespace CsgTests
{
    [TestClass]
    public class Bounds
    {
        [TestMethod]
        public void ConstructBounds()
        {
            BoundedBsp a = new Cube();

            Assert.IsTrue(a.Bounds.HasValue);
            Assert.AreEqual(new BoundingBox(new Vector3(-0.5f), new Vector3(0.5f)), a.Bounds.Value);
        }

        [TestMethod]
        public void TransformBounds()
        {
            BoundedBsp a = new Cube().Transform(Matrix.CreateTranslation(0.1f, 0, 0)) as BoundedBsp;

            Assert.IsTrue(a.Bounds.HasValue);
            Assert.AreEqual(new BoundingBox(new Vector3(-0.4f, -0.5f, -0.5f), new Vector3(0.6f, 0.5f, 0.5f)), a.Bounds.Value);
        }

        [TestMethod]
        public void UnionBounds()
        {
            BoundedBsp a = new Cube();
            BoundedBsp b = new Cube().Transform(Matrix.CreateTranslation(0.1f, 0, 0)) as BoundedBsp;

            a.Union(b);

            Assert.IsTrue(a.Bounds.HasValue);
            Assert.AreEqual(new BoundingBox(new Vector3(-0.5f), new Vector3(0.6f, 0.5f, 0.5f)), a.Bounds.Value);
        }
    }
}
