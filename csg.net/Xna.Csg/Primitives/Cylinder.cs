using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xna.Csg.Primitives
{
    public class Cylinder
        : BSP
    {
        public Cylinder(int rotations)
            : base(CreatePolygons(rotations + 2), new BoundingBox(new Vector3(-1f), new Vector3(1f)), new object[] { "cylinder", rotations })
        {

        }

        private static IEnumerable<Polygon> CreatePolygons(int rotations)
        {
            Vector2[] positions = new Vector2[rotations];

            for (int i = 0; i < rotations; i++)
            {
                float angle = MathHelper.TwoPi / rotations * i;
                positions[i] = new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle));
            }

            //top circle
            var top = new Polygon(positions.Select(a => new Vector3(a.X, 0.5f, a.Y)).Select(a => new Vertex(a, Vector3.Zero)));
            top.CalculateVertexNormals();
            yield return top;

            //bottom circle
            var bottom = new Polygon(positions.Reverse().Select(a => new Vector3(a.X, -0.5f, a.Y)).Select(a => new Vertex(a, Vector3.Zero)));
            bottom.CalculateVertexNormals();
            yield return bottom;

            //sides
            for (int i = 0; i < rotations; i++)
            {
                Vector2 pos1 = positions[(i + 1) % rotations];
                Vector2 pos2 = positions[i];

                var poly = new Polygon(new Vertex[] {                                  
                    new Vertex(new Vector3(pos2.X, 0.5f, pos2.Y), Vector3.Zero),
                    new Vertex(new Vector3(pos2.X, -0.5f, pos2.Y), Vector3.Zero),
                    new Vertex(new Vector3(pos1.X, -0.5f, pos1.Y), Vector3.Zero),
                    new Vertex(new Vector3(pos1.X, 0.5f, pos1.Y), Vector3.Zero),
                });
                poly.CalculateVertexNormals();

                yield return poly;
            }
        }
    }
}
