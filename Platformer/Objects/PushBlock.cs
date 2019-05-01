using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Platformer.Objects.Main;
using SPG.Objects;
using SPG.Util;

namespace Platformer.Objects
{
    public class PushBlock : Solid
    {
        private bool isPushing = false;
        private float lastX;
        private Direction dir;
        private int freezeTimeout = 0;

        public PushBlock(float x, float y, Room room) : base(x, y, room)
        {
            BoundingBox = new SPG.Util.RectF(0, 0, Globals.TILE, Globals.TILE);
            Visible = true;
            Depth = Globals.LAYER_FG - .001f;

            Gravity = .1f;            
        }

        public void Push(Direction dir)
        {
            if (isPushing)
                return;

            this.dir = dir;

            var colX = this.CollisionPoint<Solid>(X + 8 + Math.Sign((int)dir) * Globals.TILE, Y + 8).FirstOrDefault();

            if (colX != null)
                return;

            isPushing = true;            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var t = Globals.TILE;
            
            if (!isPushing)
            {
                lastX = X;
            }
            else
            {
                XVel = Math.Sign((int)dir);
                if (Math.Abs(lastX - X) >= Globals.TILE)
                {
                    XVel = 0;
                    Position = new Vector2(MathUtil.Div(X, Globals.TILE) * Globals.TILE, Y);
                    isPushing = false;
                }
            }
            Move(XVel, 0);


            var player = this.CollisionRectangle<Player>(Left, Top + t, Right, Bottom + t).FirstOrDefault();
                
            
            if (player != null)
            {
                isPushing = true;
                //Position = new Vector2(X, MathUtil.Div(Y, Globals.TILE) * Globals.TILE);
            }

            YVel = Math.Min(YVel + Gravity, 3);
            if (!isPushing)
            {
                if (YVel > 0)
                {
                    var colY = this.CollisionPoint<Solid>(X + 8, Y + Globals.TILE + YVel).FirstOrDefault();

                    if (colY == null)
                    {
                        Move(0, YVel);

                    }
                    else
                    {
                        YVel = 0;
                    }
                }
            }
        }
    }
}
