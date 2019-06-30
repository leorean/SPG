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

        public SlimeParticle(ParticleEmitter emitter) : base(emitter)
        {
            Scale = new Vector2(.5f);
            LifeTime = 120;

            Position = new Vector2(emitter.X - 6 + (float)(RND.Next * 12), emitter.Y);

            Texture = AssetManager.Particles[8];
            DrawOffset = new Vector2(8);

            Alpha = 1;
            Angle = (float)((RND.Next * 360) / (2 * Math.PI));

            XVel = -.5f + (float)(RND.Next * 1f);
            YVel = -.25f - (float)(RND.Next * .5f);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            DrawOffset = new Vector2(.5f * Texture.Width, .5f * Texture.Height);

            // destroy:

            var inWater = GameManager.Current.Map.CollisionTile(Position.X, Position.Y, GameMap.WATER_INDEX);
            if (GameManager.Current.Map.CollisionTile(Position.X, Position.Y))
                LifeTime = 0;

            if (inWater)
            {
                YVel *= .7f;
                XVel *= .9f;
                
                Alpha = Math.Max(Alpha - .04f, 0);
            }
            else
            {
                YVel = Math.Min(YVel + .05f, 1.5f);
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
        public List<Color> ParticleColors { get; set; } = new List<Color>
        {
            new Color(255, 255, 255),
            new Color(206, 255, 255),
            new Color(168, 248, 248),
            new Color(104, 216, 248)
        };

        public SlimeEmitter(float x, float y, int type) : base(x, y)
        {
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
            var colorIndex = RND.Int(ParticleColors.Count - 1);
            
            var particle = new SlimeParticle(this);
            particle.Color = ParticleColors[colorIndex];            
        }        
    }
}
