using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xna.Csg
{
    public class Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;

        public Vertex(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = Vector3.Normalize(normal);
        }

        public void Flip()
        {
            Normal = -Normal;
        }

        public virtual Vertex Interpolate(Vertex other, float t)
        {
            return new Vertex(Vector3.Lerp(Position, other.Position, t), Vector3.Lerp(Normal, other.Normal, t));
        }

        public override string ToString()
        {
            return Position + ", N=" + Normal;
        }

        public virtual Vertex Clone()
        {
            return new Vertex(Position, Normal);
        }

        public Vertex Transform(Matrix transformation)
        {
            Vector3.Transform(ref Position, ref transformation, out Position);
            Vector3.TransformNormal(ref Normal, ref transformation, out Normal);

            return this;
        }
    }
}
