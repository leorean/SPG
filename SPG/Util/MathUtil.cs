using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace SPG.Util
{
    public static class MathUtil
    {
        public static float Clamp(this float f, float v1, float v2)
        {
            return Math.Min(Math.Max(f, v1), v2);
        }

        public static int Div(double v1, double v2)
        {            
            return Floor(v1 / v2);
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

        public static float ToAngle(this Vector2 vector, bool inRadiant = false)
        {
            var rad = (float)Math.Atan2(vector.Y, vector.X);

            if (inRadiant)
                return rad;

            return (float)(rad / (2f * Math.PI)) * 360;
        }

        public static Vector2 Vector2(this Vector3 vec)
        {
            return new Vector2(vec.X, vec.Y);
        }

        public static double LengthDirX(float angle)
        {
            var rad = (angle / 360) * 2 * Math.PI;

            return Math.Cos(rad);
            
        }

        public static double LengthDirY(float angle)
        {
            var rad = (angle / 360) * 2 * Math.PI;

            return Math.Sin(rad);
        }
    }
}