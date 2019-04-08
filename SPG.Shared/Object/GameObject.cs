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
    public class GameObject
    {
        public string Name { get; protected set; }
        public int ID { get; protected set; }

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
        
        public float X
        {
            get => Position.X;
            set => Position = new Vector2(value, Position.Y);
        }

        public float Y
        {
            get => Position.Y;
            set => Position = new Vector2(Position.X, value);
        }

        public float XVel { get; set; }
        public float YVel { get; set; }
        public float Gravity { get; set; }
                
        // debug

        private bool debug;

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
        
        public void SetDebug(bool debug)
        {
            this.debug = debug;
        }

        /*
        /// <summary>
        /// Automatically sets the pivot to half of the texture size.
        /// </summary>
        public void CenterAuto()
        {
            Pivot = new Vector2(Width * .5f, Height * .5f);
        }*/

        /*
        /// <summary>
        /// Returns the ID of the colliding object. -1 if no collision.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int Collision(GameObject other)
        {            
            if (CollisionRect.Width > 0 && other.Width > 0 && CollisionRect.Height > 0 && other.Height > 0)
            {
                // x = 500, bounds = (20, 20), pivot = (10, 10)
                // 

                var left = CollisionRect.X;
                var right = CollisionRect.X + CollisionRect.Width;
                var top = CollisionRect.Y;
                var bottom = CollisionRect.Y + CollisionRect.Height;

                var oleft = other.CollisionRect.X;
                var oright = other.CollisionRect.X + other.CollisionRect.Width;
                var otop = other.CollisionRect.Y;
                var obottom = other.CollisionRect.Y + other.CollisionRect.Height;

                if ((left >= oleft || right <= oright) && (top >= otop || bottom <= obottom))
                {
                    return other.ID;
                }
            }

            return -1;
        }*/

        public virtual void Update()
        {

            /*Vector3 rayStart = new Vector3(Position, 0.0f);
            Vector3 pointOffset = new Vector3(Position.X + 10, Position.Y, 0.0f);
            Vector3 direction = rayStart - pointOffset;
            Ray bulletRay = new Ray(rayStart, direction);

            float? output;

            bulletRay.Intersects(ref box, out output);
            if (output.HasValue)
            {
                wallInWay = true;
            }*/
        }

        public virtual void Draw()
        {
            if (Texture == null)
            {
                Debug.WriteLine($"Warning: object '{Name}'({ID}) has no texture!");                
            } else
            {
                GameManager.Game.SpriteBatch.Draw(Texture, Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
            }

            if (debug)
            {
                var rect = new Rectangle((int)(Position.X + BoundingBox.X),
                    (int)(Position.Y + BoundingBox.Y),
                    (int)BoundingBox.Width, (int)BoundingBox.Height);                
                SPG.Util.Draw.DrawRectangle(rect, Color.Black, Depth);                
            }
        }

    }
}

