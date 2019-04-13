using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace SPG.View
{

    public static class CameraExtensions
    {
        public static void BeginCamera(this SpriteBatch mBatch, Camera camera)
        {
            mBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, camera.GetViewTransformationMatrix());
        }

        public static void BeginCamera(this SpriteBatch mBatch, Camera camera, BlendState bstate)
        {
            mBatch.Begin(SpriteSortMode.Deferred, bstate, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone, null, camera.GetViewTransformationMatrix());
        }

        public static Rectangle NewInflate(this Rectangle rect, int width, int height)
        {
            var r = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
            r.Inflate(width, height);
            return r;
        }

        public static void DrawString(this SpriteBatch mBatch, SpriteFont font, string text, Vector2 pos, Color color, float scale = 1f)
        {
            mBatch.DrawString(font, text, pos, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        public static void Draw(this SpriteBatch mBatch, Texture2D tex, Rectangle rec)
        {
            mBatch.Draw(tex, rec, Color.White);
        }

        public static void Draw(this SpriteBatch mBatch, Texture2D tex, Vector2 pos)
        {
            mBatch.Draw(tex, pos, Color.White);
        }

        public static void BeginResolution(this SpriteBatch mBatch, ResolutionRenderer renderer)
        {
            mBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone, null, renderer.GetTransformationMatrix());
        }

        public static void BeginResolution(this SpriteBatch mBatch, ResolutionRenderer renderer, BlendState bstate)
        {
            mBatch.Begin(SpriteSortMode.Deferred, bstate, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone, null, renderer.GetTransformationMatrix());
        }
    }
    
    public class Camera : IDisposable
    {
        #region Variables
        private float _zoom;
        private float _rotation;
        private Vector2 _position;
        private Matrix _transform = Matrix.Identity;
        private bool _isViewTransformationDirty = true;
        private Matrix _camTranslationMatrix = Matrix.Identity;
        private Matrix _camRotationMatrix = Matrix.Identity;
        private Matrix _camScaleMatrix = Matrix.Identity;
        private Matrix _resTranslationMatrix = Matrix.Identity;

        protected ResolutionRenderer ResolutionRenderer;
        private Vector3 _camTranslationVector = Vector3.Zero;
        private Vector3 _camScaleVector = Vector3.Zero;
        private Vector3 _resTranslationVector = Vector3.Zero;

        private bool _boundsEnabled = false;
        private Rectangle _bounds;
        
        public void EnableBounds(Rectangle rect)
        {
            _bounds = rect;
            _boundsEnabled = true;
        }
        
        /// <summary>
        /// Current camera position
        /// </summary>
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                if (_boundsEnabled)
                {
                    _position = new Vector2(
                        Math.Min(
                            Math.Max(value.X, _bounds.X + ResolutionRenderer.ViewWidth * .5f / Zoom),
                            _bounds.Width - ResolutionRenderer.ViewWidth * .5f / Zoom),
                        Math.Min(
                            Math.Max(value.Y, _bounds.Y + ResolutionRenderer.ViewHeight * .5f / Zoom),
                            _bounds.Height - ResolutionRenderer.ViewHeight * .5f / Zoom)
                        );
                } else
                {
                    _position = value;
                }
                _isViewTransformationDirty = true;
            }
        }

        /// <summary>
        /// Minimum zoom value (can be no less than 0.1f)
        /// </summary>
        public float MinZoom { get; set; }
        /// <summary>
        /// Maximum zoom value
        /// </summary>
        public float MaxZoom { get; set; }

        /// <summary>
        /// Gets or sets camera zoom value
        /// </summary>
        public float Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = value;
                if (_zoom < 0.1f)
                    _zoom = 0.1f;
                if (_zoom < MinZoom) _zoom = MinZoom;
                if (_zoom > MaxZoom) _zoom = MaxZoom;
                _isViewTransformationDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets camera rotation value
        /// </summary>
        public float Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;
                _isViewTransformationDirty = true;
            }
        }
        #endregion

        public Camera(ResolutionRenderer resolutionRenderer)
        {
            ResolutionRenderer = resolutionRenderer;
            _zoom = 0.1f;
            _rotation = 0.0f;
            _position = Vector2.Zero;
            MinZoom = 0.1f;
            MaxZoom = 999f;
        }

        /// <summary>
        /// Center camera and fit the area of specified rectangle
        /// </summary>
        /// <param name="rec">Rectange</param>
        public void CenterOnTarget(Rectangle rec)
        {
            Position = new Vector2(rec.Center.X, rec.Center.Y);
            var fat1 = ResolutionRenderer.ViewWidth / (float)ResolutionRenderer.ViewHeight;
            var fat2 = rec.Width / (float)rec.Height;
            float ratio;
            if (fat2 >= fat1) ratio = ResolutionRenderer.ViewWidth / (float)rec.Width;
            else ratio = ResolutionRenderer.ViewHeight / (float)rec.Height;
            Zoom = ratio;
        }

        /// <summary>
        /// Move camera by specified vector
        /// </summary>
        /// <param name="amount">Vector movement</param>
        public void Move(Vector2 amount)
        {
            Position += amount;
        }

        /// <summary>
        /// Set camera position
        /// </summary>
        /// <param name="position">Position</param>
        public void SetPosition(Vector2 position)
        {
            Position = position;
        }

        /// <summary>
        /// Get camera transformation matrix
        /// </summary>
        public Matrix GetViewTransformationMatrix()
        {
            if (_isViewTransformationDirty)
            {
                _camTranslationVector.X = -_position.X;
                _camTranslationVector.Y = -_position.Y;

                Matrix.CreateTranslation(ref _camTranslationVector, out _camTranslationMatrix);
                Matrix.CreateRotationZ(_rotation, out _camRotationMatrix);

                _camScaleVector.X = _zoom;
                _camScaleVector.Y = _zoom;
                _camScaleVector.Z = 1;

                Matrix.CreateScale(ref _camScaleVector, out _camScaleMatrix);

                _resTranslationVector.X = ResolutionRenderer.ViewWidth * 0.5f;
                _resTranslationVector.Y = ResolutionRenderer.ViewHeight * 0.5f;
                _resTranslationVector.Z = 0;

                Matrix.CreateTranslation(ref _resTranslationVector, out _resTranslationMatrix);

                _transform = _camTranslationMatrix *
                             _camRotationMatrix *
                             _camScaleMatrix *
                             _resTranslationMatrix *
                             ResolutionRenderer.GetTransformationMatrix();

                _isViewTransformationDirty = false;
            }

            return _transform;
        }

        public void RecalculateTransformationMatrices()
        {
            _isViewTransformationDirty = true;
        }

        /// <summary>
        /// Convert screen coordinates to virtual
        /// </summary>
        /// <param name="coord">Coordinates</param>
        /// <param name="useIrr"></param>
        public Vector2 ToVirtual(Vector2 coord, bool useIrr = true)
        {
            if (useIrr) coord = coord - new Vector2(ResolutionRenderer.Viewport.X, ResolutionRenderer.Viewport.Y);
            return Vector2.Transform(coord, Matrix.Invert(GetViewTransformationMatrix()));
        }

        /// <summary>
        /// Convert virtual coordinates to screen
        /// </summary>
        /// <param name="coord">Coordinates</param>
        public Vector2 ToDisplay(Vector2 coord)
        {
            return Vector2.Transform(coord, GetViewTransformationMatrix());
        }

        public void Dispose()
        {
            ResolutionRenderer = null;
        }

        #region SmoothTransition Logic
        private int _trDuration;
        public bool IsTransitionActive { get; private set; }
        private float _trElapsedTime;
        private Vector2 _trTargetPosition;

        /// <summary>
        /// Start camera transition to specified position
        /// </summary>
        /// <param name="targetPos">Target position</param>
        /// <param name="duration">Expected transition duration</param>
        public void StartTransition(Vector2 targetPos, int duration = 5000)
        {
            if (IsTransitionActive)
                ResetTransition();
            _trTargetPosition = targetPos;
            IsTransitionActive = true;
            _trDuration = duration;
        }
        /// <summary>
        /// Start camera transition to specified position
        /// </summary>
        /// <param name="targetPos">Target position</param>
        /// <param name="duration">Expected transition duration</param>
        public void StartTransition(Point targetPos, int duration = 5000)
        {
            StartTransition(new Vector2(targetPos.X, targetPos.Y), duration);
        }
        /// <summary>
        /// Update transition target position without cancelling current transition
        /// </summary>
        /// <param name="targetPos">Target position</param>
        public void UpdateTransitionTarget(Vector2 targetPos)
        {
            _trTargetPosition = targetPos;
        }
        /// <summary>
        /// Update transition target position without cancelling current transition
        /// </summary>
        /// <param name="targetPos">Target position</param>
        public void UpdateTransitionTarget(Point targetPos)
        {
            _trTargetPosition = new Vector2(targetPos.X, targetPos.Y);
        }
        /// <summary>
        /// Stop and reset transition
        /// </summary>
        public void StopTransition()
        {
            ResetTransition();
            IsTransitionActive = false;
        }
        /// <summary>
        /// Reset current transition
        /// </summary>
        public void ResetTransition()
        {
            _trElapsedTime = 0f;
        }

        private void UpdateTransition(GameTime gt)
        {
            _trElapsedTime += (float)gt.ElapsedGameTime.TotalMilliseconds;
            var amount = MathHelper.Clamp(_trElapsedTime / _trDuration, 0, 1);

            Vector2 result;
            Vector2 cpos = Position;
            Vector2.Lerp(ref cpos, ref _trTargetPosition, amount, out result);
            SetPosition(result);
            if (amount >= 1f)
                StopTransition();
        }
        #endregion

        /// <summary>
        /// Update logic (must be called within main update method)
        /// </summary>
        /// <param name="gt">Game time</param>
        public void Update(GameTime gt)
        {
            if (IsTransitionActive)
                UpdateTransition(gt);
        }
    }    
}