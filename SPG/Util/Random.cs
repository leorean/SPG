using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace SPG.Util
{
    public static class RND
    {
        private static Random r = new System.Random();
        public static double Next
        {
            get => r.NextDouble();
        }

        public static int Int(int maxVal)
        {
            var rnd = Next * maxVal;

            return (int)Math.Round(rnd);
        }

        public static T Choose<T>(params T[] p)
        {
            return p[Int(p.Length - 1)];            
        }
    }
}