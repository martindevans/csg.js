﻿using System;
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
            BSP a = new Cube();

            Assert.IsTrue(a.Bounds.HasValue);
            //Assert.AreEqual(new BoundingBox(new Vector3(-0.5f), new Vector3(0.5f)), a.Bounds.Value);

            float epsilon = 0.00001f;
            Assert.AreEqual(-0.5f, a.Bounds.Value.Min.X, epsilon);
            Assert.AreEqual(-0.5f, a.Bounds.Value.Min.Y, epsilon);
            Assert.AreEqual(-0.5f, a.Bounds.Value.Min.Z, epsilon);
            Assert.AreEqual(0.5f, a.Bounds.Value.Max.X, epsilon);
            Assert.AreEqual(0.5f, a.Bounds.Value.Max.Y, epsilon);
            Assert.AreEqual(0.5f, a.Bounds.Value.Max.Z, epsilon);
        }

        [TestMethod]
        public void TransformBounds()
        {
            BSP a = new Cube().Transform(Matrix.CreateTranslation(0.1f, 0, 0));

            Assert.IsTrue(a.Bounds.HasValue);

            float epsilon = 0.00001f;
            Assert.AreEqual(-0.4f, a.Bounds.Value.Min.X, epsilon);
            Assert.AreEqual(-0.5f, a.Bounds.Value.Min.Y, epsilon);
            Assert.AreEqual(-0.5f, a.Bounds.Value.Min.Z, epsilon);
            Assert.AreEqual(0.6f, a.Bounds.Value.Max.X, epsilon);
            Assert.AreEqual(0.5f, a.Bounds.Value.Max.Y, epsilon);
            Assert.AreEqual(0.5f, a.Bounds.Value.Max.Z, epsilon);
        }

        [TestMethod]
        public void UnionBounds()
        {
            BSP a = new Cube();
            BSP b = new Cube().Transform(Matrix.CreateTranslation(0.1f, 0, 0));

            a.Union(b);

            Assert.IsTrue(a.Bounds.HasValue);

            float epsilon = 0.00001f;
            Assert.AreEqual(-0.5f, a.Bounds.Value.Min.X, epsilon);
            Assert.AreEqual(-0.5f, a.Bounds.Value.Min.Y, epsilon);
            Assert.AreEqual(-0.5f, a.Bounds.Value.Min.Z, epsilon);
            Assert.AreEqual(0.6f, a.Bounds.Value.Max.X, epsilon);
            Assert.AreEqual(0.5f, a.Bounds.Value.Max.Y, epsilon);
            Assert.AreEqual(0.5f, a.Bounds.Value.Max.Z, epsilon);
        }
    }
}
