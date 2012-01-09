using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Xna.Csg.Primitives;

namespace Xna.Csg
{
    public class CompositeBSP
        :ICsgProvider
    {
        public int SubSpaceSize;

        Dictionary<Tuple<int, int, int>, SubSpace> spaces = new Dictionary<Tuple<int, int, int>, SubSpace>();

        public CompositeBSP(int spaceSize)
        {
            SubSpaceSize = spaceSize;
        }

        public void Union(BSP brush)
        {
            DoCsgOperation(brush, (b, world) =>
                {
                    world.Union(b);
                });
        }

        public void Subtract(BSP brush)
        {
            DoCsgOperation(brush, (b, world) =>
            {
                world.Subtract(b);
            });
        }

        public void Intersect(BSP brush)
        {
            DoCsgOperation(brush, (b, world) =>
            {
                world.Intersect(b);
            });
        }

        private SubSpace GetSpace(Vector3 position)
        {
            int x = ((int)(position.X / SubSpaceSize)) * SubSpaceSize;
            int y = ((int)(position.Y / SubSpaceSize)) * SubSpaceSize;
            int z = ((int)(position.Z / SubSpaceSize)) * SubSpaceSize;

            Tuple<int, int, int> key = new Tuple<int, int, int>(x, y, z);

            SubSpace s;
            if (!spaces.TryGetValue(key, out s))
            {
                s = new SubSpace(x, y, z, SubSpaceSize);
                spaces.Add(key, s);
            }

            return s;
        }

        private void DoCsgOperation(BSP brush, Action<BSP, BSP> action)
        {
            var brushClone = brush.Clone();

            while (!brush.Polygons.IsEmpty())
            {
                //Get the subspace which contains this polygon
                var subSpace = GetSpace(brush.Vertices.First().Position);

                //Make a new brush which only fits into this subspace
                var clone = brushClone.Clone();
                subSpace.ClipToBounds(clone);

                //Remove brush for this subspace from overall brush
                brushClone.Subtract(clone);

                //Do the action with the brush which fits in this space and the space itself
                action(clone, subSpace.BSP);
            }
        }

        private class SubSpace
        {
            public BSP BSP
            {
                get;
                private set;
            }

            public readonly int MinX;
            public readonly int MinY;
            public readonly int MinZ;

            public readonly int MaxX;
            public readonly int MaxY;
            public readonly int MaxZ;

            public readonly int Size;

            public SubSpace(int x, int y, int z, int size)
            {
                BSP = new BSP();

                MinX = x;
                MinY = y;
                MinZ = z;
                MaxX = x + size;
                MaxY = y + size;
                MaxZ = z + size;
                Size = size;
            }

            public void ClipToBounds(BSP clone)
            {
                Vector3 center = new Vector3(MinX * 0.5f + MaxX * 0.5f, MinY * 0.5f + MaxY * 0.5f, MinZ * 0.5f + MaxZ * 0.5f);

                clone.Intersect(new Cube().Transform(Matrix.CreateScale(Size) * Matrix.CreateTranslation(center.X, center.Y, center.Z)));
            }
        }
    }
}
