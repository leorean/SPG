using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Leore.Objects.Level.Blocks;
using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Map;
using Leore.Objects.Effects.Emitters;
using Microsoft.Xna.Framework.Graphics;
using Leore.Util;
using Leore.Objects.Effects;
using SPG.Util;
using Leore.Resources;

namespace Leore.Objects.Projectiles
{
    public class FireSpell : GameObject
    {
        private Player player => GameManager.Current.Player;
        private Orb orb => GameManager.Current.Player.Orb;

        private static FireSpell instance;
        
        private float power;
        private float maxPower = 1 * 60;

        private int delay;
        private int maxDelay = 5;
        
        private double t;

        private SpellLevel level;

        public FireSpell(float x, float y, SpellLevel level) : base(x, y)
        {

            this.level = level;

            new CrimsonBurstEmitter(orb.X, orb.Y) { ParticleColors = GameResources.FireColors };

            orb.Visible = false;
            Depth = player.Depth + .0002f;            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            power = Math.Min(power + 1, maxPower);

            t = (t + .5f) % (Math.PI * 2);

            Angle = (Angle + .2f) % (int)(2 * Math.PI);

            var p = power / maxPower;

            Position = player.Position + new Vector2(7 * Math.Sign((int)player.Direction), 0);
            
            delay = Math.Max(delay - 1, 0);

            if (delay == 0)
            {
                switch (level)
                {
                    case SpellLevel.ONE:
                        {
                            var proj = new FireProjectile1(X, Y);
                            proj.XVel = Math.Sign((int)player.Direction) * (1.5f - Math.Abs(.5f * (int)player.LookDirection));
                            proj.YVel = Math.Max(-2f + 1f * (int)player.LookDirection, -2.5f);
                            
                            delay = (int)(20 + (1 - p) * 20);
                        }
                        break;

                    case SpellLevel.TWO:
                        {
                            var proj = new FireProjectile2(X, Y, player.LookDirection);
                            proj.XVel = Math.Sign((int)player.Direction) * 2f;
                            proj.YVel = Math.Sign((int)player.LookDirection) * 1f;
                            delay = (int)(20 + (1 - p) * 20);
                        }
                        break;

                    case SpellLevel.THREE:
                        {
                            var proj = new FireProjectile3(X, Y);
                            proj.XVel = Math.Sign((int)player.Direction) * (.75f + 2.5f * p) + player.XVel;
                            proj.YVel = -.3f * (float)Math.Sin(t) + player.YVel + 1f * (int)player.LookDirection;

                            delay = (int)(maxDelay * (1 - .75f * p) * 3);
                        }
                        break;
                }
            }

            if (orb.State != OrbState.ATTACK || level != orb.Level)
            {
                // TODO

                //if (power == maxPower)
                //{
                //    if (level == SpellLevel.THREE)
                //    {
                //        for (var i = -1; i < 2; i++)
                //        {
                //            var proj = new FireProjectile(X, Y + i * 4, level);
                //            proj.XVel = 4 * Math.Sign((int)player.Direction);
                //            proj.YVel = 2.8f * (int)player.LookDirection;                            
                //        }
                //    }
                //}

                new CrimsonBurstEmitter(orb.X, orb.Y) { ParticleColors = GameResources.FireColors };

                Destroy();
            }            
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            //sb.Draw(AssetManager.FireBall, Position, null, Color, Angle, new Vector2(16), new Vector2(.5f + .5f * power / maxPower), SpriteEffects.None, Depth + .0001f);

            if (power / maxPower > 0)
                sb.DrawBar(player.Position + new Vector2(0, -16), 24, power / maxPower, Color.White, Color.Black, orb.Depth + .0001f, 2, false);
        }
        
        public override void Destroy(bool callGC = false)
        {
            orb.Visible = true;
            instance = null;
            base.Destroy(callGC);
        }
        
        public static void Create(float x, float y, SpellLevel level)
        {
            if (instance == null)
                instance = new FireSpell(x, y, level);
        }
    }
}
