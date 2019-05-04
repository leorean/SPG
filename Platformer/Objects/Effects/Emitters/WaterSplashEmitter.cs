﻿using Microsoft.Xna.Framework;
using Platformer.Objects.Level;
using Platformer.Objects.Main;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Effects.Emitters
{
    public class WaterSplashParticle : Particle
    {
        public WaterSplashParticle(ParticleEmitter emitter, float xVel = 0) : base(emitter)
        {
            Scale = new Vector2(2f, 2f);
            LifeTime = 120;

            Position = new Vector2(emitter.X - 6 + (float)(RND.Next * 12), emitter.Y);

            Alpha = 1;
            Angle = (float)((RND.Next * 360) / (2 * Math.PI));

            XVel = -1 + (float)(RND.Next * 2f) + xVel;
            YVel = -1.5f - (float)(RND.Next * 1f);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            int tx = MathUtil.Div(Position.X, Globals.TILE);
            int ty = MathUtil.Div(Position.Y + YVel, Globals.TILE);

            // destroy:

            var inWater = (GameManager.Current.Map.LayerData[2].Get(tx, ty) != null);
            if (ObjectManager.CollisionPoint<Solid>(Position.X, Position.Y).Count > 0)
                LifeTime = 0;

            if (inWater)
            {
                YVel *= .7f;
                XVel *= .9f;

                //YVel -= .3f;
                Alpha = Math.Max(Alpha - .04f, 0);
            }
            else
            {
                YVel = Math.Min(YVel + .15f, 3f);
            }

            if (Alpha == 0)
                LifeTime = 0;
        }
    }

    public class WaterSplashEmitter : ParticleEmitter
    {
        List<Color> particleColors;

        private float xVel;

        public WaterSplashEmitter(float x, float y, float xVel = 0) : base(x, y)
        {
            particleColors = new List<Color>
            {
                new Color(255, 255, 255),
                new Color(206, 255, 255),
                new Color(168, 248, 248),
                new Color(104, 216, 248)
            };

            SpawnRate = 15;

            this.xVel = xVel;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            SpawnRate = 0;

            if (particles.Count == 0)
                Destroy();
        }

        public override void CreateParticle()
        {
            var particle = new WaterSplashParticle(this, xVel);

            var colorIndex = RND.Int(particleColors.Count - 1);
            particle.Color = particleColors[colorIndex];
        }        
    }
}
