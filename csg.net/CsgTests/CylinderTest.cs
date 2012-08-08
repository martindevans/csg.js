using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Xna.Csg;
using Xna.Csg.Primitives;

namespace CsgTests
{
    /// <summary>
    /// Summary description for CylinderTest
    /// </summary>
    [TestClass]
    public class CylinderTest
    {
        [TestMethod]
        public void CreateCylinderWithFactory()
        {
            Cylinder s = new Cylinder(7, (a, b) => new VertexTest(a, b));

            Assert.IsTrue(s.Polygons.SelectMany(a => a.Vertices).All(a => a is VertexTest));
        }

        private class VertexTest
            : Vertex
        {
            public VertexTest(Vector3 a, Vector3 b)
                : base(a, b)
            {

            }
        }
    }
}
