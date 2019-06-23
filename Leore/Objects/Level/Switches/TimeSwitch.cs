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
using Leore.Objects.Effects;
using Leore.Objects.Effects.Emitters;

namespace Leore.Objects.Level.Switches
{
    public class TimeSwitch : GroundSwitch
    {
        private float timer;
        private float maxTimer;
        private float dist;
        private float maxDist = 4;
        private float z;

        private PlayerProjectile lastProj;

        private bool prepared = true;

        public TimeSwitch(float x, float y, int timer, Room room) : base(x, y, false, room)
        {
            this.timer = timer;
            maxTimer = timer;

            BoundingBox = new RectF(0, 0, 16, 16);
            DrawOffset = Vector2.Zero;            

            Depth = Globals.LAYER_BG + .0001f;
        }

        public override void Update(GameTime gameTime)
        {
            //base.Update(gameTime);

            if (prepared)
            {
                var proj = this.CollisionBoundsFirstOrDefault<PlayerProjectile>(X, Y);
                if (proj != null && lastProj != proj)
                {
                    new SingularEffect(Center.X, Center.Y + z, 10);

                    proj.HandleCollision(this);
                    lastProj = proj;
                    timer = maxTimer;
                    Active = true;
                    prepared = false;
                }
            }

            if (!Active)
            {
                dist = Math.Max(dist - 1f, 0);                
            }
            else
            {
                if (!prepared)
                {
                    dist = Math.Min(dist + .3f, maxDist);
                    if (dist == maxDist)
                        prepared = true;
                }
                else
                {
                    timer = Math.Max(timer - 1, 0);
                    dist = (timer / maxTimer) * maxDist;

                    if (timer == 0)
                    {
                        new StarEmitter(Center.X, Center.Y, 3);
                        Active = false;
                    }
                }
            }
            z = -dist * 1.5f;
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);
            sb.Draw(AssetManager.TimeSwitch[0], Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
            sb.Draw(AssetManager.TimeSwitch[1], Position + new Vector2(0, z), null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth + .0001f);

            // lt, rt, lb, rb

            sb.Draw(AssetManager.TimeSwitch[2], Position + new Vector2(-8, -8) + new Vector2(-dist, -dist) + new Vector2(0, z), null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth + .0002f);
            sb.Draw(AssetManager.TimeSwitch[3], Position + new Vector2(+8, -8) + new Vector2(+dist, -dist) + new Vector2(0, z), null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth + .0002f);
            sb.Draw(AssetManager.TimeSwitch[4], Position + new Vector2(-8, +8) + new Vector2(-dist, +dist) + new Vector2(0, z), null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth + .0002f);
            sb.Draw(AssetManager.TimeSwitch[5], Position + new Vector2(+8, +8) + new Vector2(+dist, +dist) + new Vector2(0, z), null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth + .0002f);
            
            sb.Draw(AssetManager.TimeSwitch[6], Position + new Vector2(-8, -8) + new Vector2(-dist * .5f, -dist * .5f) + new Vector2(0, z), null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth - .0003f);
            sb.Draw(AssetManager.TimeSwitch[7], Position + new Vector2(+8, -8) + new Vector2(+dist * .5f, -dist * .5f) + new Vector2(0, z), null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth - .0003f);
            sb.Draw(AssetManager.TimeSwitch[8], Position + new Vector2(-8, +8) + new Vector2(-dist * .5f, +dist * .5f) + new Vector2(0, z), null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth - .0003f);
            sb.Draw(AssetManager.TimeSwitch[9], Position + new Vector2(+8, +8) + new Vector2(+dist * .5f, +dist * .5f) + new Vector2(0, z), null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth - .0003f);

            sb.Draw(AssetManager.TimeSwitch[10], Position + new Vector2(0, z), null, new Color(Color, dist/maxDist * .5f), Angle, DrawOffset, Scale, SpriteEffects.None, Depth - .0002f);
        }
    }
}
