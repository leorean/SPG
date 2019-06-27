using Leore.Objects.Items;
using Leore.Resources;
using Microsoft.Xna.Framework;
using SPG.Util;
using System.Collections.Generic;

namespace Leore.Objects.Effects.Emitters
{
    public class PotionEmitter : SaveStatueEmitter
    {
        public float Radius { get; set; } = 8;

        public PotionEmitter(float x, float y, PotionType potionType) : base(x, y)
        {
            switch (potionType)
            {                
                case PotionType.HP:
                    particleColors = GameResources.HpColors;
                    break;
                case PotionType.MP:
                    particleColors = GameResources.MpColors;
                    break;
                case PotionType.Regen:
                    particleColors = GameResources.RegenColors;
                    break;

            }
        }

        public PotionEmitter(float x, float y, List<Color> colors) : base(x, y)
        {
            particleColors = colors;
        }

        public override void CreateParticle()
        {
            var particle = new SaveStatueParticle(this);

            var colorIndex = RND.Int(particleColors.Count - 1);
            particle.Color = particleColors[colorIndex];

            var posX = X - (.5f * Radius) + (float)RND.Next * Radius;
            var posY = Y + 3;

            particle.Position = new Vector2(posX, posY);
        }
    }
}
