using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xna.Csg.Primitives
{
    public class Cylinder
        : Prism
    {
        public Cylinder(uint rotations)
            : this(rotations, 0, 1, new object[] { "cylinder", rotations })
        {

        }

        internal Cylinder(uint rotations, float initialAngle, float radius, object[] description)
            : base(1, description, CalculatePoints(rotations + 2, initialAngle, radius))
        {

        }

        private static Vector2[] CalculatePoints(uint rotations, float initialAngle, float radius)
        {
            Vector2[] positions = new Vector2[rotations];

            for (int i = 0; i < rotations; i++)
            {
                float angle = MathHelper.TwoPi / rotations * i + initialAngle;
                positions[i] = new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle)) * radius;
            }

            return positions;
        }
    }
}
