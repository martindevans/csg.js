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
        public Mesh(Vector3[] vertices, int[][] indices, Func<int, Vector3, Vertex> vertexFactory)
            :base(CreatePolygons(vertices, indices, vertexFactory), vertices.Bounds(), CreateDescription(vertices, indices), true)
        {
        }

        private static IEnumerable<Polygon> CreatePolygons(Vector3[] points, int[][] indices, Func<int, Vector3, Vertex> vertexFactory)
        {
            Vertex[] vertices = points.Select((v, i) => vertexFactory(i, v)).ToArray();

            for (int i = 0; i < indices.Length; i += 3)
                yield return new Polygon(indices[i].Select(index => vertices[index]));
        }

        private static object[] CreateDescription(Vector3[] points, int[][] indices)
        {
            object[] description = new object[3 + points.Length * 3 + indices.Length + indices.Select(a => a.Length).Sum()];

            int i = 0;
            description[i++] = "mesh";
            description[i++] = points.Length;

            for (int p = 0; p < points.Length; p++)
            {
                description[i++] = points[p].X;
                description[i++] = points[p].Y;
                description[i++] = points[p].Z;
            }

            description[i++] = indices.Length;

            for (int p = 0; p < indices.Length; p++)
            {
                var polygon = indices[p];
                description[i++] = polygon.Length;
                for (int j = 0; j < polygon.Length; j++)
                    description[i++] = polygon[j];
            }

            return description;
        }
    }
}
