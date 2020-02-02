using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;

namespace Leore.Objects.Effects.Emitters
{
    public class KeyBurstParticle : Particle
    {
        private GameObject target;

        private int maxLifeTime;
        
        public KeyBurstParticle(ParticleEmitter emitter, GameObject target) : base(emitter)
        {

            this.target = target;

            LifeTime = 40;

            Scale = new Vector2(3, 3);

            Angle = (float)(RND.Next * 360);

            float spd = (float)(2 + RND.Next * 2f);

            XVel = (float)MathUtil.LengthDirX(Angle) * spd;
            YVel = (float)MathUtil.LengthDirY(Angle) * spd;

            int colorIndex = 0;
            colorIndex = RND.Int((Emitter as KeyBurstEmitter).Colors.Count - 1);
            Color = (Emitter as KeyBurstEmitter).Colors[colorIndex];
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!ObjectManager.Exists(target))
            {
                LifeTime = 0;
                return;
            }

            maxLifeTime = Math.Max(LifeTime, maxLifeTime);

            if (LifeTime > .25f * maxLifeTime)
            {
                XVel *= .9f;
                YVel *= .9f;                
            }
            else {
                XVel = (target.Center.X - Position.X) / 4f;
                YVel = (target.Center.Y - Position.Y) / 4f;
                Alpha = Math.Max(Alpha - .1f, 0);
                
                if (MathUtil.Euclidean(Position, target.Position) < 4) {
                    LifeTime = 0;
                }
            }
            var s = Math.Max(4 * LifeTime / (float)maxLifeTime, 2f);
            
            Scale = new Vector2(s);
        }
    }

    public class KeyBurstEmitter : ParticleEmitter
    {
        public List<Color> Colors { get; set; } = new List<Color>();

        private GameObject target;

        public Action OnFinishedAction;
        private PotionBurstEmitter potionBurstEmitter;

        public KeyBurstEmitter(float x, float y, GameObject target) : base(x, y)
        {
            Colors.Add(new Color(255, 243, 130));
            Colors.Add(new Color(251, 223, 116));
            Colors.Add(new Color(237, 160, 72));

            this.target = target;
            SpawnRate = 25;
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            SpawnRate = 0;

            if (Particles.Count == 0)
            {
                OnFinishedAction?.Invoke();
                Destroy();
            }
        }

        public override void CreateParticle()
        {
            var particle = new KeyBurstParticle(this, target);
        }
    }
}
