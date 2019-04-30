using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SPG.Objects;

namespace Platformer.Objects
{
    public class PushBlock : Solid
    {
        public PushBlock(float x, float y, Room room) : base(x, y, room)
        {
            BoundingBox = new SPG.Util.RectF(.1f, 0, Globals.TILE - .1f, Globals.TILE - .1f);
            Visible = true;
            Depth = Globals.LAYER_FG - .001f;

            Gravity = .1f;

            DebugEnabled = true;
        }

        public bool Push(float vel)
        {
            /*var colX = this.CollisionRectangle<Solid>(X + vel, Y + 1, X + Width + vel, Y + Height - 2).FirstOrDefault();

            if (colX != null)
                return false;
            else
                Move(vel, 0);
            */

            XVel = vel;

            var colX = this.CollisionBounds<Solid>(X + vel, Y).FirstOrDefault();

            if (colX != null)
                return false;
            
            return true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var colX = this.CollisionBounds<Solid>(X + XVel, Y).FirstOrDefault();

            if (colX == null)
            {
                Move(XVel, 0);
            }
            else
            {
                XVel = 0;
            }


            YVel = Math.Min(YVel + Gravity, 3);

            var colY = this.CollisionBounds<Solid>(X, Y + YVel).FirstOrDefault();
            
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
