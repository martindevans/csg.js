using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xna.Csg
{
    public interface ICsgProvider
    {
        void Union(BSP brush);

        void Subtract(BSP brush);

        void Intersect(BSP brush);
    }
}
