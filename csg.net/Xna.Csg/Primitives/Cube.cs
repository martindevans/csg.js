using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xna.Csg.Primitives
{
    public class Cube
        :BSP
    {
        public Cube()
            :base(CreatePolygons())
        {
        }

        private static IEnumerable<Polygon> CreatePolygons()
        {
            KeyValuePair<int[], Vector3>[] info = new KeyValuePair<int[], Vector3>[]
            {
                new KeyValuePair<int[], Vector3>(new int[] { 0, 4, 6, 2 }, new Vector3(-1, 0, 0)),
                new KeyValuePair<int[], Vector3>(new int[] { 1, 3, 7, 5 }, new Vector3(+1, 0, 0)),
                new KeyValuePair<int[], Vector3>(new int[] { 0, 1, 5, 4 }, new Vector3(0, -1, 0)),
                new KeyValuePair<int[], Vector3>(new int[] { 2, 6, 7, 3 }, new Vector3(0, +1, 0)),
                new KeyValuePair<int[], Vector3>(new int[] { 0, 2, 3, 1 }, new Vector3(0, 0, -1)),
                new KeyValuePair<int[], Vector3>(new int[] { 4, 5, 7, 6 }, new Vector3(0, 0, +1))
            };

            Vector3 center = Vector3.Zero;
            Vector3 extents = new Vector3(0.5f);

            var data = info.Select(i =>
            {
                return new Polygon(i.Key.Select(x =>
                {
                    var pos = new Vector3(
                        center.X + extents.X * (2 * ((x & 1) > 0 ? 1 : 0) - 1),
                        center.Y + extents.Y * (2 * ((x & 2) > 0 ? 1 : 0) - 1),
                        center.Z + extents.Z * (2 * ((x & 4) > 0 ? 1 : 0) - 1)
                    );

                    return new Vertex(pos, i.Value);
                }));
            });

            return data;
        }
    }
}
