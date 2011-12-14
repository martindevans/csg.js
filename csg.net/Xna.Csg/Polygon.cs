using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xna.Csg
{
    public enum PolygonType
    {
        Coplanar = 0,
        Front = 1,
        Back = 2,
        Spanning = 3
    }

    public class Polygon
    {
        public readonly Plane Plane;

        public Vertex[] Vertices;

        public Polygon(Vertex a, Vertex b, Vertex c)
        {
            Plane = new Plane(a.Position, b.Position, c.Position);

            Vertices = new[] { a, b, c };
        }

        public Polygon(IEnumerable<Vertex> vertices)
        {
            Vertices = vertices.ToArray();

            Plane = new Plane(vertices.First().Position, vertices.Skip(1).First().Position, vertices.Skip(2).First().Position);
        }

        public void CalculateVertexNormals()
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Normal = Plane.Normal;
            }
        }

        public Polygon Flip()
        {
            return new Polygon(Vertices.Select(a => a.Flip()).Reverse());
        }

        public Polygon Clone()
        {
            return new Polygon(Vertices);
        }
    }
}
