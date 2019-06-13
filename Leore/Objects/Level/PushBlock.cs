using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Main;
using Leore.Objects.Enemies;
using SPG.Objects;
using SPG.Util;

namespace Leore.Objects.Level
{
    public class PushBlock : Solid
    {
        public bool IsPushing { get; private set; } = false;
        public bool IsFalling { get; private set; } = false;

        private float lastX;
        private float lastY;
        private Direction dir;
        
        public float PushVel { get; private set; } = .5f;

        private bool aboutToFall;
        
        public PushBlock(float x, float y, Room room) : base(x, y, room)
        {
            BoundingBox = new RectF(0, 0, Globals.TILE, Globals.TILE);
            Visible = true;
            Depth = Globals.LAYER_FG + .0001f;

            Gravity = .1f;
        }

        public bool Push(Direction dir)
        {
            if (IsPushing || IsFalling)
                return false;

            this.dir = dir;

            var colX = this.CollisionPointFirstOrDefault<Solid>(X + 8 + Math.Sign((int)dir) * Globals.TILE, Y + 8);

            if (colX != null)
                return false;

            IsPushing = true;
            return true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var T = Globals.TILE;

            aboutToFall = false;

            // "hack" to clean tile solid underneath
            new Solid(X, Y, Room).Destroy();
            
            // x

            if (!IsPushing)
            {
                lastX = X;
            }
            else
            {
                XVel = PushVel * Math.Sign((int)dir);
                if (Math.Abs(lastX - X) >= Globals.TILE)
                {
                    XVel = 0;
                    Position = new Vector2(MathUtil.Div(X, Globals.TILE) * Globals.TILE, Y);
                    IsPushing = false;
                }
            }
            Move(XVel, 0);

            // y

            if (!IsFalling)
            {
                lastY = Y;

                var colY = this.CollisionRectangles<Collider>(X + 8, Y + 8, X + 8, Y + 16).FirstOrDefault();

                if (colY == null)
                {
                    var player = this.CollisionRectangles<Player>(Left, Top + T, Right, Bottom + T).FirstOrDefault();

                    if (player != null)
                        aboutToFall = true;

                    if (player == null && !IsPushing)
                        IsFalling = true;
                }
            }
            else
            {
                if (GameManager.Current.Player.State == Player.PlayerState.PUSH)
                    GameManager.Current.Player.State = Player.PlayerState.IDLE;

                YVel = Math.Min(YVel + Gravity, 3);

                var playerCollision = this.CollisionBounds(GameManager.Current.Player, X, Y + YVel);
                if (playerCollision)
                {
                    Move(0, -1);
                    
                    Position = new Vector2(X, MathUtil.Div(Y, T) * T);
                    IsFalling = false;
                    lastY = Y;
                    
                    XVel = 0;
                    YVel = 0;                    
                }
                
                var enemy = this.CollisionBoundsFirstOrDefault<Enemy>(X, Y);
                if (enemy != null)
                {
                    enemy.Hit(999, 90);
                }

                var colY = this.CollisionRectangleFirstOrDefault<Collider>(X + 8, Y + 8, X + 8, Y + 16);
                if (colY != null)
                {
                    YVel = 0;
                    Position = new Vector2(X, MathUtil.Div(Y, Globals.TILE) * Globals.TILE);
                    IsFalling = false;
                }
            }
            Move(0, YVel);            
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            if (aboutToFall)
            {
                sb.Draw(Texture, Position + new Vector2(0, 16), null, new Color(Color.White, .5f), Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
            }
        }
    }
}
