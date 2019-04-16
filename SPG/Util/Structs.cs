﻿using Microsoft.Xna.Framework;
using System;

namespace SPG.Util
{
    public struct Grid<T>
    {
        private T[][] data;

        public int Width { get; }
        public int Height { get; }

        public int Count { get => Width * Height; }
        
        public Grid(int w, int h)
        {
            this.Width = w;
            this.Height = h;

            data = new T[w][];
            for (var i = 0; i < w; i++)
            {
                data[i] = new T[h];
            }
        }

        public void Set(int i, T value)
        {
            var x = i % Width;
            var y = (int)MathUtil.Floor((double)(i / Width));

            data[x][y] = value;
        }

        public void Set(int x, int y, T value)
        {
            data[x][y] = value;
        }

        public T Get(int i)
        {
            var x = i % Width;
            var y = (int)MathUtil.Floor((double)(i / Width));

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