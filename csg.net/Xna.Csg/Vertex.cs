using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xna.Csg
{
    public struct Vertex
    {
        public readonly Vector3 Position;
        public readonly Vector3 Normal;

        public Vertex(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }

        public Vertex Flip()
        {
            return new Vertex(Position, -Normal);
        }

        public Vertex Interpolate(Vertex other, float t)
        {
            return new Vertex(Vector3.Lerp(Position, other.Position, t), Vector3.Lerp(Normal, other.Normal, t));
        }
    }
}
