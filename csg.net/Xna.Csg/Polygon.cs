using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Xna.Csg
{
    [Flags]
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
            :this(new[] { a, b, c })
        {
        }

        public Polygon(IEnumerable<Vertex> vertices)
        {
            Vertices = vertices.GroupBy(a => a.Position).Select(a => a.First()).ToArray();

            if (Vertices.Length < 3)
                throw new ArgumentException("Degenerate polygon");

            Plane = new Plane(Vertices[0].Position, Vertices[1].Position, Vertices[2].Position);
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

        public static bool IsDegenerateSet(IEnumerable<Vertex> set)
        {
            return set.GroupBy(a => a.Position).Select(a => a.First()).Count() < 3;
        }
    }
}
