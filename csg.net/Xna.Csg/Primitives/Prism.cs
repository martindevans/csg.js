using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xna.Csg.Primitives
{
    public class Prism
        :BSP
    {
        public float Height { get; private set; }
        public IEnumerable<Vector2> Footprint { get; private set; }

        public Prism(float height, params Vector2[] points)
            :base(CreatePolygons(height, points), MeasureBounds(height, points), CreateDescription(height, points))
        {
            Height = height;
            Footprint = points;
        }

        internal Prism(float height, object[] description, params Vector2[] points)
            :base(CreatePolygons(height, points), MeasureBounds(height, points), description)
        {
            Height = height;
            Footprint = points;
        }

        internal static IEnumerable<Polygon> CreatePolygons(float height, Vector2[] points)
        {
            //top circle
            var top = new Polygon(points.Select(a => new Vector3(a.X, height / 2, a.Y)).Select(a => new Vertex(a, Vector3.Zero)));
            top.CalculateVertexNormals();

            if (top.Plane.D > 0)
                throw new ArgumentException("Prism wound wrong way");

            yield return top;

            //bottom circle
            var bottom = new Polygon(points.Reverse().Select(a => new Vector3(a.X, -height / 2, a.Y)).Select(a => new Vertex(a, Vector3.Zero)));
            bottom.CalculateVertexNormals();
            yield return bottom;

            //sides
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 pos1 = points[(i + 1) % points.Length];
                Vector2 pos2 = points[i];

                var poly = new Polygon(new Vertex[] {                                  
                    new Vertex(new Vector3(pos2.X, height / 2, pos2.Y), Vector3.Zero),
                    new Vertex(new Vector3(pos2.X, -height / 2, pos2.Y), Vector3.Zero),
                    new Vertex(new Vector3(pos1.X, -height / 2, pos1.Y), Vector3.Zero),
                    new Vertex(new Vector3(pos1.X, height / 2, pos1.Y), Vector3.Zero),
                });
                poly.CalculateVertexNormals();

                yield return poly;
            }
        }

        private static BoundingBox MeasureBounds(float height, Vector2[] points)
        {
            Vector2 min = new Vector2(points.Min(a => a.X), points.Min(a => a.Y));
            Vector2 max = new Vector2(points.Max(a => a.X), points.Max(a => a.Y));

            return new BoundingBox(new Vector3(min.X, -height / 2, min.Y), new Vector3(max.X, height / 2, max.Y));
        }

        private static object[] CreateDescription(float height, Vector2[] points)
        {
            object[] description = new object[3 + points.Length * 2];

            description[0] = "prism";
            description[1] = height;
            description[2] = (uint)points.Length;

            for (int i = 0; i < points.Length; i++)
            {
                description[3 + i * 2] = points[i].X;
                description[3 + i * 2 + 1] = points[i].Y;
            }

            return description;
        }
    }
}
