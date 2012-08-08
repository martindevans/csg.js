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

        [TestMethod]
        public void CreateSphereWithFactory()
        {
            Sphere s = new Sphere(2, (a, b) => new VertexTest(a, b));

            Assert.IsTrue(s.Polygons.SelectMany(a => a.Vertices).All(a => a is VertexTest));
        }

        private class VertexTest
            : Vertex
        {
            public VertexTest(Vector3 a, Vector3 b)
                : base(a, b)
            {

            }

            public override Vertex Interpolate(Vertex other, float t)
            {
                var b = base.Interpolate(other, t);
                return new VertexTest(b.Position, b.Normal);
            }
        }
    }
}
