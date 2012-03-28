using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xna.Csg;
using Microsoft.Xna.Framework;

namespace CsgTests
{
    [TestClass]
    public class PolygonTest
    {
        [TestMethod]
        public void NormalWinding()
        {
            Vertex a = new Vertex(new Vector3(0, 0, 0), Vector3.Zero);
            Vertex b = new Vertex(new Vector3(0, 1, 0), Vector3.Zero);
            Vertex c = new Vertex(new Vector3(1, 0, 0), Vector3.Zero);

            Polygon abc = new Polygon(a, b, c);
            Polygon cba = new Polygon(c, b, a);

            Vector3 abcNormal = Vector3.Cross(b.Position - a.Position, c.Position - a.Position);
            Assert.AreEqual(abcNormal, abc.Plane.Normal);

            Vector3 cbaNormal = Vector3.Cross(b.Position - c.Position, a.Position - c.Position);
            Assert.AreEqual(cbaNormal, cba.Plane.Normal);

            Assert.AreEqual(abc.Plane, cba.Flip().Plane);
            Assert.AreNotEqual(abc.Plane, cba.Plane);
        }

        [TestMethod]
        public void ClonePolygon()
        {
            Vertex a = new Vertex(new Vector3(0, 0, 0), Vector3.Zero);
            Vertex b = new Vertex(new Vector3(0, 1, 0), Vector3.Zero);
            Vertex c = new Vertex(new Vector3(1, 0, 0), Vector3.Zero);

            Polygon abc = new Polygon(a, b, c);
            Polygon abc2 = abc.Clone();

            Assert.AreEqual(abc.Plane, abc2.Plane);
        }
    }
}
