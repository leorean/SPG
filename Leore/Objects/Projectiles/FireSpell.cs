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
        private float maxPower;

        private int delay;
        private int maxDelay = 5;
        
        private double t;

        private SpellLevel level;

        public FireSpell(float x, float y, SpellLevel level) : base(x, y)
        {
            switch (level)
            {
                case SpellLevel.ONE:
                    maxPower = 1 * 60;
                    break;
                case SpellLevel.TWO:
                    maxPower = 3 * 60;
                    break;
                case SpellLevel.THREE:
                    maxPower = 1 * 60;
                    break;
            }

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

                player.MP = Math.Max(player.MP - GameResources.MPCost[SpellType.FIRE][level], 0);

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
                            var tVel = .3f;

                            new FireProjectile2(X, Y, player.Direction, player.LookDirection, 0, 3f, 0, tVel) { IsPrimary = true };
                            
                            if (p > .25f)
                            {
                                new FireProjectile2(X, Y, player.Direction, player.LookDirection, 1f, 2.5f, 1.5f * (float)Math.PI, tVel) { Scale = new Vector2(.4f), Damage = 1 };
                                new FireProjectile2(X, Y, player.Direction, player.LookDirection, 1f, 2.5f, 0.5f * (float)Math.PI, tVel) { Scale = new Vector2(.4f), Damage = 1 };
                            }
                            if (p > .5f)
                            {
                                new FireProjectile2(X, Y, player.Direction, player.LookDirection, 1f, 3.2f, 1f * (float)Math.PI, tVel) { Scale = new Vector2(.4f), Damage = 1 };
                                new FireProjectile2(X, Y, player.Direction, player.LookDirection, 1f, 3.2f, 2f * (float)Math.PI, tVel) { Scale = new Vector2(.4f), Damage = 1 };
                            }
                            delay = 10 + (int)(15 * (1 - p));
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
            
            if (orb.State != OrbState.ATTACK || level != orb.Level || player.MP < GameResources.MPCost[SpellType.FIRE][level])
            {
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
