using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Xna.Csg.Primitives;

namespace Xna.Csg
{
    public class BSP
        :ICsgProvider
    {
        readonly bool _createDescription;
        object[] _description;
        public IEnumerable<object> Description
        {
            get
            {
                return _description;
            }
        }

        Node _root;
        public BoundingBox? Bounds
        {
            get;
            private set;
        }
        public IEnumerable<Polygon> Polygons
        {
            get
            {
                return _root.AllPolygons;
            }
        }

        #region constructors
        public BSP(bool createDescription = true)
            :this(new Node(), null, createDescription ? new object[0] : null)
        {
            _createDescription = createDescription;
        }

        protected BSP(IEnumerable<Polygon> polygons, BoundingBox bounds, object[] description, bool createDescription = true)
            : this()
        {
            _createDescription = createDescription;
            _root.Build(polygons);
            Bounds = bounds;
            _description = description;
        }

        private BSP(Node root, BoundingBox? bounds, object[] description, bool createDescription = true)
        {
            _root = root;
            Bounds = bounds;
            _description = description;
            _createDescription = createDescription;
        }

        public BSP Clone()
        {
            return Clone(a => a.Clone());
        }

        public BSP Clone(Func<Vertex, Vertex> clone)
        {
            return new BSP(_root.Clone(clone), Bounds, _description, _createDescription);
        }
        #endregion

        private object[] CreateDescription(string operation, object[] existing, params object[] args)
        {
            if (_createDescription)
                return new object[] { operation }.Append(existing).Append(args).ToArray();

            return null;
        }

        private BoundingBox? MeasureBounds(BSP bsp)
        {
            BoundingBox? b = null;

            foreach (var position in bsp.Polygons.SelectMany(a => a.Vertices).Select(a => a.Position))
            {
                if (b.HasValue)
                    b.Value.IncludePoint(position);
                else
                    b = new BoundingBox(position, position);
            }

            return b;
        }

        public virtual BSP Transform(Matrix transformation)
        {
            var polys = _root.AllPolygons.Select(a => new Polygon(a.Vertices.Select(v => v.Clone().Transform(transformation))));

            BSP b = new BSP(
                polys,
                Bounds.Value.Transform(transformation),
                CreateDescription("transform", _description, transformation.M11, transformation.M12, transformation.M13, transformation.M14, transformation.M21, transformation.M22, transformation.M23, transformation.M24, transformation.M31, transformation.M32, transformation.M33, transformation.M34, transformation.M41, transformation.M42, transformation.M43, transformation.M44)
            );
            
            return b;
        }

        #region mutation
        public virtual void Union(BSP bInput)
        {
            var a = _root;
            var b = bInput._root.Clone(null);

            a.ClipTo(b);
            b.ClipTo(a);
            b.Invert();
            b.ClipTo(a);
            b.Invert();
            a.Build(b.AllPolygons);

            if (Bounds.HasValue)
            {
                if (bInput.Bounds.HasValue)
                {
                    Bounds = new BoundingBox(
                        new Vector3(
                            Math.Min(Bounds.Value.Min.X, bInput.Bounds.Value.Min.X),
                            Math.Min(Bounds.Value.Min.Y, bInput.Bounds.Value.Min.Y),
                            Math.Min(Bounds.Value.Min.Z, bInput.Bounds.Value.Min.Z)
                        ),
                        new Vector3(
                            Math.Max(Bounds.Value.Max.X, bInput.Bounds.Value.Max.X),
                            Math.Max(Bounds.Value.Max.Y, bInput.Bounds.Value.Max.Y),
                            Math.Max(Bounds.Value.Max.Z, bInput.Bounds.Value.Max.Z)
                        )
                    );
                }
                else
                    Bounds = Bounds;
            }
            else
                Bounds = bInput.Bounds;

            _description = CreateDescription("union", _description, bInput._description);
        }

        public virtual void Subtract(BSP bInput)
        {
            if (!_root.AllPolygons.Any())
                return;

            var a = this._root;

            var b = bInput._root.Clone(null);

            a.Invert();
            a.ClipTo(b);
            b.ClipTo(a);
            b.Invert();
            b.ClipTo(a);
            b.Invert();
            a.Build(b.AllPolygons);
            a.Invert();

            Bounds = MeasureBounds(this);

            _description = CreateDescription("subtract", _description, bInput._description);
        }

        public virtual void Intersect(BSP bInput)
        {
            if (!_root.AllPolygons.Any())
                return;

            var a = this._root;
            var b = bInput._root.Clone(null);

            a.Invert();
            b.ClipTo(a);
            b.Invert();
            a.ClipTo(b);
            b.ClipTo(a);
            a.Build(b.AllPolygons);
            a.Invert();

            if (Bounds.HasValue)
            {
                if (bInput.Bounds.HasValue)
                {
                    Bounds = new BoundingBox(
                        new Vector3(
                            Math.Max(Bounds.Value.Min.X, bInput.Bounds.Value.Min.X),
                            Math.Max(Bounds.Value.Min.Y, bInput.Bounds.Value.Min.Y),
                            Math.Max(Bounds.Value.Min.Z, bInput.Bounds.Value.Min.Z)
                        ),
                        new Vector3(
                            Math.Min(Bounds.Value.Max.X, bInput.Bounds.Value.Max.X),
                            Math.Min(Bounds.Value.Max.Y, bInput.Bounds.Value.Max.Y),
                            Math.Min(Bounds.Value.Max.Z, bInput.Bounds.Value.Max.Z)
                        )
                    );
                }
                else
                    Bounds = null;
            }
            else
                Bounds = null;

            _description = CreateDescription("intersect", _description, bInput._description);
        }

        public virtual void Clear()
        {
            _root = new Node();
            Bounds = null;

            _description = new object[0];
        }

        public virtual void Split(out BSP topLeftFront, out BSP topLeftBack, out BSP topRightBack, out BSP topRightFront, out BSP bottomLeftFront, out BSP bottomLeftBack, out BSP bottomRightBack, out BSP bottomRightFront, Func<Vector3, Vector3, Vertex> vertexFactory = null)
        {
            if (!Bounds.HasValue)
                throw new InvalidOperationException("Cannot split a BSP tree without bounds");
            vertexFactory = vertexFactory ?? ((p, n) => new Vertex(p, n));

            var bounds = Bounds.Value;
            var size = (bounds.Max - bounds.Min);

            float halfWidth = size.X / 2;
            float halfHeight = size.Y / 2;
            float halfDepth = size.Z / 2;

            float qtrWidth = halfWidth / 2;
            float qtrHeight = halfHeight / 2;
            float qtrDepth = halfDepth / 2;

            topLeftFront = new Cube(vertexFactory).Transform(Matrix.CreateScale(halfWidth, halfHeight, halfDepth) * Matrix.CreateTranslation(-qtrWidth, qtrHeight, -qtrDepth));
            topLeftFront.Intersect(this);

            topLeftBack = new Cube(vertexFactory).Transform(Matrix.CreateScale(halfWidth, halfHeight, halfDepth) * Matrix.CreateTranslation(-qtrWidth, qtrHeight, qtrDepth));
            topLeftBack.Intersect(this);

            topRightFront = new Cube(vertexFactory).Transform(Matrix.CreateScale(halfWidth, halfHeight, halfDepth) * Matrix.CreateTranslation(qtrWidth, qtrHeight, -qtrDepth));
            topRightFront.Intersect(this);

            topRightBack = new Cube(vertexFactory).Transform(Matrix.CreateScale(halfWidth, halfHeight, halfDepth) * Matrix.CreateTranslation(qtrWidth, qtrHeight, qtrDepth));
            topRightBack.Intersect(this);

            bottomLeftFront = new Cube(vertexFactory).Transform(Matrix.CreateScale(halfWidth, halfHeight, halfDepth) * Matrix.CreateTranslation(-qtrWidth, -qtrHeight, -qtrDepth));
            bottomLeftFront.Intersect(this);

            bottomLeftBack = new Cube(vertexFactory).Transform(Matrix.CreateScale(halfWidth, halfHeight, halfDepth) * Matrix.CreateTranslation(-qtrWidth, -qtrHeight, qtrDepth));
            bottomLeftBack.Intersect(this);

            bottomRightFront = new Cube(vertexFactory).Transform(Matrix.CreateScale(halfWidth, halfHeight, halfDepth) * Matrix.CreateTranslation(qtrWidth, -qtrHeight, -qtrDepth));
            bottomRightFront.Intersect(this);

            bottomRightBack = new Cube(vertexFactory).Transform(Matrix.CreateScale(halfWidth, halfHeight, halfDepth) * Matrix.CreateTranslation(qtrWidth, -qtrHeight, qtrDepth));
            bottomRightBack.Intersect(this);
        }
        #endregion

        public virtual float? RayCast(Ray ray)
        {
            if (Bounds.HasValue)
            {
                float? boundsDistance = ray.Intersects(Bounds.Value);

                if (boundsDistance.HasValue)
                    return _root.RayCast(ray);
            }

            return null;
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
                if (!splitPlane.HasValue)
                    return;

                //flip polygons
                for (int i = 0; i < polygons.Count; i++)
                    polygons[i].Flip();

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

            private IEnumerable<Polygon> ClipPolygons(IList<Polygon> polygons)
            {
                if (!splitPlane.HasValue)
                    return polygons.ToArray();

                List<Polygon> frontClippedPolygons = new List<Polygon>();
                List<Polygon> backClippedPolygons = new List<Polygon>();

                for (int i = 0; i < polygons.Count; i++)
                    splitPlane.Value.SplitPolygon(polygons[i], frontClippedPolygons, backClippedPolygons, frontClippedPolygons, backClippedPolygons);

                if (front != null)
                    frontClippedPolygons = front.ClipPolygons(frontClippedPolygons).ToList();
                if (back != null)
                    backClippedPolygons = back.ClipPolygons(backClippedPolygons).ToList();
                else
                    backClippedPolygons.Clear();

                return frontClippedPolygons.Concat(backClippedPolygons);
            }

            public void ClipTo(Node other)
            {
                polygons = other.ClipPolygons(polygons).ToList();

                if (front != null)
                    front.ClipTo(other);
                if (back != null)
                    back.ClipTo(other);
            }

            [ThreadStatic]
            private static Random _random;
            private static Polygon SelectSplitPlane(IEnumerable<Polygon> polygons)
            {
                int count = polygons.Count();

                if (count == 0)
                    return null;

                if (count == 1)
                    return polygons.First();

                if (_random == null)
                    _random = new Random();

                return polygons.Skip(_random.Next(0, count - 1)).First();
            }

            private static void Build(Node node, IEnumerable<Polygon> polygons, Queue<KeyValuePair<Node, IEnumerable<Polygon>>> todo)
            {
                if (!node.splitPlane.HasValue)
                {
                    var split = SelectSplitPlane(polygons);
                    if (split == null)
                        return;
                    else
                        node.splitPlane = split.Plane;
                }

                List<Polygon> frontPolys = new List<Polygon>();
                List<Polygon> backPolys = new List<Polygon>();

                foreach (var poly in polygons)
                    node.splitPlane.Value.SplitPolygon(poly, node.polygons, node.polygons, frontPolys, backPolys);

                if (frontPolys.Count > 0)
                {
                    if (node.front == null)
                        node.front = new Node();
                    todo.Enqueue(new KeyValuePair<Node, IEnumerable<Polygon>>(node.front, frontPolys));
                }

                if (backPolys.Count > 0)
                {
                    if (node.back == null)
                        node.back = new Node();
                    todo.Enqueue(new KeyValuePair<Node, IEnumerable<Polygon>>(node.back, backPolys));
                }
            }

            public void Build(IEnumerable<Polygon> polygons)
            {
                Queue<KeyValuePair<Node, IEnumerable<Polygon>>> todo = new Queue<KeyValuePair<Node, IEnumerable<Polygon>>>();

                todo.Enqueue(new KeyValuePair<Node, IEnumerable<Polygon>>(this, polygons));

                int loops = 0;
                while (todo.Count > 0)
                {
                    loops++;
                    var work = todo.Dequeue();
                    Build(work.Key, work.Value, todo);
                }
            }

            public Node Clone(Func<Vertex, Vertex> clone)
            {
                clone = clone ?? (a => a.Clone());

                Node n = new Node();

                if (splitPlane.HasValue)
                    n.splitPlane = splitPlane.Value;

                if (front != null)
                    n.front = front.Clone(clone);

                if (back != null)
                    n.back = back.Clone(clone);

                n.polygons.AddRange(polygons.Select(a => a.Clone(clone)));

                return n;
            }
            #endregion

            #region query
            private float? RayCastNode(Node n, Ray r)
            {
                return n == null ? null : n.RayCast(r);
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

                if (distance < -Extensions.Epsilon)
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
                else if (distance > Extensions.Epsilon)
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
