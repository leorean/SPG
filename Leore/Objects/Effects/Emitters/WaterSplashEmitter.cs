﻿using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Map;
using SPG.Util;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Leore.Main;

namespace Leore.Objects.Effects.Emitters
{
    public class WaterSplashParticle : Particle
    {
        private bool visible;

        public WaterSplashParticle(ParticleEmitter emitter, float xVel = 0, float scale = 2f) : base(emitter)
        {
            Scale = new Vector2(scale);
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

            DrawOffset = new Vector2(.5f * Texture.Width, .5f * Texture.Height);

            // destroy:

            var inWater = GameManager.Current.Map.CollisionTile(Position.X, Position.Y, GameMap.WATER_INDEX);
            if (GameManager.Current.Map.CollisionTile(Position.X, Position.Y))
            {                
                LifeTime = 0;
            }

            if (inWater)
            {
                YVel *= .7f;
                XVel *= .9f;

                //YVel -= .3f;
                Alpha = Math.Max(Alpha - .04f, 0);
            }
            else
            {
                YVel = Math.Min(YVel + .15f, 1.5f);
            }

            if (Alpha == 0)
                LifeTime = 0;

            visible = true;

            //if (LifeTime == 0)
            //{
            //    SoundManager.Play(Sounds.WaterSplashBubble, pitch: -.2f + RND.Next * .4f);
            //}
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            if (visible)
                base.Draw(sb, gameTime);
        }
    }

    public class WaterSplashEmitter : ParticleEmitter
    {
        private float xVel;

        public List<Color> ParticleColors { get; set; } = new List<Color>
        {
            new Color(255, 255, 255),
            new Color(206, 255, 255),
            new Color(168, 248, 248),
            new Color(104, 216, 248)
        };

        public WaterSplashEmitter(float x, float y, float xVel = 0) : base(x, y)
        {
            SpawnRate = 15;

            this.xVel = xVel;            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            SpawnRate = 0;

            if (Particles.Count == 0)
                Destroy();
        }

        public override void CreateParticle()
        {
            var colorIndex = RND.Int(ParticleColors.Count - 1);
            
            var particle = new WaterSplashParticle(this, xVel, Scale.X);
            particle.Color = ParticleColors[colorIndex];
            particle.Texture = Texture == null ? particle.Texture : Texture;
        }        
    }
}
