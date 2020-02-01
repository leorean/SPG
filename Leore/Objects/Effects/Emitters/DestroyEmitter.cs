using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Util;
using System;

using SPG.Map;
using Leore.Main;

namespace Leore.Objects.Effects.Emitters
{

    public class BushParticle : Particle
    {
        private double t;
        private float scale;
        private bool kill;

        public BushParticle(ParticleEmitter emitter) : base(emitter)
        {
            LifeTime = 120;

            Texture = RND.Choose(AssetManager.Particles[5], AssetManager.Particles[6], AssetManager.Particles[7]);
            
            scale = .5f;
            Scale = new Vector2(scale);

            DrawOffset = new Vector2(8);

            Angle = 0;

            XVel = -1 + (float)RND.Next * 2;
            YVel = -.1f - (float)RND.Next * 1f;

            t = RND.Next * 2 * Math.PI;            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            if (!kill) {

                if (YVel < -.1f)
                {
                    XVel *= .85f;
                    YVel *= .9f;
                }
                else
                {
                    t = (t + .2f) % (2 * Math.PI);
                    XVel += .015f * (float)Math.Sin(t);
                    XVel = Math.Sign(XVel) * Math.Min(Math.Abs(XVel), .5f);

                    YVel = Math.Min(YVel + .01f, .3f);
                    Scale = new Vector2(Math.Sign(XVel != 0 ? XVel : 1) * scale, scale);

                    if (GameManager.Current.Map.CollisionTile(Position.X, Position.Y + 1) || GameManager.Current.Map.CollisionTile(Position.X, Position.Y, GameMap.WATER_INDEX))
                    {
                        kill = true;
                    }
                }
            } else
            {
                XVel *= .9f;
                YVel = 0;
                Alpha = Math.Max(Alpha - .05f, 0);
                if (Alpha == 0)
                    LifeTime = 0;
            }

            Position = new Vector2(Position.X + XVel, Position.Y + YVel);
        }
    }

    public class DestroyParticle : Particle
    {
        private bool stuck;
        private bool die;
        
        public DestroyParticle(ParticleEmitter emitter, int type, Vector2 scale) : base(emitter)
        {
            LifeTime = 120;

            XVel = -1 + (float)RND.Next * 2;
            YVel = -1 - (float)RND.Next * .5f;

            Texture = RND.Choose(AssetManager.Particles[1 + type * 2], AssetManager.Particles[2 + type * 2]);
            Scale = scale;

            DrawOffset = new Vector2(8);

            stuck = true;
            die = false;

            Angle = (float)(RND.Next * 2 * Math.PI);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            YVel += .15f;

            if (stuck)
            {
                if (!GameManager.Current.Map.CollisionTile(Position.X, Position.Y))
                    stuck = false;
            }
            else
            {
                if (!die)
                {
                    if (GameManager.Current.Map.CollisionTile(Position.X, Position.Y, GameMap.WATER_INDEX))
                    {
                        XVel *= .8f;
                        YVel *= .8f;
                    }

                    var colX = GameManager.Current.Map.CollisionTile(Position.X + XVel, Position.Y);
                    if (colX)
                    {
                        XVel = 0;
                    }
                    var colY = GameManager.Current.Map.CollisionTile(Position.X, Position.Y + YVel);
                    if (colY)
                    {
                        YVel *= -.5f;

                        if (Math.Abs(YVel) < .5f)
                            die = true;
                    }
                }
            }
            if (die)
            {
                XVel = 0;
                YVel = 0;
                Alpha = Math.Max(Alpha - .1f, 0);
                if (Alpha == 0)
                    LifeTime = 0;
            }
        }
    }

    public class DestroyEmitter : ParticleEmitter
    {
        int type;
        public DestroyEmitter(float x, float y, int type = 0) : base(x, y)
        {
            this.type = type;            
            SpawnRate = 4;

            Scale = new Vector2(.5f);

            if (type == 0) { } // destroy block (default)
            if (type == 2) // bush
                SpawnRate = 8;
            if (type == 4) // ice
                SpawnRate = 8;
            if (type == 6) // fire
                SpawnRate = 12;
            if (type == 8) // hard destroy block
                SpawnRate = 6;
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
            if (type != 2)
                new DestroyParticle(this, type, Scale);
            else
                new BushParticle(this);
        }
    }
}
