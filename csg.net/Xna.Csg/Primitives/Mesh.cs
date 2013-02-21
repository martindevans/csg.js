using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Xna.Csg.Primitives
{
    public class Mesh
        : BSP
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="vertexFactory"></param>
        /// <remarks>BSP Meshes are wound CLOCKWISE</remarks>
        public Mesh(Vector3[] vertices, int[] indices, Func<int, Vector3, Vertex> vertexFactory)
            :base(CreatePolygons(vertices, indices, vertexFactory), vertices.Bounds(), null, false)
        {
        }

        private static IEnumerable<Polygon> CreatePolygons(Vector3[] points, int[] indices, Func<int, Vector3, Vertex> vertexFactory)
        {
            Vertex[] vertices = points.Select((v, i) => vertexFactory(i, v)).ToArray();

            for (int i = 0; i < indices.Length; i += 3)
            {
                yield return new Polygon(
                    vertices[indices[i]],
                    vertices[indices[i + 1]],
                    vertices[indices[i + 2]]
                );
            }
        }
    }
}
