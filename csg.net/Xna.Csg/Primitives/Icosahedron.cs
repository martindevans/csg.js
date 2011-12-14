using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xna.Csg.Primitives
{
    public class Icosahedron
        :BSP
    {
        public Icosahedron()
            :base(CreatePolygons())
        {
        }

        static Vector3[] icosahedronVertices = new Vector3[] {
                new Vector3(-0.52573067f, 0, 0.850651082f),
                new Vector3(0.52573067f, 0, 0.850651082f),
                new Vector3(-0.52573067f, 0, -0.850651082f),
                new Vector3(0.52573067f, 0, -0.850651082f),
                new Vector3(0, 0.850651082f, 0.52573067f),
                new Vector3(0, 0.850651082f, -0.52573067f),
                new Vector3(0, -0.850651082f,0.52573067f),
                new Vector3(0, -0.850651082f, -0.52573067f),
                new Vector3(0.850651082f, 0.52573067f,0),
                new Vector3(-0.850651082f, 0.52573067f,0),
                new Vector3(0.850651082f, -0.52573067f,0),
                new Vector3(-0.850651082f, -0.52573067f,0)};
        static int[][] icosahedronIndices = new int[][] {
                new[] { 0, 6, 1 },
                new[] { 0, 11, 6},
                new[] {  1, 4, 0},
                new[] { 1, 8, 4},
                new[] { 1, 10, 8},
                new[] { 2, 5, 3},
                new[] { 2, 9, 5},
                new[] { 2, 11, 9},
                new[] { 3, 7, 2},
                new[] { 3, 10, 7},
                new[] { 4, 8, 5},
                new[] { 4, 9, 0},
                new[] { 5, 8, 3},
                new[] { 5, 9, 4},
                new[] { 6, 10, 1},
                new[] { 6, 11, 7},
                new[] { 7, 10, 6},
                new[] { 7, 11, 2},
                new[] { 8, 10, 3},
                new[] { 9, 11, 0}
        };

        private static IEnumerable<Polygon> CreatePolygons()
        {
            var vertices = icosahedronVertices.Select(a => new Vertex(a, a)).ToArray();

            return icosahedronIndices.Select(a => new Polygon(a.Select(i => vertices[i])));
        }
    }
}
