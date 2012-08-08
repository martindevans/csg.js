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
        public Plane Plane { get; private set; }

        private Vertex[] _vertices;
        public Vertex[] Vertices
        {
            get { return _vertices; }
            set
            {
                _vertices = value;
                CalculatePlane();
            }
        }

        public Polygon(Vertex a, Vertex b, Vertex c)
            :this(new[] { a, b, c })
        {
        }

        public Polygon(IEnumerable<Vertex> vertices)
        {
            Vertices = vertices.GroupBy(a => a.Position).Select(a => a.First()).ToArray();

            if (Vertices.Length < 3)
                throw new ArgumentException("Degenerate polygon");
        }

        private void CalculatePlane()
        {
            Plane = new Plane(Vertices[0].Position, Vertices[1].Position, Vertices[2].Position);

            for (int i = 0; i < Vertices.Length; i++)
                Vertices[i].Normal = Plane.Normal;
        }

        public void Flip()
        {
            foreach (var vertex in Vertices)
                vertex.Flip();
            Vertices = _vertices.Reverse().ToArray();
        }

        public Polygon Clone()
        {
            return Clone(null);
        }

        public Polygon Clone(Func<Vertex, Vertex> clone = null)
        {
            clone = clone ?? (a => a.Clone());

            return new Polygon(Vertices.Select(a => clone(a)));
        }

        public static bool IsDegenerateSet(IEnumerable<Vertex> set)
        {
            return set.GroupBy(a => a.Position).Select(a => a.First()).Count() < 3;
        }
    }
}
