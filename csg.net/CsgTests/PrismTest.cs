using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xna.Csg.Primitives;
using Microsoft.Xna.Framework;

namespace CsgTests
{
    [TestClass]
    public class PrismTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IncorrectWinding()
        {
            Prism c = new Prism(1, new Vector2[]
            {   
                new Vector2(0, 0),
                new Vector2(1, 1),
                new Vector2(-1, 1),
            });
        }
    }
}
