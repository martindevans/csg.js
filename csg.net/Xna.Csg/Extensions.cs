using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Xna.Csg
{
    public static class Extensions
    {
        public const float Epsilon = 1e-4f;

        public static void SplitPolygon(this Plane plane, Polygon polygon, IList<Polygon> coPlanarFront, IList<Polygon> coPlanarBack, IList<Polygon> front, IList<Polygon> back)
        {
            List<PolygonType> types = new List<PolygonType>();
            PolygonType polygonType = 0;

            for (int i = 0; i < polygon.Vertices.Length; i++)
            {
                var t = polygon.Vertices[i].Position.Distance(plane);

                var type = (t < -Epsilon) ? PolygonType.Back : (t > Epsilon) ? PolygonType.Front : PolygonType.Coplanar;
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

                    if (f.Count >= 3 && !Polygon.IsDegenerateSet(f)) front.Add(new Polygon(f));
                    if (b.Count >= 3 && !Polygon.IsDegenerateSet(b)) back.Add(new Polygon(b));

                    break;
            }
        }

        public static void ToTriangleList<V, I>(this ICsgProvider tree, Func<Vector3, Vector3, V> positionNormalToVertex, Func<V, I> insertVertex, Action<I, I, I> createTriangle)
        {
            foreach (var polygon in tree.Polygons)
            {
                var indices = polygon.Vertices.Select(a => positionNormalToVertex(a.Position, a.Normal)).Select(a => insertVertex(a)).ToArray();

                //Triangulate out from one corner
                for (int i = 2; i < indices.Length; i++)
                {
                    createTriangle(indices[0], indices[i - 1], indices[i]);
                }

                ////alternating direction triangles
                //int topIndex = indices.Length - 1;
                //int bottomIndex = 0;
                //while (topIndex > bottomIndex + 1)
                //{
                //    createTriangle(indices[topIndex], indices[bottomIndex], indices[bottomIndex + 1]);

                //    if (topIndex - 1 != bottomIndex + 1)
                //        createTriangle(indices[topIndex], indices[bottomIndex + 1], indices[topIndex - 1]);
                //    else
                //        break;

                //    topIndex--;
                //    bottomIndex++;
                //}
            }
        }

        public static void ToListLine<V, I>(this ICsgProvider tree, Func<Vector3, Vector3, V> positionNormalToVertex, Func<V, I> insertVertex, Action<I, I> createLine)
        {
            foreach (var polygon in tree.Polygons)
            {
                var indices = polygon.Vertices.Select(a => positionNormalToVertex(a.Position, a.Normal)).Select(a => insertVertex(a)).ToArray();

                for (int i = 0; i < indices.Length; i++)
			    {
			        createLine(indices[i], indices[(i + 1) % indices.Length]);
			    }
            }
        }

        public static bool IsEmpty<T>(this IEnumerable<T> e)
        {
            foreach (var item in e)
            {
                return false;
            }

            return true;
        }

        public static float? Intersects(this Ray ray, BSP bsp)
        {
            return bsp.RayCast(ray);
        }

        public static float Distance(this Vector3 point, Plane plane)
        {
            float dot = Vector3.Dot(plane.Normal, point);
            float value = dot + plane.D;
            return value;
        }

        public static BoundingBox IncludePoint(this BoundingBox bound, Vector3 point)
        {
            bound.Min.X = Math.Min(bound.Min.X, point.X);
            bound.Min.Y = Math.Min(bound.Min.Y, point.Y);
            bound.Min.Z = Math.Min(bound.Min.Z, point.Z);

            bound.Max.X = Math.Max(bound.Max.X, point.X);
            bound.Max.Y = Math.Max(bound.Max.Y, point.Y);
            bound.Max.Z = Math.Max(bound.Max.Z, point.Z);

            return bound;
        }

        public static BoundingBox Transform(this BoundingBox bound, Matrix transform)
        {
            var points = new Vector3[8]
            {
                bound.Min,
                new Vector3(bound.Min.X, bound.Min.Y, bound.Max.Z),
                new Vector3(bound.Min.X, bound.Max.Y, bound.Min.Z),
                new Vector3(bound.Min.X, bound.Max.Y, bound.Max.Z),

                bound.Max,
                new Vector3(bound.Max.X, bound.Min.Y, bound.Max.Z),
                new Vector3(bound.Max.X, bound.Min.Y, bound.Min.Z),
                new Vector3(bound.Max.X, bound.Min.Y, bound.Min.Z)
            }.Select(a => Vector3.Transform(a, transform));

            BoundingBox b = new BoundingBox(points.First(), points.First());
            foreach (var point in points.Skip(1))
            {
                b = b.IncludePoint(point);
            }

            return b;
        }

        /// <summary>
        /// enumerates the start and then the end
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> start, IEnumerable<T> end)
        {
            foreach (var item in start)
                yield return item;

            foreach (var item in end)
                yield return item;
        }

        /// <summary>
        /// Appends the given items onto this enumeration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> start, params T[] end)
        {
            return Append(start, end as IEnumerable<T>);
        }
    }
}
