using Microsoft.Xna.Framework;
using System;

namespace SPG.Util
{
    public struct Grid<T>
    {
        private T[][] data;

        private int w;
        private int h;

        public Grid(int w, int h)
        {
            this.w = w;
            this.h = h;

            data = new T[w][];
            for (var i = 0; i < w; i++)
            {
                data[i] = new T[h];
            }
        }

        public void Set(int i, T value)
        {
            var x = i % w;
            var y = (int)Math.Floor((double)(i / w));

            data[x][y] = value;
        }

        public void Set(int x, int y, T value)
        {
            data[x][y] = value;
        }

        public T Get(int i)
        {
            var x = i % w;
            var y = (int)Math.Floor((double)(i / w));

            return data[x][y];
        }

        public T Get(int x, int y)
        {
            return data[x][y];
        }        
    }

    public struct RectF
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }

        public RectF(float x, float y, float w, float h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }
    }
    
    public struct Size
    {
        private Point p;

        public int Width => p.X;
        public int Height => p.Y;

        public Size(int w, int h)
        {
            p = new Point(w, h);
        }
    }
}