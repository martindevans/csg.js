using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xna.Csg
{
    public interface ICsgProvider
    {
        void Union(BSP bsp);

        void Intersect(BSP bsp);

        void Subtract(BSP bsp);

        void Clear();

        IEnumerable<Polygon> Polygons
        {
            get;
        }
    }
}
