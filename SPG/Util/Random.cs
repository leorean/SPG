using System;
using System.Collections.Generic;
using System.Text;

namespace SPG.Util
{
    public static class RND
    {
        public static double Next
        {
            get => new System.Random().NextDouble();
        }
    }
}