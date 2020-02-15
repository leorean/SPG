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
    public class FireSpell : SpellObject
    {
        private Player player => GameManager.Current.Player;
        private Orb orb => GameManager.Current.Player.Orb;

        private static FireSpell instance;
        
        private float curPower;
        private float maxPower;
        public float Power { get; private set; }

        private int delay;
        
        private double t;
        
        private int iteration;
        
        public int ArcSpeed { get; set; } = 10;
        public int ArcAngle { get; set; }
        public List<FireArcProjectile> ArcProjectiles { get; set; }  = new List<FireArcProjectile>();

        public FireSpell(float x, float y, SpellLevel level) : base(x, y, level)
        {
            switch (level)
            {
                case SpellLevel.ONE:
                    maxPower = 1 * 60;
                    break;
                case SpellLevel.TWO:
                    maxPower = 4 * 60;
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
            
            curPower = Math.Min(curPower + 1, maxPower);

            t = (t + .5f) % (Math.PI * 2);

            Angle = (Angle + .2f) % (int)(2 * Math.PI);

            ArcAngle = (ArcAngle + ArcSpeed) % 360;

            Power = curPower / maxPower;

            Position = player.Position + new Vector2(7 * Math.Sign((int)player.Direction), 0);
            
            delay = Math.Max(delay - 1, 0);

            if (delay == 0)
            {
                
                switch (level)
                {
                    case SpellLevel.ONE:
                        {
                            if (player.MP > 0)
                            {
                                FireBallProjectile proj = null;
                                if (iteration == 0)
                                {
                                    SubtractMP();
                                    proj = new FireBallProjectile(X, Y);
                                    proj.XVel = Math.Sign((int)player.Direction) * 1.5f;
                                    proj.YVel = -1f;
                                }
                                if (iteration == 1)
                                {
                                    SubtractMP();
                                    proj = new FireBallProjectile(X, Y);
                                    proj.XVel = Math.Sign((int)player.Direction) * 1.5f;
                                    proj.YVel = -1.5f;
                                }
                                if (iteration == 2)
                                {
                                    SubtractMP();
                                    proj = new FireBallProjectile(X, Y);
                                    proj.XVel = Math.Sign((int)player.Direction) * 1.35f;
                                    proj.YVel = -1.75f;
                                }

                                if (proj != null)
                                {
                                    if (player.LookDirection == Direction.UP)
                                    {
                                        proj.XVel *= .5f;
                                        proj.YVel -= .5f;
                                    }
                                    if (player.LookDirection == Direction.DOWN)
                                    {
                                        proj.XVel *= .5f;
                                        proj.YVel += 1f;
                                    }
                                }

                                iteration = (iteration + 1) % 6;
                                delay = 8;
                            }
                            else
                                Destroy();
                        }
                        break;
                    case SpellLevel.TWO:
                        {

                            bool enoughMP = player.MP >= GameResources.MPCost[SpellType.FIRE][level];
                            if (ArcProjectiles.Count < 3 && enoughMP)
                            {
                                SubtractMP();

                                var proj = new FireArcProjectile(X, Y, this);
                                delay = 30;
                                ArcProjectiles.Add(proj);
                            }
                        }
                        break;
                    case SpellLevel.THREE:
                        {
                            SubtractMP();

                            if (player.MP > 0)
                            {

                                var proj = new FlameThrowerProjectile(X, Y);
                                proj.XVel = Math.Sign((int)player.Direction) * (1.75f + 1.5f * Power) + player.XVel;
                                proj.YVel = -.3f * (float)Math.Sin(t) + player.YVel + 1f * (int)player.LookDirection;

                                delay = (int)((.5f - .25f * Power) * 15);
                            }
                            else
                                Destroy();
                        }
                        break;
                }
            }
            
            if (orb.State != OrbState.ATTACK || orb.Type != SpellType.FIRE || level != orb.Level)
            {
                Destroy();
            }            
        }

        private void SubtractMP()
        {
            player.MP = Math.Max(player.MP - GameResources.MPCost[SpellType.FIRE][level], 0);
        }


        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);
        }
        
        public override void Destroy(bool callGC = false)
        {
            new SaveBurstEmitter(orb.X, orb.Y) { ParticleColors = GameResources.FireColors };

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
