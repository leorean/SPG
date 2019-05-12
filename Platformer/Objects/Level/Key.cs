using Microsoft.Xna.Framework;
using Platformer.Objects.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPG.Objects;

namespace Platformer.Objects.Level
{
    public class Key : Platform
    {
        public bool Persistent { get; set; }

        private Player player { get => Parent as Player; }

        private bool stuck = false;
        
        public Key(float x, float y, Room room) : base(x, y, room)
        {
            Depth = Globals.LAYER_PLAYER + .001f;

            Visible = true;
            DebugEnabled = true;
            DrawOffset = new Vector2(8, 8);
            BoundingBox = new SPG.Util.RectF(-4, -8, 8, 16);            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            YVel += .1f;
            if (player != null)
            {
                XVel = 0;
                YVel = 0;
                Position = new Vector2(player.X, player.Y - Globals.TILE);
            }

            var colX = this.CollisionBounds<Solid>(X + XVel, Y).FirstOrDefault();

            if (colX == null)
            {
                Move(XVel, 0);
            } else
            {
                XVel = 0;
            }

            var colY = this.CollisionBounds<Solid>(X, Y + YVel).FirstOrDefault();

            if (colY == null)
            {
                Move(0, YVel);
            }
            else
            {
                YVel = 0;
            }

            if (stuck)
            {
                var tx = (GameManager.Current.Player.X - X) / 8f;
                var ty = (GameManager.Current.Player.Y - Y) / 8f;
                Move(tx, ty);

                var col = this.CollisionBounds<Solid>(X, Y).FirstOrDefault();
                if (col == null)
                    stuck = false;
            }
        }

        public void Take(Player player)
        {
            Parent = player;
        }

        public void Throw()
        {
            Position = new Vector2(X + Math.Sign((int)player.Direction) * Globals.TILE, player.Y - Globals.TILE);
            var emitter = new Effects.Emitters.SaveBurstEmitter(X, Y);

            var col = this.CollisionBounds<Solid>(X + XVel, Y + YVel).FirstOrDefault();

            if (col != null)
                stuck = true;

            Parent = null;
        }

        /*public override void Destroy(bool callGC = false)
        {
            if (!Persistent)
            {
                base.Destroy(callGC);
            }
        }*/
    }
}
