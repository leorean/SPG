using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Map;
using SPG.Util;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Leore.Main;

namespace Leore.Objects.Effects.Emitters
{
    public class SlimeParticle : Particle
    {
        private bool visible;
        private bool onGround;

        public SlimeParticle(ParticleEmitter emitter, float radius) : base(emitter)
        {
            Scale = new Vector2(.5f);
            LifeTime = 120;
            
            Position = new Vector2(emitter.X - .5f * radius + (float)(RND.Next * radius), emitter.Y - .5f * radius + (float)(RND.Next * radius));

            Texture = AssetManager.Particles[8];
            DrawOffset = new Vector2(8);

            Alpha = 1;
            Angle = (float)((RND.Next * 360) / (2 * Math.PI));

            XVel = -.25f + (float)(RND.Next * .5f);
            YVel = -1f - (float)(RND.Next * .5f);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            DrawOffset = new Vector2(.5f * Texture.Width, .5f * Texture.Height);

            // destroy:

            var inWater = GameManager.Current.Map.CollisionTile(Position.X, Position.Y, GameMap.WATER_INDEX);
            if (GameManager.Current.Map.CollisionTile(Position.X, Position.Y + YVel))
                onGround = true;
                //LifeTime = 0;

            if (inWater)
            {
                YVel *= .7f;
                XVel *= .9f;
                
                Alpha = Math.Max(Alpha - .04f, 0);
            }
            else
            {
                YVel = Math.Min(YVel + .08f, 1.5f);
            }

            if (onGround)
            {
                XVel = 0;
                YVel = 0;
                Scale = new Vector2(Math.Max(Scale.X - .01f, 0));
                Alpha = Math.Max(Alpha - .01f, 0);
            }

            if (Alpha == 0)
                LifeTime = 0;
            
            visible = true;
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            if (visible)
                base.Draw(sb, gameTime);
        }
    }

    public class SlimeEmitter : ParticleEmitter
    {
        public float Alpha { get; set; }

        private float radius;

        public SlimeEmitter(float x, float y, int type, float radius) : base(x, y)
        {
            this.radius = radius;
            SpawnRate = 15;            
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
            var particle = new SlimeParticle(this, radius);
            particle.Color = Color;
            particle.Alpha = Alpha;
        }        
    }
}
