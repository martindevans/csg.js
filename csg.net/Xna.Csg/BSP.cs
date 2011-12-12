using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xna.Csg
{
    public class BSP
    {
        Node root;

        public BSP()
        {
            root = new Node();
        }

        public BSP(IEnumerable<Polygon> polygons)
            :this()
        {
            root.Build(polygons);
        }

        public BSP Clone()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Polygon> ToPolygons()
        {
            return root.AllPolygons;
        }

        public BSP Inverse()
        {
            var bsp = Clone();
            bsp.root.Invert();
            return bsp;
        }

        public static BSP Union(BSP aInput, BSP bInput)
        {
            var a = aInput.Clone();
            var b = bInput.Clone();

            a.root.ClipTo(b.root);
            b.root.ClipTo(a.root);
            b.root.Invert();
            b.root.ClipTo(a.root);
            b.root.Invert();
            a.root.Build(b.root.AllPolygons);

            return a;
        }

        public static BSP Subtract(BSP aInput, BSP bInput)
        {
            var a = aInput.Clone();
            var b = bInput.Clone();

            a.root.Invert();
            a.root.ClipTo(b.root);
            b.root.ClipTo(a.root);
            b.root.Invert();
            b.root.ClipTo(a.root);
            b.root.Invert();
            a.root.Build(b.root.AllPolygons);
            a.root.Invert();

            return a;
        }

        public static BSP Intersect(BSP aInput, BSP bInput)
        {
            var a = aInput.Clone();
            var b = bInput.Clone();

            a.root.Invert();
            b.root.ClipTo(a.root);
            b.root.Invert();
            a.root.ClipTo(b.root);
            b.root.ClipTo(a.root);
            a.root.Build(b.root.AllPolygons);
            a.root.Invert();

            return a;
        }

        public static BSP Cube(Vector3 center, float radius)
        {
            KeyValuePair<int[], Vector3>[] info = new KeyValuePair<int[],Vector3>[]
            {
                new KeyValuePair<int[], Vector3>(new int[] { 0, 4, 6, 2 }, new Vector3(-1, 0, 0)),
                new KeyValuePair<int[], Vector3>(new int[] { 1, 3, 7, 5 }, new Vector3(+1, 0, 0)),
                new KeyValuePair<int[], Vector3>(new int[] { 0, 1, 5, 4 }, new Vector3(0, -1, 0)),
                new KeyValuePair<int[], Vector3>(new int[] { 2, 6, 7, 3 }, new Vector3(0, +1, 0)),
                new KeyValuePair<int[], Vector3>(new int[] { 0, 2, 3, 1 }, new Vector3(0, 0, -1)),
                new KeyValuePair<int[], Vector3>(new int[] { 4, 5, 7, 6 }, new Vector3(0, 0, +1))
            };

            var data = info.Select(i =>
            {
                return new Polygon(i.Key.Select(x =>
                {
                    var pos = new Vector3(
                        center.X + radius * (2 * ((x & 1) > 0 ? 1 : 0) - 1),
                        center.Y + radius * (2 * ((x & 2) > 0 ? 1 : 0) - 1),
                        center.Z + radius * (2 * ((x & 4) > 0 ? 1 : 0) - 1)
                    );

                    return new Vertex(pos, i.Value);
                }));
            });

            return new BSP(data);
        }

        private class Node
        {
            private Plane? splitPlane;

            private Node front;
            private Node back;

            private IList<Polygon> polygons = new List<Polygon>();

            public IEnumerable<Polygon> AllPolygons
            {
                get
                {
                    foreach (var p in polygons)
                        yield return p;

                    if (front != null)
                        foreach (var p in front.AllPolygons)
                            yield return p;
                    if (back != null)
                        foreach (var p in back.AllPolygons)
                            yield return p;
                }
            }

            public void Invert()
            {
                //flip polygons
                for (int i = 0; i < polygons.Count; i++)
                    polygons[i] = polygons[i].Flip();

                //flip splitplane
                splitPlane = new Plane(-splitPlane.Value.Normal, -splitPlane.Value.D);

                //flip front and back
                if (front != null)
                    front.Invert();
                if (back != null)
                    back.Invert();

                //swap front and back
                var tmp = front;
                front = back;
                back = tmp;
            }

            public IEnumerable<Polygon> ClipPolygons(IList<Polygon> polygons)
            {
                List<Polygon> frontPolys = new List<Polygon>();
                List<Polygon> backPolys = new List<Polygon>();

                for (int i = 0; i < polygons.Count; i++)
                    splitPlane.Value.SplitPolygon(polygons[i], frontPolys, backPolys, frontPolys, backPolys);

                if (front != null)
                    frontPolys = front.ClipPolygons(frontPolys).ToList();
                if (back != null)
                    backPolys = back.ClipPolygons(backPolys).ToList();

                return frontPolys.Concat(backPolys);
            }

            public void ClipTo(Node other)
            {
                polygons = other.ClipPolygons(polygons).ToList();

                if (front != null)
                    front.ClipTo(other);
                if (back != null)
                    back.ClipTo(other);
            }

            public void Build(IEnumerable<Polygon> polygons)
            {
                if (!splitPlane.HasValue)
                    splitPlane = polygons.First().Plane;

                List<Polygon> frontPolys = new List<Polygon>();
                List<Polygon> backPolys = new List<Polygon>();

                foreach (var poly in polygons)
                {
                    splitPlane.Value.SplitPolygon(poly, this.polygons, this.polygons, frontPolys, backPolys);
                }

                if (frontPolys.Count > 0)
                {
                    front = new Node();
                    front.Build(frontPolys);
                }

                if (backPolys.Count > 0)
                {
                    back = new Node();
                    back.Build(backPolys);
                }
            }
        }
    }
}
