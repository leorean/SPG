using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Main;
using SPG.Map;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using SPG;
using Leore.Resources;

namespace Leore.Objects.Effects.Emitters
{
    public class WaterFallParticle : Particle
    {
        bool stuck;
        public bool Collision { get; set; }

        private float trail;

        private float particleAlpha;

        public WaterFallParticle(ParticleEmitter emitter, float particleAlpha = 1f, float trail = 5f, float yVel = .5f) : base(emitter)
        {
            Scale = new Vector2(1 + (float)RND.Next * 1f);
            Position = new Vector2(emitter.X + (float)RND.Next * (16f - Scale.X), emitter.Y - trail * Scale.Y);

            YVel = yVel + (float)RND.Next * .5f;
            LifeTime = 200;
            
            Depth = Globals.LAYER_BG + .0001f * YVel;

            this.particleAlpha = particleAlpha;
            this.trail = trail;

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
        private float trail = 1f;
        private float yVel = .7f;

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
                    particleAlpha = .5f;
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
                    particleColors = GameResources.FireColors;
                    particleAlpha = .5f;
                    yVel = .25f;
                    break;
            }
        }

        public override void CreateParticle()
        {
            var waterFallParticle = new WaterFallParticle(this, particleAlpha, trail, yVel);

            var colorIndex = RND.Int(particleColors.Count - 1);
            waterFallParticle.Color = particleColors[colorIndex];
            waterFallParticle.Alpha = particleColors[colorIndex].A / 255f;
        }
    }

    public class WaterFall : RoomObject
    {
        private WaterFallEmitter emitter;
        
        private float yoff;
        private int height;
        private float flowSpd;
        private float animSpd;

        public WaterFall(float x, float y, Room room, int type) : base(x, y, room, null)
        {
            emitter = new WaterFallEmitter(X, Y, type);
            emitter.Parent = this;
            emitter.Active = true;

            Depth = Globals.LAYER_BG + .00001f;

            switch (type)
            {
                case 0: Color = new Color(Color, 1f); flowSpd = .6f; animSpd = .15f; break;
                case 1: Color = new Color(Color, 1f); flowSpd = .6f; animSpd = .15f; break;
                case 2: Color = new Color(Color, 1f); flowSpd = .45f; animSpd = .1f; break;
            }

            SetAnimation(type * 6, (type * 6) + 5, animSpd, true);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            yoff = (yoff + flowSpd) % Globals.T;

            for (int i = 0; i < 9; i++)
            {
                var col = GameManager.Current.Map.CollisionTile(Position.X, Position.Y + i * Globals.T, GameMap.WATER_INDEX)
                || (GameManager.Current.Map.CollisionTile(Position.X, Position.Y + i * Globals.T, GameMap.FG_INDEX));

                if (col)
                {
                    height = i;
                    break;
                }
            }
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //base.Draw(sb, gameTime);

            var z = (int)yoff;

            for (var i = 0; i <= height; i++)
            {
                var h = (i == height) ? 3 : 16;

                sb.Draw(AssetManager.WaterFall[AnimationFrame], Position + new Vector2(0, i * Globals.T), new Rectangle(0, 16 - z, 16, h), Color, 0, DrawOffset, Scale, SpriteEffects.None, Depth);
            }
        }
    }
}
