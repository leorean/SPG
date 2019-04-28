using System;
using System.Collections.Generic;
using System.Text;

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
    }
}