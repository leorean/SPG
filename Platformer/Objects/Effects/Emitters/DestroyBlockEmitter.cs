using Microsoft.Xna.Framework;
using Platformer.Objects.Main;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SPG.Map;

namespace Platformer.Objects.Effects.Emitters
{
    public class DestroyBlockParticle : Particle
    {
        private bool stuck;
        private bool die;

        public DestroyBlockParticle(ParticleEmitter emitter) : base(emitter)
        {
            LifeTime = 120;

            XVel = -1 + (float)RND.Next * 2;
            YVel = -1 - (float)RND.Next * .5f;

            Texture = RND.Choose(AssetManager.Particles[1], AssetManager.Particles[2]);
            Scale = new Vector2(.5f);

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

    public class DestroyBlockEmitter : ParticleEmitter
    {
        public DestroyBlockEmitter(float x, float y) : base(x, y)
        {
            SpawnRate = 4;
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
            new DestroyBlockParticle(this);
        }
    }
}
