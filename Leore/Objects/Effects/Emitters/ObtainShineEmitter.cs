﻿using Microsoft.Xna.Framework;
using Leore.Main;
using SPG.Objects;
using System;
using SPG.Util;

namespace Leore.Objects.Effects.Emitters
{
    public class ObtainShineParticle : Particle
    {
        private float s = .01f;
        public float lightScale;
        private Vector2 off;
        
        public ObtainShineParticle(ParticleEmitter emitter) : base(emitter)
        {
            DrawOffset = new Vector2(32, 32);
            Texture = AssetManager.WhiteCircle[0];

            LifeTime = 120;

            off = - new Vector2(1.5f) + new Vector2((float)RND.Next * 3, (float)RND.Next * 3);

            Scale = new Vector2(s);            
            Depth = emitter.Depth;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            s = (float)Math.Min(s + .02 * lightScale, lightScale * 5f);

            Scale = new Vector2(s);

            Alpha = Math.Max(Alpha - .001f, 0);
            Position = Emitter.Position + off;            
        }
    }

    public class ObtainShineEmitter : ParticleEmitter
    {
        public float GlowScale;
        public float GlowAlpha;

        public ObtainShineEmitter(float x, float y) : base(x, y)
        {
            SpawnTimeout = 15;
            GlowScale = .3f;
            GlowAlpha = .1f;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void CreateParticle()
        {
            var particle = new ObtainShineParticle(this);
            particle.lightScale = GlowScale;
            particle.Alpha = GlowAlpha;
            particle.Color = Color;
            particle.Depth = Depth;
        }
    }
}
