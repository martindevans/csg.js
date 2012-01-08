using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xna.Csg
{
    public static class Extensions
    {
        public const float EPSILON = 1e-5f;

        public static void SplitPolygon(this Plane plane, Polygon polygon, IList<Polygon> coPlanarFront, IList<Polygon> coPlanarBack, IList<Polygon> front, IList<Polygon> back)
        {
            List<PolygonType> types = new List<PolygonType>();
            PolygonType polygonType = (PolygonType)0;

            for (int i = 0; i < polygon.Vertices.Length; i++)
            {
                var t = Vector3.Dot(plane.Normal, polygon.Vertices[i].Position) + plane.D;

                var type = (t < -EPSILON) ? PolygonType.Back : (t > EPSILON) ? PolygonType.Front : PolygonType.Coplanar;
                polygonType |= type;
                types.Add(type);
            }

            switch (polygonType)
            {
                case PolygonType.Coplanar:
                    (Vector3.Dot(plane.Normal, polygon.Plane.Normal) > 0 ? coPlanarFront : coPlanarBack).Add(polygon);
                    break;
                case PolygonType.Front:
                    front.Add(polygon);
                    break;
                case PolygonType.Back:
                    back.Add(polygon);
                    break;
                case PolygonType.Spanning:

                    List<Vertex> f = new List<Vertex>();
                    List<Vertex> b = new List<Vertex>();

                    for (var i = 0; i < polygon.Vertices.Length; i++)
                    {
                        var j = (i + 1) % polygon.Vertices.Length;
                        PolygonType ti = types[i];
                        PolygonType tj = types[j];

                        Vertex vi = polygon.Vertices[i];
                        Vertex vj = polygon.Vertices[j];

                        if (ti != PolygonType.Back)
                            f.Add(vi);

                        if (ti != PolygonType.Front)
                            b.Add(vi);

                        if ((ti | tj) == PolygonType.Spanning)
                        {
                            var t = (-plane.D - Vector3.Dot(plane.Normal, vi.Position)) / Vector3.Dot(plane.Normal, vj.Position - vi.Position);
                            var v = vi.Interpolate(vj, t);
                            f.Add(v);
                            b.Add(v);
                        }
                    }

                    if (f.Count >= 3) front.Add(new Polygon(f));
                    if (b.Count >= 3) back.Add(new Polygon(b));

                    break;
            }
        }

        public static void ToTriangleList<V, I>(this BSP tree, Func<Vector3, Vector3, V> positionNormalToVertex, Func<V, I> insertVertex, Action<I, I, I> createTriangle)
        {
            foreach (var polygon in tree.ToPolygons())
            {
                var indices = polygon.Vertices.Select(a => positionNormalToVertex(a.Position, a.Normal)).Select(a => insertVertex(a)).ToArray();

                for (int i = 2; i < indices.Length; i++)
                {
                    createTriangle(indices[0], indices[i - 1], indices[i]);
                }
            }
        }

        public static void ToListLine<V, I>(this BSP tree, Func<Vector3, Vector3, V> positionNormalToVertex, Func<V, I> insertVertex, Action<I, I> createLine)
        {
            foreach (var polygon in tree.ToPolygons())
            {
                var indices = polygon.Vertices.Select(a => positionNormalToVertex(a.Position, a.Normal)).Select(a => insertVertex(a)).ToArray();

                for (int i = 0; i < indices.Length; i++)
			    {
			        createLine(indices[i], indices[(i + 1) % indices.Length]);
			    }
            }
        }
    }
}
