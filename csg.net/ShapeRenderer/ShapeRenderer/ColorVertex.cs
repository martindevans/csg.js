using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Xna.Csg;

namespace ShapeRenderer
{
    public class ColorVertex
        :Vertex
    {
        public Color Color { get; private set; }

        public ColorVertex(Vector3 position, Vector3 normal, Color color)
            :base(position, normal)
        {
            Color = color;
        }

        public override Vertex Clone()
        {
            return new ColorVertex(Position, Normal, Color);
        }

        public override Vertex Interpolate(Vertex other, float t)
        {
            var pos = Vector3.Lerp(Position, other.Position, t);
            var norm = Vector3.Lerp(Normal, other.Normal, t);

            var colorVertex = other as ColorVertex;
            Color color = colorVertex == null ? Color : Color.Lerp(Color, (colorVertex).Color, t);

            return new ColorVertex(pos, norm, color);
        }
    }
}
