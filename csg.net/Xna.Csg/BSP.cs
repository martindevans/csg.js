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

        #region constructors
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
        #endregion

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

        #region mutation
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
        #endregion

        public float? RayCast(Ray ray)
        {
            return root.RayCast(ray);
        }

        private class Node
        {
            #region fields and properties
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
            #endregion

            #region mutation
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
            #endregion

            #region query
            private float? RayCastNode(Node n, Ray r)
            {
                if (n == null)
                    return null;

                return n.RayCast(r);
            }

            private bool RayCastItems(List<Polygon> polygons, Vector3 planeIntersectionPoint)
            {
                //project plane into 2D with a topology preserving mapping onto 2 axes
                Func<Vector3, Vector2> vectorReducer;
                Vector3 normal = new Vector3(Math.Abs(splitPlane.Value.Normal.X), Math.Abs(splitPlane.Value.Normal.Y), Math.Abs(splitPlane.Value.Normal.Z));
                if (normal.X > normal.Y && normal.X > normal.Z)
                    vectorReducer = a => new Vector2(a.Y, a.Z);
                else if (normal.Y > normal.X && normal.Y > normal.Z)
                    vectorReducer = a => new Vector2(a.X, a.Z);
                else
                    vectorReducer = a => new Vector2(a.X, a.Y);

                foreach (var poly in polygons)
                {
                    bool inside = true;
                    for (int i = 0; i < poly.Vertices.Length; i++)
                    {
                        var start = vectorReducer(poly.Vertices[i].Position - planeIntersectionPoint);
                        var end = vectorReducer(poly.Vertices[(i + 1) % poly.Vertices.Length].Position - planeIntersectionPoint);

                        var side = end - start;

                        if (Vector2.Dot(side, -start) <= 0)
                        {
                            inside = false;
                            break;
                        }
                    }

                    if (inside)
                        return true;
                }

                return false;
            }

            public float? RayCast(Ray r)
            {
                if (!splitPlane.HasValue)
                    return null;

                float distance = r.Position.Distance(splitPlane.Value);
                Vector3 planeIntersectionPoint = r.Position + r.Direction * distance;

                if (distance < -Extensions.EPSILON)
                {
                    var b = RayCastNode(back, r);
                    if (b.HasValue)
                        return b;

                    if (RayCastItems(polygons, planeIntersectionPoint))
                        return distance;

                    if (Vector3.Dot(r.Direction, splitPlane.Value.Normal) > 0)
                    {
                        var f = RayCastNode(front, new Ray(r.Position + r.Direction * distance, r.Direction));
                        if (f.HasValue)
                            return f;
                    }
                }
                else if (distance > Extensions.EPSILON)
                {
                    var f = RayCastNode(front, new Ray(r.Position + r.Direction * distance, r.Direction));
                    if (f.HasValue)
                        return f;

                    if (RayCastItems(polygons, planeIntersectionPoint))
                        return distance;

                    if (Vector3.Dot(r.Direction, splitPlane.Value.Normal) < 0)
                    {
                        var b = RayCastNode(back, r);
                        if (b.HasValue)
                            return b;
                    }
                }
                else
                {
                    if (RayCastItems(polygons, planeIntersectionPoint))
                        return distance;

                    float dot = Vector3.Dot(r.Direction, splitPlane.Value.Normal);
                    if (dot == 0)
                        return null;
                    if (dot > 0)
                    {
                        var f = RayCastNode(front, new Ray(planeIntersectionPoint, r.Direction));
                        if (f.HasValue)
                            return f;
                    }
                    else
                    {
                        var b = RayCastNode(back, r);
                        if (b.HasValue)
                            return b;
                    }
                }

                return null;
            }
            #endregion
        }
    }
}
