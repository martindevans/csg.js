using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xna.Csg.Primitives
{
    public class Sphere
        : BSP
    {
        public Sphere(uint subdivisions = 0, Func<Vector3, Vector3, Vertex> vertexFactory = null)
            : base(Subdivide(CreatePolygons(vertexFactory), subdivisions, vertexFactory), new BoundingBox(new Vector3(-1f), new Vector3(1f)), new object[] { "sphere", subdivisions })
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
                new Vector3(-0.850651082f, -0.52573067f,0)
        };
        static int[][] icosahedronIndices = new int[][] {
                new[] { 0, 6, 1 },
                new[] { 0, 11, 6},
                new[] { 1, 4, 0},
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

        private static IEnumerable<Polygon> CreatePolygons(Func<Vector3, Vector3, Vertex> vertexFactory = null)
        {
            vertexFactory = vertexFactory ?? ((p, n) => new Vertex(p, n));

            var vertices = icosahedronVertices.Select(a => vertexFactory(a, a)).ToArray();

            return icosahedronIndices.Select(a => new Polygon(a.Select(i => vertices[i])));
        }

        private static IEnumerable<Polygon> Subdivide(IEnumerable<Polygon> polygons, uint subdivisions, Func<Vector3, Vector3, Vertex> vertexFactory = null)
        {
            vertexFactory = vertexFactory ?? ((p, n) => new Vertex(p, n));

            for (int i = 0; i < subdivisions; i++)
            {
                polygons = Subdivide(polygons, vertexFactory).ToArray();
            }

            return polygons;
        }

        private static IEnumerable<Polygon> Subdivide(IEnumerable<Polygon> polygons, Func<Vector3, Vector3, Vertex> vertexFactory)
        {
            return polygons.AsParallel().SelectMany(a => Subdivide(a, vertexFactory));
        }

        private static IEnumerable<Polygon> Subdivide(Polygon polygon, Func<Vector3, Vector3, Vertex> vertexFactory)
        {
            Vertex abMid = polygon.Vertices[0].Interpolate(polygon.Vertices[1], 0.5f);
            Vertex bcMid = polygon.Vertices[1].Interpolate(polygon.Vertices[2], 0.5f);
            Vertex caMid = polygon.Vertices[2].Interpolate(polygon.Vertices[0], 0.5f);

            abMid.Position.Normalize();
            bcMid.Position.Normalize();
            caMid.Position.Normalize();

            yield return new Polygon(polygon.Vertices[0], abMid, caMid);
            yield return new Polygon(abMid, polygon.Vertices[1], bcMid);
            yield return new Polygon(bcMid, polygon.Vertices[2], caMid);
            yield return new Polygon(abMid, bcMid, caMid);
        }
    }
}
