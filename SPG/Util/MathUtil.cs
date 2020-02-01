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

        public static bool In<T>(this T v1, T v2, T v3)
        {
            var f1 = (float)(object)v1;
            var f2 = (float)(object)v2;
            var f3 = (float)(object)v3;

            return f1 >= f2 && f1 <= f3;
        }

        public static double DegToRad(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        public static double RadToDeg(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        public static double VectorToAngle(this Vector2 vector, bool inRadiant = false)
        {
            var rad = Math.Atan2(vector.Y, vector.X);

            if (inRadiant)
                return rad;

            return RadToDeg(rad);
        }

        public static Vector2 Vector2(this Vector3 vec)
        {
            return new Vector2(vec.X, vec.Y);
        }

        public static double LengthDirX(double degAngle)
        {
            var rad = (degAngle / 360f) * 2 * Math.PI;
            return Math.Cos(rad);
            
        }

        public static double LengthDirY(double degAngle)
        {
            var rad = (degAngle / 360f) * 2 * Math.PI;
            return Math.Sin(rad);
        }

        public static float AtMost(float val, float limit)
        {
            return Math.Sign(val) * Math.Min(Math.Abs(val), limit);
        }

        public static float AtLeast(float val, float limit)
        {
            return Math.Sign(val) * Math.Max(Math.Abs(val), limit);
        }

        public static double Euclidean(Point p1, Point p2) => Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));

        public static double Euclidean(Vector2 p1, Vector2 p2) => Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));

        public static double Euclidean(float p1x, float p1y, float p2x, float p2y) => Math.Sqrt(Math.Pow(p1x - p2x, 2) + Math.Pow(p1y - p2y, 2));

    }
}