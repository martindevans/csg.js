using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xna.Csg.Primitives
{
    public class Cube
        : Cylinder
    {
        public Cube(Func<Vector3, Vector3, Vertex> vertexFactory = null)
            : base(2, MathHelper.PiOver4, 0.70710678118654752440084436210485f, new object[] { "cube" }, vertexFactory)
        {

        }
    }
}
