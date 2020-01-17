using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Draw;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SPG.Util;
using SPG.View;
using System.Linq;

namespace SPG.Objects
{
    public class Particle
    {
        public ParticleEmitter Emitter { get; private set; }

        public Texture2D Texture { get; set; }
        public float Depth;

        public float Angle;
        public Vector2 Scale;

        public float XVel;
        public float YVel;

        public Vector2 Position;
        public Color Color;
        public float Alpha;
        public Vector2 DrawOffset;

        public float X => Position.X;
        public float Y => Position.Y;

        public int LifeTime;
        
        public Particle(ParticleEmitter emitter)
        {        
            Color = Color.White;
            
            Scale = Vector2.One;
            Alpha = 1;

            Emitter = emitter;
            Emitter.Add(this);

            Position = Emitter.Position;

            Texture = Primitives2D.Pixel;
            Depth = emitter.Depth;
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
            sb.Draw(Texture, Position, null, new Color(Color, Alpha), Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
        }
    }

    // ++++ Emitter ++++
    
    public abstract class ParticleEmitter : GameObject
    {
        public List<Particle> Particles { get; protected set; }
        
        public bool Active { get; set; }


        /// <summary>
        /// Gets or sets the amount of particles spawned per spawn
        /// </summary>
        public int SpawnRate { get; set; } = 1;

        /// <summary>
        /// Gets or sets a number of frames to wait for particles to spawn
        /// </summary>
        public int SpawnTimeout { get; set; } = 0;

        protected int currentSpawnTimeout = 0;

        public ParticleEmitter(float x, float y) : base(x, y)
        {
            Depth = Globals.LAYER_PARTICLE;

            Particles = new List<Particle>();

            SpawnRate = 1;

            Active = true;
        }

        public void Add(Particle particle)
        {
            Particles.Add(particle);
        }

        public void Remove(Particle particle)
        {
            if (Particles.Contains(particle))
                Particles.Remove(particle);
        }

        ~ParticleEmitter()
        {
            Particles.Clear();
        }

        public abstract void CreateParticle();

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            if (Active)
            {
                currentSpawnTimeout -= 1;
                if (currentSpawnTimeout <= 0)
                {
                    for(var i = 0; i < SpawnRate; i++)
                        CreateParticle();

                    currentSpawnTimeout = SpawnTimeout;
                }
            }
            
            foreach (var p in Particles.ToList())
            {
                p.Update(gameTime);

                if (p.LifeTime == 0)
                    Remove(p);                
            }
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            // the emitter doesn't draw itself
            //base.Draw(gameTime);

            // draw only particles which are within the camera bounds

            var t = Globals.T;

            foreach (var part in Particles)
            {
                if (part.Position.X.In(Camera.Current.ViewX - t, Camera.Current.ViewX + Camera.Current.ViewWidth + t)
                    && part.Position.Y.In(Camera.Current.ViewY - t, Camera.Current.ViewY + Camera.Current.ViewHeight + t))
                {
                    part.Draw(sb, gameTime);
                }
            }
        }        
    }    
}
