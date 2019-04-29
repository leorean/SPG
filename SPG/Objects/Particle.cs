﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SPG.Objects
{
    public class ParticleEmitter : GameObject
    {
        protected List<Particle> particles;
        private Texture2D pixel;
        
        public delegate void ParticleDelegate(Particle particle);

        /// <summary>
        /// This function is called for each particle for each update step.
        /// </summary>
        public ParticleDelegate ParticleUpdate;

        /// <summary>
        ///  This function is called for each particle when it is created.
        /// </summary>
        public ParticleDelegate ParticleInit;

        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the spawn rate. >1 means multiple particles per run
        /// </summary>
        public float SpawnRate { get; set; }
        private float spawn;

        public ParticleEmitter(float x, float y) : base(x, y)
        {
            pixel = new Texture2D(GameManager.Game.GraphicsDevice, 1, 1);
            pixel.SetData(new Color[] { Color.White });

            Texture = pixel;

            particles = new List<Particle>();

            SpawnRate = 1;

            Active = true;
        }

        private void Add(Particle particle)
        {
            particles.Add(particle);
        }

        private void Remove(Particle particle)
        {
            if (particles.Contains(particle))
                particles.Remove(particle);
        }

        ~ParticleEmitter()
        {
            particles.Clear();
            ParticleUpdate = null;
            ParticleInit = null;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Active)
            {

                spawn += SpawnRate;

                int spawnAmount = (int)Math.Floor(spawn);

                for (var i = 0; i < spawnAmount; i++)
                {
                    var part = new Particle(this);
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

        public class Particle
        {
            private ParticleEmitter emitter;

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
                Position = emitter.Position;

                Scale = Vector2.One;
                Alpha = 1;

                this.emitter = emitter;
                emitter.Add(this);

                emitter.ParticleInit(this);
            }

            ~Particle()
            {
                emitter.Remove(this);
                emitter = null;
            }

            public void Update(GameTime gameTime)
            {
                LifeTime = Math.Max(LifeTime - 1, 0);

                emitter.ParticleUpdate(this);

                Position = new Vector2(Position.X + XVel, Position.Y + YVel);
            }

            public void Draw(SpriteBatch sb, GameTime gameTime)
            {
                sb.Draw(emitter.Texture, Position, null, new Color(Color, Alpha), Angle, Vector2.Zero, Scale, SpriteEffects.None, emitter.Depth);
            }
        }
    }

    
}
