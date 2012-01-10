using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xna.Csg
{
    public class BoundedBsp
        :BSP
    {
        public BoundingBox? Bounds
        {
            get;
            private set;
        }

        #region constructors
        public BoundedBsp()
        {
            Bounds = null;
        }

        public BoundedBsp(BSP bsp)
            :base(bsp.ToPolygons())
        {
            Bounds = MeasureBounds(bsp);
        }

        private BoundedBsp(BSP bsp, BoundingBox? bounds)
            :this(bsp)
        {
            this.Bounds = bounds;
        }

        protected BoundedBsp(IEnumerable<Polygon> polygons, BoundingBox bounds)
            : base(polygons)
        {
            this.Bounds = bounds;
        }
        #endregion

        private BoundingBox? MeasureBounds(BSP bsp)
        {
            BoundingBox? b = null;

            foreach (var position in bsp.ToPolygons().SelectMany(a => a.Vertices).Select(a => a.Position))
            {
                if (b.HasValue)
                    b.Value.IncludePoint(position);
                else
                    b = new BoundingBox(position, position);
            }

            return b;
        }

        public override BSP Transform(Matrix transformation)
        {
            BSP bsp = base.Transform(transformation);

            BoundingBox? b = null;
            if (Bounds.HasValue)
                b = Bounds.Value.Transform(transformation);

            return new BoundedBsp(bsp, b);
        }

        private BoundingBox GetBounds(BSP bsp)
        {
            BoundingBox other;
            var boundedOther = bsp as BoundedBsp;
            if (boundedOther != null)
                other = boundedOther.Bounds.Value;
            else
                other = MeasureBounds(bsp).Value;

            return other;
        }

        public override void Subtract(BSP bInput)
        {
            base.Subtract(bInput);

            Bounds = MeasureBounds(this);
        }

        public override void Intersect(BSP bInput)
        {
            base.Intersect(bInput);

            BoundingBox other = GetBounds(bInput);
            if (Bounds.HasValue)
            {
                Bounds = new BoundingBox(
                    new Vector3(
                        Math.Max(Bounds.Value.Min.X, other.Min.X),
                        Math.Max(Bounds.Value.Min.Y, other.Min.Y),
                        Math.Max(Bounds.Value.Min.Z, other.Min.Z)
                    ),
                    new Vector3(
                        Math.Min(Bounds.Value.Max.X, other.Max.X),
                        Math.Min(Bounds.Value.Max.Y, other.Max.Y),
                        Math.Min(Bounds.Value.Max.Z, other.Max.Z)
                    )
                );
            }
        }

        public override void Union(BSP bInput)
        {
            base.Union(bInput);

            BoundingBox other = GetBounds(bInput);
            if (Bounds.HasValue)
            {
                Bounds = new BoundingBox(
                    new Vector3(
                        Math.Min(Bounds.Value.Min.X, other.Min.X),
                        Math.Min(Bounds.Value.Min.Y, other.Min.Y),
                        Math.Min(Bounds.Value.Min.Z, other.Min.Z)
                    ),
                    new Vector3(
                        Math.Max(Bounds.Value.Max.X, other.Max.X),
                        Math.Max(Bounds.Value.Max.Y, other.Max.Y),
                        Math.Max(Bounds.Value.Max.Z, other.Max.Z)
                    )
                );
            }
            else
                Bounds = other;
        }

        public override void Clear()
        {
            Bounds = null;

            base.Clear();
        }

        public override float? RayCast(Microsoft.Xna.Framework.Ray ray)
        {
            if (Bounds.HasValue)
            {
                float? boundsDistance = ray.Intersects(Bounds.Value);

                if (boundsDistance.HasValue)
                    return base.RayCast(ray);
            }

            return null;
        }
    }
}
