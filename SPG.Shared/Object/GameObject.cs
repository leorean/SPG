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
using SPG.Util;

namespace SPG.Objects
{
    public interface IGameObject
    {

    }

    public class GameObject : IGameObject
    {
        // general props

        public string Name { get; protected set; }
        public int ID { get; protected set; }

        public bool Enabled { get; set; } = true;
        public bool Visible { get; set; } = true;

        // draw/visual

        public Texture2D Texture { get; set; } = null;

        public int Width { get => Texture != null ? Texture.Width : 0; }
        public int Height { get => Texture != null ? Texture.Height : 0; }

        private float angle;
        public float Angle { get => angle; set => angle = value % 360.0f; }

        public Vector2 Scale { get; set; } = new Vector2(1, 1);
        public Color Color { get; set; } = Color.White;

        public float Depth { get; set; } = 1f;

        
        /*
         * TODO:
         * animation
         */

        // world/position/collision

        public Vector2 Position { get; set; } = Vector2.Zero;
        // use positive values because it is used in the drawing primitive
        public Vector2 DrawOffset { get; set; }
        public RectF BoundingBox { get; set; } = new RectF(0, 0, Globals.TILE, Globals.TILE);
        
        public float X { get => Position.X; }
        public float Y { get => Position.Y; }

        public float Left { get => Position.X + BoundingBox.X; }
        public float Right { get => Left + BoundingBox.Width; }
        public float Top { get => Position.Y; }
        public float Bottom { get => Top + BoundingBox.Height; }

        public float XVel { get; set; }
        public float YVel { get; set; }
        public float Gravity { get; set; }
                
        // debug

        public bool DebugEnabled { get; set; }

        // constructor

        public GameObject(float x, float y, string name) : this()
        {
            Position = new Vector2(x, y);
            Name = name;  
        }

        protected GameObject()
        {
            ID = ObjectManager.Add(this);
        }
        
        ~GameObject()
        {
            ObjectManager.Remove(this);
        }

        // methods
        
        public void Move(float x, float y)
        {
            Position = new Vector2(Position.X + x, Position.Y + y);
        }

        public virtual void Update(GameTime gameTime)
        {
            
        }

        public virtual void Draw(GameTime gameTime)
        {
            if (Texture == null)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: object '{Name}'({ID}) has no texture!");                
            } else
            {
                GameManager.Game.SpriteBatch.Draw(Texture, Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
            }

            if (DebugEnabled)
            {
                var rect = new RectF((Position.X + BoundingBox.X) - .5f,
                    (Position.Y + BoundingBox.Y) - .5f,
                    BoundingBox.Width, BoundingBox.Height);

                SPG.Draw.Primitives2D.DrawRectangle(rect, Color.Black, false);
            }
        }

    }
}

