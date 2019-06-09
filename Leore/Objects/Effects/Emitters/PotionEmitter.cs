using Leore.Objects.Items;
using Leore.Resources;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Leore.Objects.Effects.Emitters
{
    public class PotionEmitter : SaveStatueEmitter
    {
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
    }
}
