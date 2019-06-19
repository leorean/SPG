using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SPG.Util;
using SPG.Objects;
using Leore.Objects.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using Leore.Main;

namespace Leore.Objects.Level
{
    public class TimeSwitch : GroundSwitch
    {
        private int timer;
        private int maxTimer;
        private float yoff = 16;

        public TimeSwitch(float x, float y, int timer, Room room) : base(x, y, false, room)
        {
            this.timer = timer;
            maxTimer = timer;

            Depth = Globals.LAYER_BG + .0001f;
        }

        public override void Update(GameTime gameTime)
        {
            //base.Update(gameTime);

            if (timer == 0) {

                var proj = this.CollisionBoundsFirstOrDefault<PlayerProjectile>(X, Y);
                if (proj != null)
                {
                    proj.HandleCollision(this);
                    timer = maxTimer;
                }
            }

            if (!Active)
            {
                yoff = Math.Min(yoff + 1f, 16);
                if (yoff == 16 && timer == maxTimer)
                {
                    Active = true;
                }
            }
            else
            {
                timer = Math.Max(timer - 1, 0);
                yoff = timer / 16;
            }

        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //base.Draw(sb, gameTime);
            sb.Draw(AssetManager.Switch[4], Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
            //sb.Draw(AssetManager.Switch[5], Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth + .0001f);
            var partRect = new Rectangle(0, (int)(16 - yoff), 16, (int)yoff);
            sb.Draw(AssetManager.Switch[5], Position, partRect, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth + .0001f);
        }
    }
}
