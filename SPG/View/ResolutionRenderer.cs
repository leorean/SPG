using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SPG.View
{
    public class ResolutionRenderer
    {
        public Viewport Viewport { get; protected set; }

        private static Matrix _scaleMatrix;
        /// <summary>
        /// Indicates that matrix update is needed
        /// </summary>
        private bool _dirtyMatrix = true;
        
        public Color BackgroundColor { get; set; } = Color.Black;
        
        public Vector2 VirtualScreenCenter { get; private set; }        
        public Vector2 VirtualScreenSize { get; private set; }
        
        public int ViewHeight { get; private set; }
        public int ViewWidth { get; private set; }
        
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }

        public ResolutionRenderer(int viewWidth, int viewHeight, int screenWidth, int screenHeight)
        {
            ViewWidth = viewWidth;
            ViewHeight = viewHeight;
            VirtualScreenCenter = new Vector2(ViewWidth * .5f, ViewHeight * .5f);
            VirtualScreenSize = new Vector2(ViewWidth, ViewHeight);

            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
            Initialize();
        }

        /// <summary>
        /// Initializes resolution renderer and marks it for refresh
        /// </summary>
        private void Initialize()
        {
            SetupVirtualScreenViewport();
            //calculate new ratio
            //mark for refresh
            _dirtyMatrix = true;
        }

        /// <summary>
        /// Setup viewport to real screen size
        /// </summary>
        public void SetupFullViewport()
        {
            var vp = new Viewport();
            vp.X = vp.Y = 0;
            vp.Width = ScreenWidth;
            vp.Height = ScreenHeight;
            GameManager.Game.GraphicsDevice.Viewport = vp;
            _dirtyMatrix = true;
        }


        public void SetupDraw()
        {
            SetupFullViewport();
            GameManager.Game.GraphicsDevice.Clear(BackgroundColor);
            SetupVirtualScreenViewport();
        }

        /// <summary>
        /// Get modified matrix for sprite rendering
        /// </summary>
        public Matrix GetTransformationMatrix()
        {
            if (_dirtyMatrix)
                RecreateScaleMatrix();

            return _scaleMatrix;
        }

        private void RecreateScaleMatrix()
        {
            if (!_invertScale)
                Matrix.CreateScale((float)ScreenWidth / ViewWidth, (float)ScreenWidth / ViewWidth, 1f, out _scaleMatrix);
            else Matrix.CreateScale((float)ScreenHeight / ViewHeight, (float)ScreenHeight / ViewHeight, 1f, out _scaleMatrix);
            _dirtyMatrix = false;
        }

        bool _invertScale;
        public void SetupVirtualScreenViewport()
        {
            var targetAspectRatio = ViewWidth / (float)ViewHeight;
            // figure out the largest area that fits in this resolution at the desired aspect ratio
            var width = ScreenWidth;
            var height = (int)(width / targetAspectRatio + .5f);

            if (height > ScreenHeight)
            {
                _invertScale = true;
                height = ScreenHeight;
                // PillarBox
                width = (int)(height * targetAspectRatio + .5f);
            }
            else _invertScale = false;

            // set up the new viewport centered in the backbuffer
            Viewport = new Viewport
            {
                X = (ScreenWidth / 2) - (width / 2),
                Y = (ScreenHeight / 2) - (height / 2),
                Width = width,
                Height = height
            };

            GameManager.Game.GraphicsDevice.Viewport = Viewport;
        }

        /// <summary>
        /// Converts screen coordinates to virtual coordinates
        /// </summary>
        /// <param name="screenPosition">Screen coordinates</param>
        public Vector2 ToVirtual(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition - new Vector2(Viewport.X, Viewport.Y), Matrix.Invert(GetTransformationMatrix()));
        }

        /// <summary>
        /// Converts screen coordinates to virtual coordinates
        /// </summary>
        /// <param name="screenPosition">Screen coordinates</param>
        public Point ToVirtual(Point screenPosition)
        {
            var v = Vector2.Transform(new Vector2(screenPosition.X, screenPosition.Y) - new Vector2(Viewport.X, Viewport.Y), Matrix.Invert(GetTransformationMatrix()));
            return new Point((int)v.X, (int)v.Y);
        }

        /// <summary>
        /// Converts screen coordinates to virtual coordinates
        /// </summary>
        /// <param name="virtualPosition">Screen coordinates</param>
        public Point ToDisplay(Point virtualPosition)
        {
            var v = Vector2.Transform(new Vector2(virtualPosition.X, virtualPosition.Y) + new Vector2(Viewport.X, Viewport.Y), GetTransformationMatrix());
            return new Point((int)v.X, (int)v.Y);
        }
    }
}