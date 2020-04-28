using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace SPG.Util
{
    public static class RND
    {
        private static Random r = new System.Random();
        
        //public static double Next
        //{
        //    get => r.NextDouble();
        //}

        public static float Next
        {
            get => (float) r.NextDouble();
        }

        public static double RandomGauss(double mu = 0, double sigma = 1)
        {
            if (sigma <= 0)
                throw new ArgumentOutOfRangeException("sigma", "Must be greater than zero.");

            var u1 = 1 - Next;
            var u2 = 1 - Next;
            var temp1 = Math.Sqrt(-2 * Math.Log(u1));
            var temp2 = 2 * Math.PI * u2;

            return mu + sigma * ((temp1 * Math.Cos(temp2)) / (2 * Math.PI));
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