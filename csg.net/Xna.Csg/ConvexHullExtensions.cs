using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Xna.Csg
{
    public static class ConvexHullExtensions
    {
        /// <summary>
        /// Given a set of points, performs the quickhull algorithm on them to determine a convex set
        /// </summary>
        /// <param name="points">An unordered set of points in 2D space</param>
        /// <returns>The points in order of the convex hull</returns>
        public static IEnumerable<Vector2> Quickhull(this IEnumerable<Vector2> points)
        {
            Vector2 min = points.Aggregate((a, b) => a.X < b.X ? a : b);
            Vector2 max = points.Aggregate((a, b) => a.X > b.X ? a : b);
            Vector2 dir = Vector2.Normalize(max - min);

            List<Vector2> above = new List<Vector2>();
            List<Vector2> below = new List<Vector2>();
            foreach (var point in points)
            {
                if (point == min || point == max)
                    continue;

                if (IsLeftOfLine(point, min, dir))
                    above.Add(point);
                else
                    below.Add(point);
            }

            yield return min;

            foreach (var point in Quickhull(above, min, dir, max))
                yield return point;

            yield return max;

            foreach (var point in Quickhull(below, min, dir, max))
                yield return point;
        }

        private static IEnumerable<Vector2> Quickhull(IEnumerable<Vector2> points, Vector2 baseLineAnchor, Vector2 baseLineDirection, Vector2 baseLineEnd)
        {
            if (!points.Any())
                yield break;

            float distance = float.MinValue;
            Vector2 furthest = Vector2.Zero;
            foreach (var item in points)
            {
                var dist = DistanceFromPointToLine(item, baseLineAnchor, baseLineDirection);
                if (dist > distance)
                {
                    distance = dist;
                    furthest = item;
                }
            }

            var leftOuterPointers = new List<Vector2>();
            var rightOuterPoints = new List<Vector2>();

            var leftDir = Vector2.Normalize(furthest - baseLineAnchor);
            var rightDir = Vector2.Normalize(baseLineEnd - furthest);

            foreach (var item in points)
            {
                if (item == furthest)
                    continue;
                if (IsLeftOfLine(item, baseLineAnchor, leftDir))
                    leftOuterPointers.Add(item);
                else if (IsLeftOfLine(item, furthest, rightDir)) //Left of a downwards pointing line is the right
                    rightOuterPoints.Add(item);
            }

            foreach (var item in Quickhull(leftOuterPointers, baseLineAnchor, leftDir, furthest))
                yield return item;

            yield return furthest;

            foreach (var item in Quickhull(rightOuterPoints, furthest, rightDir, baseLineEnd))
                yield return item;
        }

        public static float DistanceFromPointToLine(Vector2 point, Vector2 anchor, Vector2 direction)
        {
            Vector2 ap = (anchor - point);
            float apDotDir = Vector2.Dot(ap, direction);

            var result = (ap - apDotDir * direction).Length();
            return result;
        }

        public static bool IsLeftOfLine(Vector2 point, Vector2 anchor, Vector2 direction)
        {
            return direction.X * (point.Y - anchor.Y) - direction.Y * (point.X - anchor.X) > 0;
        }

        public static float ConvexHullArea(this IEnumerable<Vector2> hull, bool preserveWindingSign = false)
        {
            //http://www.mathopenref.com/coordpolygonarea.html

            float area = hull.Zip(hull.Skip(1).Append(hull), (a, b) =>
                a.X * b.Y - a.Y * b.X
            ).Aggregate((a, b) =>
                    a + b
            ) / 2;

            if (preserveWindingSign)
                return area;
            return Math.Abs(area);
        }
    }
}
