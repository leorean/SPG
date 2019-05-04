using Microsoft.Xna.Framework;
using Platformer.Objects.Level;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Effects
{
    /*
    public class WaterSplashEmitter : ParticleEmitter
    {
        public WaterSplashEmitter(float x, float y) : base(x, y)
        {
            particleColors = new List<Color>();
            particleColors.Add(new Color(255, 255, 255));
            particleColors.Add(new Color(206, 255, 255));
            particleColors.Add(new Color(168, 248, 248));
            particleColors.Add(new Color(104, 216, 248));

            SpawnRate = 10f;

            ParticleInit = (particle) =>
            {
                SpawnRate = 0;

                particle.Scale = new Vector2(2f, 2f);
                particle.LifeTime = 120;

                particle.Position = new Vector2(X - 6 + (float)(RND.Next * 12), Y);

                particle.Alpha = 1;
                particle.Angle = (float)((RND.Next * 360) / (2 * Math.PI));

                particle.XVel = -1 + (float)(RND.Next * 2f);
                particle.YVel = -1.5f - (float)(RND.Next * 1f);

                var colorIndex = RND.Int(particleColors.Count - 1);
                particle.Color = particleColors[colorIndex];
            };

            ParticleUpdate = (particle) =>
            {
                int tx = MathUtil.Div(particle.Position.X, Globals.TILE);
                int ty = MathUtil.Div(particle.Position.Y + particle.YVel, Globals.TILE);
                
                // destroy:

                var inWater = (MainGame.Current.Map.LayerData[2].Get(tx, ty) != null);
                if (this.CollisionPoint<Solid>(particle.Position.X, particle.Position.Y).Count > 0)
                    particle.LifeTime = 0;

                if (inWater)
                {
                    particle.YVel *= .6f;
                    particle.XVel *= .9f;
                    particle.Alpha = Math.Max(particle.Alpha - .04f, 0);
                } else
                {
                    particle.YVel = Math.Min(particle.YVel + .15f, 3f);
                }

                if (particle.Alpha == 0)
                    particle.LifeTime = 0;
            };
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (particles.Count == 0)
                Destroy();
        }
    }

    public class PlayerLevitationEmitter : ParticleEmitter
    {
        public PlayerLevitationEmitter(float x, float y, GameObject parent) : base(x, y)
        {
            Parent = parent;

            particleColors = new List<Color>();
            particleColors.Add(new Color(255, 255, 255));
            particleColors.Add(new Color(217, 255, 152));
            particleColors.Add(new Color(177, 255, 116));
            particleColors.Add(new Color(131, 237, 100));

            SpawnRate = .5f;
            
            ParticleInit = (particle) =>
            {
                particle.LifeTime = 30;
                
                particle.Scale = new Vector2(3, 3);

                particle.Angle = (float)(RND.Next * 360);

                float spd = (float)(1 + RND.Next * .5f);

                float posX = (float)MathUtil.LengthDirX(particle.Angle) * 8;
                float posY = (float)MathUtil.LengthDirY(particle.Angle) * 8;

                particle.Position = new Vector2(X + posX, Y + posY);

                particle.YVel = 2;
                
                var colorIndex = RND.Int(particleColors.Count - 1);
                particle.Color = particleColors[colorIndex];
            };

            ParticleUpdate = (particle) =>
            {
                var s = Math.Max(3 * particle.LifeTime / 30f, 1);

                particle.XVel *= .9f;
                particle.YVel *= .9f;

                particle.Alpha = particle.LifeTime / 15f;

                particle.Scale = new Vector2(s);
            };
        }
    }

    // save statue emitter

    public class SaveStatueEmitter : ParticleEmitter
    {
        public SaveStatueEmitter(float x, float y) : base(x, y)
        {
            particleColors = new List<Color>();
            particleColors.Add(new Color(255, 255, 255));
            particleColors.Add(new Color(206, 255, 255));
            particleColors.Add(new Color(168, 248, 248));
            particleColors.Add(new Color(104, 216, 248));

            SpawnRate = 50;

            ParticleInit = (particle) =>
            {
                particle.LifeTime = 30;

                SpawnRate = 0;
                
                particle.Scale = new Vector2(3, 3);

                particle.Angle = (float)(RND.Next * 360);

                float spd = (float)(1 + RND.Next * .5f);

                particle.XVel = (float)MathUtil.LengthDirX(particle.Angle) * spd;
                particle.YVel = (float)MathUtil.LengthDirY(particle.Angle) * spd;

                var colorIndex = RND.Int(particleColors.Count - 1);
                particle.Color = particleColors[colorIndex];
            };

            ParticleUpdate = (particle) =>
            {
                var s = Math.Max(3 * particle.LifeTime / 30f, 1);

                particle.XVel *= .9f;
                particle.YVel *= .9f;

                particle.Alpha = particle.LifeTime / 15f;

                particle.Scale = new Vector2(s);
            };
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (particles.Count == 0)
                Destroy();
        }
    }
    */
}
