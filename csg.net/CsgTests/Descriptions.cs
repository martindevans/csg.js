using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xna.Csg.Primitives;
using Xna.Csg;
using Microsoft.Xna.Framework;

namespace CsgTests
{
    [TestClass]
    public class Descriptions
    {
        [TestMethod]
        public void CubeDescription()
        {
            Cube c = new Cube();

            Assert.AreEqual(1, c.Description.Count());
        }

        [TestMethod]
        public void SphereDescription()
        {
            Sphere s = new Sphere(1);

            Assert.AreEqual(2, s.Description.Count());
            Assert.AreEqual("sphere", s.Description.First());
            var v = s.Description.Skip(1).First();
            Assert.AreEqual((uint)1, v);
        }

        [TestMethod]
        public void CylinderDescription()
        {
            Cylinder c = new Cylinder(3);

            Assert.AreEqual(2, c.Description.Count());
            Assert.AreEqual("cylinder", c.Description.First());
            Assert.AreEqual((uint)3, c.Description.Skip(1).First());
        }

        [TestMethod]
        public void PrismDescription()
        {
            Prism c = new Prism(3, new Vector2[]
            {                
                new Vector2(-1, 17),
                new Vector2(3, 2),
                new Vector2(0, 10),
            });

            Assert.AreEqual(10, c.Description.Count());
            Assert.AreEqual("prism", c.Description.First());
            Assert.AreEqual((float)3, c.Description.Skip(1).First());
            Assert.AreEqual((uint)3, c.Description.Skip(2).First());

            Assert.AreEqual((float)-1, c.Description.Skip(3).First());
            Assert.AreEqual((float)17, c.Description.Skip(4).First());
            Assert.AreEqual((float)3, c.Description.Skip(5).First());
            Assert.AreEqual((float)2, c.Description.Skip(6).First());
            Assert.AreEqual((float)0, c.Description.Skip(7).First());
            Assert.AreEqual((float)10, c.Description.Skip(8).First());
        }

        [TestMethod]
        public void CloneCubeDescription()
        {
            BSP cube = new Cube();
            BSP cube2 = cube.Clone();
            Assert.IsTrue(cube.Description.Zip(cube2.Description, (a, b) => a.Equals(b)).Aggregate((a, b) => a & b));

            BSP sphere = new Sphere(1);
            Assert.IsFalse(sphere.Description.Zip(cube2.Description, (a, b) => a.Equals(b)).Aggregate((a, b) => a & b));
        }

        [TestMethod]
        public void CloneCylinderDescription()
        {
            BSP cyl = new Cylinder(3);
            BSP cyl2 = cyl.Clone();
            Assert.AreEqual(cyl.Description.Count(), cyl2.Description.Count());
            Assert.IsTrue(cyl.Description.Zip(cyl2.Description, (a, b) => a.Equals(b)).Aggregate((a, b) => a & b));
        }

        [TestMethod]
        public void UnionDescription()
        {
            Cube cube = new Cube();
            Cylinder cyl = new Cylinder(3);

            var shape = cube.Clone();
            shape.Union(cyl);

            var desc = shape.Description.ToArray();
            Assert.AreEqual(4, desc.Length);
            Assert.AreEqual("union", desc[0]);
            Assert.AreEqual("cube", desc[1]);
            Assert.AreEqual("cylinder", desc[2]);
            Assert.AreEqual((uint)3, desc[3]);
        }

        [TestMethod]
        public void IntersectionDescription()
        {
            Cube cube = new Cube();
            Cylinder cyl = new Cylinder(3);

            var shape = cube.Clone();
            shape.Intersect(cyl);

            var desc = shape.Description.ToArray();
            Assert.AreEqual(4, desc.Length);
            Assert.AreEqual("intersect", desc[0]);
            Assert.AreEqual("cube", desc[1]);
            Assert.AreEqual("cylinder", desc[2]);
            Assert.AreEqual((uint)3, desc[3]);
        }

        [TestMethod]
        public void SubtractDescription()
        {
            Cube cube = new Cube();
            Cylinder cyl = new Cylinder(3);

            var shape = cube.Clone();
            shape.Subtract(cyl);

            var desc = shape.Description.ToArray();
            Assert.AreEqual(4, desc.Length);
            Assert.AreEqual("subtract", desc[0]);
            Assert.AreEqual("cube", desc[1]);
            Assert.AreEqual("cylinder", desc[2]);
            Assert.AreEqual((uint)3, desc[3]);
        }
    }
}
