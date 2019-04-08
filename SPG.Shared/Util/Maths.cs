using Microsoft.Xna.Framework;
using System;

namespace SPG.Util
{
    public static class Math
    {
        public static int Div<T, V>(T v1, V v2)
        {
            float f = (float)(object)v1;
            float s = (float)(object)v2;

            return Floor(f / s);
        }

        public static int Floor<T>(T value)
        {
            return (int)System.Math.Floor((double)(object)value);
        }

        public static bool In<T>(T v1, T v2, T v3)
        {
            var f1 = (float)(object)v1;
            var f2 = (float)(object)v2;
            var f3 = (float)(object)v3;

            return f1 >= f2 && f1 <= f3;
        }

        public static Vector2 Vector2(this Vector3 vec)
        {
            return new Vector2(vec.X, vec.Y);
        }
    }
}