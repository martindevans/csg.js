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
            :this(new Node())
        {
        }

        public BSP(IEnumerable<Polygon> polygons)
            :this()
        {
            root.Build(polygons);
        }

        private BSP(Node root)
        {
            this.root = root;
        }

        public BSP Clone()
        {
            return new BSP(root.Clone());
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

            private List<Polygon> polygons = new List<Polygon>();

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
                List<Polygon> front = new List<Polygon>();
                List<Polygon> back = new List<Polygon>();

                for (int i = 0; i < polygons.Count; i++)
                    splitPlane.Value.SplitPolygon(polygons[i], front, back, front, back);

                if (this.front != null)
                    front = this.front.ClipPolygons(front).ToList();
                if (this.back != null)
                    back = this.back.ClipPolygons(back).ToList();

                return front.Concat(back);
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
                    if (front == null)
                        front = new Node();
                    front.Build(frontPolys);
                }

                if (backPolys.Count > 0)
                {
                    if (back == null)
                        back = new Node();
                    back.Build(backPolys);
                }
            }

            public Node Clone()
            {
                Node n = new Node();

                if (splitPlane.HasValue)
                    n.splitPlane = splitPlane.Value;

                if (front != null)
                    n.front = front.Clone();

                if (back != null)
                    n.back = back.Clone();

                n.polygons.AddRange(polygons.Select(a => a.Clone()));

                return n;
            }
        }
    }
}
