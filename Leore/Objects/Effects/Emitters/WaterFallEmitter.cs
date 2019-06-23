using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Main;
using SPG.Map;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using SPG;

namespace Leore.Objects.Effects.Emitters
{
    public class WaterFallParticle : Particle
    {
        bool stuck;
        public bool Collision { get; set; }

        private float trail = 5f;

        private float particleAlpha;

        public WaterFallParticle(ParticleEmitter emitter, float particleAlpha = 1f) : base(emitter)
        {
            Scale = new Vector2(1 + (float)RND.Next * 1f);
            Position = new Vector2(emitter.X + (float)RND.Next * (16f - Scale.X), emitter.Y - trail * Scale.Y);

            YVel = .5f + (float)RND.Next * .5f;
            LifeTime = 200;
            
            Depth = Globals.LAYER_BG + .0001f * YVel;

            this.particleAlpha = particleAlpha;

            Alpha = 1;            
            
            stuck = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (stuck)
            {
                if (!GameManager.Current.Map.CollisionTile(Position.X, Position.Y - trail * Scale.X, GameMap.FG_INDEX))
                    stuck = false;
            }

            if (!Collision)
                Collision = GameManager.Current.Map.CollisionTile(Position.X, Position.Y - trail * 1, GameMap.WATER_INDEX)
                || (!stuck && GameManager.Current.Map.CollisionTile(Position.X, Position.Y - trail * 1, GameMap.FG_INDEX));
            if (Collision)
            {
                LifeTime = 0;
                var random = RND.Int(4) > 2;

                if (random)
                {
                    var splash = new WaterSplashParticle(Emitter);
                    splash.Color = new Color(Color, particleAlpha);
                    splash.Position = Position - new Vector2(0, trail * Scale.Y);

                    splash.XVel = -.2f + (float)(RND.Next * .4f);
                    splash.YVel = -.5f - (float)(RND.Next * 1f);
                }
            } else
            {
                if (LifeTime < 60)
                    Alpha = Math.Max(Alpha - .02f, 0);
            }
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //base.Draw(sb, gameTime);

            var alpha = Alpha * .1f;
            for (float i = 0; i < trail * Scale.Y; i+= Scale.Y)
            {
                alpha += Alpha * (1f / trail);
                if(!GameManager.Current.Map.CollisionTile(Position.X, Position.Y + i - 2, GameMap.WATER_INDEX))
                    sb.Draw(Texture, Position + new Vector2(0, i), null, new Color(Color, alpha * particleAlpha), Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
            }
        }
    }

    public class WaterFallEmitter : ParticleEmitter
    {
        private List<Color> particleColors = new List<Color>();
        private float particleAlpha = 1f;

        public WaterFallEmitter(float x, float y, int type) : base(x, y)
        {
            SpawnRate = 1;
            SpawnTimeout = 1;

            switch (type)
            {
                case 0:
                    particleColors = new List<Color>
                    {
                        new Color(255, 255, 255),
                        new Color(206, 255, 255),
                        new Color(168, 248, 248),
                        new Color(104, 216, 248)
                    };
                    particleAlpha = .85f;
                    break;
                case 1:
                    particleColors = new List<Color>
                    {
                        new Color(71, 167, 202),
                        new Color(135, 223, 255),
                        new Color(36, 126, 158),
                        new Color(14, 238, 255)
                    };
                    particleAlpha = .5f;
                    break;
                case 2:
                    particleColors = new List<Color>
                    {
                        Colors.FromHex("e2d8fc"),
                        Colors.FromHex("b1b0e1"),
                        new Color(Colors.FromHex("646cd3"), .5f),
                        
                    };
                    particleAlpha = .85f;
                    break;
            }
        }

        public override void CreateParticle()
        {
            var waterFallParticle = new WaterFallParticle(this, particleAlpha);

            var colorIndex = RND.Int(particleColors.Count - 1);
            waterFallParticle.Color = particleColors[colorIndex];
            waterFallParticle.Alpha = particleColors[colorIndex].A / 255f;
        }
    }

    public class WaterFall : RoomObject
    {
        private WaterFallEmitter emitter;

        public WaterFall(float x, float y, Room room, int type) : base(x, y, room, null)
        {
            emitter = new WaterFallEmitter(X, Y, type);
            emitter.Parent = this;

            emitter.Active = true;
        }
    }
}
