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
        public Cylinder(uint rotations)
            : base(Prism.CreatePolygons(1, CalculatePoints(rotations + 2)), new BoundingBox(new Vector3(-1f), new Vector3(1f)), new object[] { "cylinder", rotations })
        {

        }

        private static Vector2[] CalculatePoints(uint rotations)
        {
            Vector2[] positions = new Vector2[rotations];

            for (int i = 0; i < rotations; i++)
            {
                float angle = MathHelper.TwoPi / rotations * i;
                positions[i] = new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle));
            }

            return positions;
        }
    }
}
