using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using System;

namespace SPG.View
{
    

    public class Camera
    {
        /*
        public GameObject Target { get; set; }

        public RenderTarget2D RenderTarget {get; private set;}

        public Vector2 Position { get; set; }

        private Vector3 _camTranslationVector = Vector3.Zero;
        private Matrix _camTranslationMatrix;
        private Matrix _camRotationMatrix;
        private float _rotation;

        public Camera(int width, int height)
        {
            RenderTarget = new RenderTarget2D(GameManager.Game.GraphicsDevice, width, height);
            
        }

        public Vector2 ToViewCoordinates(Vector2 position)
        {
            var relX = (position.X / GameManager.Game.GraphicsDeviceManager.PreferredBackBufferWidth) * RenderTarget.Width;
            var relY = (position.Y / GameManager.Game.GraphicsDeviceManager.PreferredBackBufferHeight) * RenderTarget.Height;

            return new Vector2(relX, relY);
        }
        
        public void Update()
        {
            if(Target != null)
            {
                Position = Target.Position;
            }
        }

        public Matrix GetViewTransformationMatrix()
        {
            if (true)
            {

                _camTranslationVector.X = -Position.X;
                _camTranslationVector.Y = -Position.Y;

                Matrix.CreateTranslation(ref _camTranslationVector, out _camTranslationMatrix);
                Matrix.CreateRotationZ(_rotation, out _camRotationMatrix);

                _camScaleVector.X = _zoom;
                _camScaleVector.Y = _zoom;
                _camScaleVector.Z = 1;

                Matrix.CreateScale(ref _camScaleVector, out _camScaleMatrix);

                _resTranslationVector.X = Irr.VirtualWidth * 0.5f;
                _resTranslationVector.Y = Irr.VirtualHeight * 0.5f;
                _resTranslationVector.Z = 0;

                Matrix.CreateTranslation(ref _resTranslationVector, out _resTranslationMatrix);

                _transform = _camTranslationMatrix *
                             _camRotationMatrix *
                             _camScaleMatrix *
                             _resTranslationMatrix *
                             Irr.GetTransformationMatrix();

                //_isViewTransformationDirty = false;
            }

            return _transform;
        }*/
        /*public Vector2 ToWorldCoordinates(Vector2 pixels)
        {
            Vector3 worldPosition = GameManager.Game.GraphicsDevice.Viewport.Unproject(new Vector3(pixels, 0),
                    Projection, View, Matrix.Identity);
            return new Vector2(worldPosition.X, worldPosition.Y);
        }

        public Vector2 ToScreenCoordinates(Vector2 worldCoords)
        {
            var screenPositon = GameManager.Game.GraphicsDevice.Viewport.Project(new Vector3(worldCoords, 0),
                    Projection, View, Matrix.Identity);
            return new Vector2(screenPositon.X, screenPositon.Y);
        }*/
    }
}