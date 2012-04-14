using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Xna.Csg;

namespace CsgTests
{
    [TestClass]
    public class ConvexHull
    {
        [TestMethod]
        public void PointLineDistance()
        {
            Vector2 anchor = Vector2.Zero;
            Vector2 direction = Vector2.UnitY;
            Vector2 point = new Vector2(0, 10);

            Assert.AreEqual(0, ConvexHullExtensions.DistanceFromPointToLine(point, anchor, direction));

            anchor = Vector2.Zero;
            direction = Vector2.UnitY;
            point = new Vector2(10, 0);

            Assert.AreEqual(10, ConvexHullExtensions.DistanceFromPointToLine(point, anchor, direction));
        }

        [TestMethod]
        public void LeftOfLine()
        {
            Vector2 anchor = Vector2.Zero;
            Vector2 direction = Vector2.UnitY;

            Assert.IsFalse(ConvexHullExtensions.IsLeftOfLine(new Vector2(10, 10), anchor, direction));

            Assert.IsTrue(ConvexHullExtensions.IsLeftOfLine(new Vector2(-10, 10), anchor, direction));
        }

        [TestMethod]
        public void Quickhull()
        {
            Vector2[] points = new Vector2[]
            {
                new Vector2(-1, 1),
                new Vector2(1, 1),
                new Vector2(1, -1),
                new Vector2(-1, -1),
                new Vector2(0, 0)
            };

            var hull = points.Quickhull().ToArray();

            Assert.AreEqual(4, hull.Length);
        }
    }
}
