using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xna.Csg
{
    public class BSP
    {
        public event Action OnChange;

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

        public BSP Transform(Matrix transformation)
        {
            BSP b = new BSP(
                root
                .AllPolygons
                .Select(a =>
                    new Polygon(
                        a.Vertices.Select(v =>
                            new Vertex(Vector3.Transform(v.Position, transformation), Vector3.TransformNormal(v.Normal, transformation))
                        )
                    )
                )
            );

            InvokeChange();

            return b;
        }

        private void InvokeChange()
        {
            if (OnChange != null)
                OnChange();
        }

        public void Union(BSP bInput)
        {
            var a = this.root;
            var b = bInput.root.Clone();

            a.ClipTo(b);
            b.ClipTo(a);
            b.Invert();
            b.ClipTo(a);
            b.Invert();
            a.Build(b.AllPolygons);

            InvokeChange();
        }

        public void Subtract(BSP bInput)
        {
            var a = this.root;
            var b = bInput.root.Clone();

            a.Invert();
            a.ClipTo(b);
            b.ClipTo(a);
            b.Invert();
            b.ClipTo(a);
            b.Invert();
            a.Build(b.AllPolygons);
            a.Invert();

            InvokeChange();
        }

        public void Intersect(BSP bInput)
        {
            var a = this.root;
            var b = bInput.root.Clone();

            a.Invert();
            b.ClipTo(a);
            b.Invert();
            a.ClipTo(b);
            b.ClipTo(a);
            a.Build(b.AllPolygons);
            a.Invert();

            InvokeChange();
        }

        public void Clear()
        {
            root = new Node();

            InvokeChange();
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
                if (!splitPlane.HasValue)
                    return polygons.ToArray();

                List<Polygon> front = new List<Polygon>();
                List<Polygon> back = new List<Polygon>();

                for (int i = 0; i < polygons.Count; i++)
                    splitPlane.Value.SplitPolygon(polygons[i], front, back, front, back);

                if (this.front != null)
                    front = this.front.ClipPolygons(front).ToList();
                if (this.back != null)
                    back = this.back.ClipPolygons(back).ToList();
                else
                    back.Clear();

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
                {
                    if (polygons.FirstOrDefault() == null)
                        return;
                    else
                        splitPlane = polygons.First().Plane;
                }

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
