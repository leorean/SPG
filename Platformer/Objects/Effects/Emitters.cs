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
    public class GlobalWaterEmitter : ParticleEmitter
    {
        public List<Color> ParticleColors;

        private float sinus;

        public GlobalWaterEmitter(float x, float y, GameObject parent) : base(x, y)
        {
            Parent = parent;

            ParticleColors = new List<Color>();
            ParticleColors.Add(new Color(255, 255, 255));
            ParticleColors.Add(new Color(206, 255, 255));
            ParticleColors.Add(new Color(168, 248, 248));
            ParticleColors.Add(new Color(104, 216, 248));

            SpawnRate = .5f;
            
            ParticleInit = (particle) =>
            {
                var room = MainGame.Current.Camera.CurrentRoom;

                var posX = room.X + RND.Int((int)room.BoundingBox.Width);
                var posY = room.Y + RND.Int((int)room.BoundingBox.Height);

                int tx = MathUtil.Div(posX, Globals.TILE);
                int ty = MathUtil.Div(posY, Globals.TILE);

                var inWater = (MainGame.Current.Map.LayerData[2].Get(tx, ty) != null);

                if (this.CollisionPoint<Solid>(posX, posY).Count > 0)
                {
                    inWater = false;
                }
                
                particle.Alpha = 0;

                if (!inWater)
                {
                    particle.LifeTime = 0;                    
                } else
                {
                    particle.Scale = new Vector2(.5f, .5f);                    
                    particle.LifeTime = 999;
                    particle.Position = new Vector2(posX, posY);
                    particle.CustomProperties.Add("t", RND.Next * 2 * Math.PI);
                }
            };

            ParticleUpdate = (particle) =>
            {
                var offset = (double)particle.CustomProperties["t"];

                var t = (offset + sinus) % (2 * Math.PI);

                particle.YVel = Math.Max(particle.YVel - .01f + (float)(RND.Next * .01f), -.25f);                
                particle.XVel = (float)Math.Sin(t) * .05f;

                particle.XVel = particle.XVel.Clamp(-.25f, .25f);

                var s = Math.Min(particle.Scale.X + .015f, 2);
                var a = Math.Min(particle.Alpha + .015f, .8f);

                particle.Scale = new Vector2(s);
                particle.Alpha = a;

                int tx = MathUtil.Div(particle.Position.X, Globals.TILE);
                int ty = MathUtil.Div(particle.Position.Y, Globals.TILE);

                // destroy:

                var inWater = (MainGame.Current.Map.LayerData[2].Get(tx, ty) != null);
                if (this.CollisionPoint<Solid>(particle.Position.X, particle.Position.Y).Count > 0)
                    particle.LifeTime = 0;

                if (!inWater)
                    particle.LifeTime = 0;                
            };
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            sinus = (float)((sinus + .1f) % (2 * Math.PI));
        }
    }

    public class PlayerLevitationEmitter : ParticleEmitter
    {
        public List<Color> ParticleColors;

        public PlayerLevitationEmitter(float x, float y, GameObject parent) : base(x, y)
        {
            Parent = parent;

            ParticleColors = new List<Color>();
            ParticleColors.Add(new Color(255, 255, 255));
            ParticleColors.Add(new Color(217, 255, 152));
            ParticleColors.Add(new Color(177, 255, 116));
            ParticleColors.Add(new Color(131, 237, 100));

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
                
                var colorIndex = RND.Int(ParticleColors.Count - 1);
                particle.Color = ParticleColors[colorIndex];
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
        public List<Color> ParticleColors;

        public SaveStatueEmitter(float x, float y) : base(x, y)
        {
            ParticleColors = new List<Color>();
            ParticleColors.Add(new Color(255, 255, 255));
            ParticleColors.Add(new Color(206, 255, 255));
            ParticleColors.Add(new Color(168, 248, 248));
            ParticleColors.Add(new Color(104, 216, 248));

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

                var colorIndex = RND.Int(ParticleColors.Count - 1);
                particle.Color = ParticleColors[colorIndex];
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
}
