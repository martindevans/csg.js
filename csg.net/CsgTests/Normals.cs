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
    public class Normals
    {
        [TestMethod]
        public void UpwardNormals()
        {
            Polygon p = new Polygon(new[] //Wound anto clockwise
            {
                new Vertex(new Vector3(0,0,0), Vector3.Zero),
                new Vertex(new Vector3(0,0,1), Vector3.Zero),
                new Vertex(new Vector3(1,0,1), Vector3.Zero),
                new Vertex(new Vector3(1,0,0), Vector3.Zero),
            });

            p.CalculateVertexNormals();

            Assert.AreEqual(Vector3.Up, p.Plane.Normal);
        }
    }
}
