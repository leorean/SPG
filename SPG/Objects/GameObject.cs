using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SPG;
using SPG.Draw;
using SPG.Map;
using SPG.Util;

namespace SPG.Objects
{
    public interface IGameObject
    {
        
    }

    public abstract class GameObject : IGameObject
    {
        // general props

        public string Name { get; set; }

        public GameObject Parent { get; set; } = null;

        /// <summary>
        /// If not overridden, the ID will be created based on the coordinates of the object.
        /// </summary>
        public int ID { get; set; }
        
        private bool _enabled;
        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }
        public bool Visible { get; set; } = true;

        // draw/visual

        private TextureSet _frames;

        private double _lastFrame = 0;
        private double _currentFrame = 0;
        public int AnimationFrame
        {
            get => MinFrame + (int)Math.Floor(_currentFrame);
        }
        public int MinFrame { get; protected set; }
        public int MaxFrame { get; protected set; }
        public double AnimationSpeed { get; protected set; }
        private bool _isLooped = false;

        public Texture2D Texture
        {
            get => (_frames != null) ? _frames[AnimationFrame] : null;
            set
            {
                if (_frames == null || value != _frames[0])
                    _frames = TextureSet.FromTexture(value);
                _currentFrame = 0;
            }
        }

        public event EventHandler AnimationComplete;

        public TextureSet AnimationTexture { get { return _frames; } set { _frames = value; } }
        
        public int Width { get => Texture != null ? Texture.Width : 0; }
        public int Height { get => Texture != null ? Texture.Height : 0; }

        private float angle;
        public float Angle { get => angle; set => angle = value % 360.0f; }

        public Vector2 Scale { get; set; } = new Vector2(1, 1);
        public Color Color { get; set; } = Color.White;

        public float Depth { get; set; } = 1f;
        
        // world/position/collision

        public Vector2 Position { get; set; } = Vector2.Zero;

        /// <summary>
        /// Returns the center of the boundingBox.
        /// </summary>
        public Vector2 Center { get => new Vector2(X + BoundingBox.X + BoundingBox.Width * .5f, Y + BoundingBox.Y + BoundingBox.Height * .5f); }

        /// <summary>
        /// Sets the draw offset. Use positive values because it is used in the drawing primitive 
        /// </summary>
        public Vector2 DrawOffset { get; set; }
        public RectF BoundingBox { get; set; } = new RectF(0, 0, Globals.TILE, Globals.TILE);
        
        public float X { get => Position.X; }
        public float Y { get => Position.Y; }

        public float Left { get => Position.X + BoundingBox.X; }
        public float Right { get => Left + BoundingBox.Width; }
        public float Top { get => Position.Y + BoundingBox.Y; }
        public float Bottom { get => Top + BoundingBox.Height; }

        public float XVel { get; set; }
        public float YVel { get; set; }
        public float Gravity { get; set; }
                
        // debug

        public bool DebugEnabled { get; set; }

        // constructor
        
        public GameObject(float x, float y, string name = null) : this()
        {
            Position = new Vector2(x, y);
            Name = name == null ? name : GetType().Name;
            this.CreateID();

            //Debug.WriteLine($"Set ID '{ID}' to object of type '{GetType()}'.");
        }
        
        private GameObject()
        {
            ObjectManager.Add(this);
            Enabled = true;
        }
        
        ~GameObject()
        {
            ObjectManager.Remove(this);
        }

        // methods
        
        public void ResetAnimation()
        {
            _currentFrame = 0;
            _lastFrame = 0;
        }

        public void SetAnimation(int minFrame, int maxFrame, double animationSpeed, bool loop)
        {
            // reset 
            if (MinFrame != minFrame || MaxFrame != maxFrame)
            {
                _lastFrame = 0;
                _currentFrame = 0;
            }

            MinFrame = minFrame;
            MaxFrame = maxFrame;
            AnimationSpeed = animationSpeed;
            _isLooped = loop;
        }
        
        /// <summary>
        /// Unregisters a game object from the object manager. 
        /// If that game object is child to another game object, it is not destroyed until the parent is destroyed.
        /// Optionally calls GC afterwards.        
        /// </summary>
        public void Destroy(bool callGC = false)
        {
            if (Parent != null && ObjectManager.Objects.Contains(Parent))
                return;

            List<GameObject> children = ObjectManager.Objects.Where(o => o.Parent == this).ToList();
            
            GameObject[] copy = new GameObject[children.Count];
            children.CopyTo(copy);

            foreach(var c in copy)
            {
                c.RemoveAndCallGC(callGC);
            }

            RemoveAndCallGC(callGC);
        }

        private void RemoveAndCallGC(bool callGC)
        {
            ObjectManager.Remove(this);
            if (callGC)
                GC.Collect();
        }

        public void Move(float xVel, float yVel)
        {
            Position = new Vector2(Position.X + xVel, Position.Y + yVel);
        }

        /// <summary>
        /// Moves towards a target center in N steps. 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="n"></param>
        public void MoveTowards(GameObject target, int n)
        {
            var tx = (target.Center.X - Center.X) / n;
            var ty = (target.Center.Y - Center.Y) / n;
            Move(tx, ty);
        }

        public virtual void BeginUpdate(GameTime gameTime) { }
        
        public virtual void EndUpdate(GameTime gameTime) { }

        public virtual void Update(GameTime gameTime)
        {
            if (MaxFrame > MinFrame && MaxFrame > 0)
            {
                if (_isLooped)
                {
                    var last = _currentFrame;
                    _currentFrame = _currentFrame + AnimationSpeed;
                    if (_currentFrame > (MaxFrame - MinFrame) + 1)
                    {
                        _currentFrame -= ((MaxFrame - MinFrame) + 1);
                        AnimationComplete?.Invoke(this, new EventArgs());
                    }
                }
                else
                {                    
                    _currentFrame = Math.Min(_currentFrame + AnimationSpeed, MaxFrame - MinFrame);

                    if (_currentFrame == MaxFrame - MinFrame && _currentFrame != _lastFrame)
                        AnimationComplete?.Invoke(this, new EventArgs());
                }
            }
            _lastFrame = _currentFrame;
        }

        public virtual void Draw(SpriteBatch sb, GameTime gameTime)
        {            
            if (Texture != null)
            {
                sb.Draw(Texture, Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
            }

            if (DebugEnabled)
            {
                var rect = new RectF((Position.X + BoundingBox.X) - .5f,
                    (Position.Y + BoundingBox.Y) - .5f,
                    BoundingBox.Width, BoundingBox.Height);

                sb.DrawRectangle(rect, Enabled ? Color.Black : Color.Gray, false);
                sb.DrawPixel(Center.X, Center.Y, Color.Red);
            }
        }        
    }
}