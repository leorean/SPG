using Microsoft.Xna.Framework;
using Leore.Main;
using Leore.Objects.Items;
using SPG.Objects;
using SPG.Util;
using System;

namespace Leore.Objects.Effects.Emitters
{
    public class PotionBurstParticle : Particle
    {
        public PotionBurstParticle(ParticleEmitter emitter) : base(emitter)
        {
            LifeTime = 60;

            Scale = new Vector2(3, 3);

            Angle = (float)(RND.Next * 360);

            float spd = (float)(2 + RND.Next * 2f);

            XVel = (float)MathUtil.LengthDirX(Angle) * spd;
            YVel = (float)MathUtil.LengthDirY(Angle) * spd;

            int colorIndex = 0;
            switch((Emitter as PotionBurstEmitter).PotionType)
            {
                case PotionType.HP:
                    colorIndex = RND.Int(Potion.HpColors.Count - 1);
                    Color = Potion.HpColors[colorIndex];
                    break;
                case PotionType.MP:
                    colorIndex = RND.Int(Potion.MpColors.Count - 1);
                    Color = Potion.MpColors[colorIndex];
                    break;
                case PotionType.Regen:
                    colorIndex = RND.Int(Potion.RegenColors.Count - 1);
                    Color = Potion.RegenColors[colorIndex];
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);


            var player = GameManager.Current.Player;
            if (LifeTime > 30)
            {
                XVel *= .9f;
                YVel *= .9f;                
            }
            else {
                XVel = (player.X - Position.X) / 8f + player.XVel;
                YVel = (player.Y - Position.Y) / 8f + player.YVel;
                Alpha = Math.Max(Alpha - .1f, 0);
            }
            var s = Math.Max(4 * LifeTime / 60f, 2f);            
            
            Scale = new Vector2(s);
        }
    }

    public class PotionBurstEmitter : ParticleEmitter
    {
        public PotionType PotionType { get; private set; }

        public PotionBurstEmitter(float x, float y, PotionType potionType) : base(x, y)
        {
            PotionType = potionType;

            SpawnRate = 25;
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
            var particle = new PotionBurstParticle(this);            
        }
    }
}
