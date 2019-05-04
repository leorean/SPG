using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Draw;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SPG.Objects
{
    public class Particle
    {
        public ParticleEmitter Emitter { get; private set; }

        public float Angle;
        public Vector2 Scale;

        public float XVel;
        public float YVel;

        public Vector2 Position;
        public Color Color;
        public float Alpha;

        public int LifeTime;
        
        public Particle(ParticleEmitter emitter)
        {        
            Color = Color.White;
            
            Scale = Vector2.One;
            Alpha = 1;

            Emitter = emitter;
            Emitter.Add(this);

            Position = Emitter.Position;
        }

        ~Particle()
        {
            Emitter.Remove(this);
            Emitter = null;
        }

        public virtual void Update(GameTime gameTime)
        {
            LifeTime = Math.Max(LifeTime - 1, 0);

            if (LifeTime == 0)
                return;
            
            Position = new Vector2(Position.X + XVel, Position.Y + YVel);
        }

        public virtual void Draw(SpriteBatch sb, GameTime gameTime)
        {
            sb.Draw(Emitter.Texture, Position, null, new Color(Color, Alpha), Angle, Vector2.Zero, Scale, SpriteEffects.None, Emitter.Depth);
        }
    }

    // ++++ Emitter ++++
    
    public abstract class ParticleEmitter : GameObject
    {
        protected List<Particle> particles;
        private Texture2D pixel;
        
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the spawn rate. >1 means multiple particles per run
        /// </summary>
        public float SpawnRate { get; set; }
        private float spawn;

        public ParticleEmitter(float x, float y) : base(x, y)
        {
            Texture = Primitives2D.Pixel;

            particles = new List<Particle>();

            SpawnRate = 1;

            Active = true;            
        }

        public void Add(Particle particle)
        {
            particles.Add(particle);
        }

        public void Remove(Particle particle)
        {
            if (particles.Contains(particle))
                particles.Remove(particle);
        }

        ~ParticleEmitter()
        {
            particles.Clear();
        }

        public abstract void CreateParticle();

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Active)
            {
                spawn += SpawnRate;
                int spawnAmount = (int)Math.Floor(spawn);
                for (var i = 0; i < spawnAmount; i++)
                {
                    CreateParticle();
                }
                spawn -= spawnAmount;
            }
            
            Particle[] copy = new Particle[particles.Count];
            particles.CopyTo(copy);

            foreach (var p in copy)
            {
                p.Update(gameTime);

                if (p.LifeTime == 0)
                    Remove(p);                
            }
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            // the emitter doesn't draw itself, instead the texture is used for the particles
            //base.Draw(gameTime);
            
            foreach (var part in particles)
                part.Draw(sb, gameTime);
        }
        
        // ++++ PARTICLE ++++


    }

    
}
