using Microsoft.Xna.Framework;
using Platformer.Objects.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Effects.Emitters
{
    public class PotionEmitter : SaveStatueEmitter
    {
        public PotionEmitter(float x, float y, PotionType potionType) : base(x, y)
        {
            switch (potionType)
            {
                case PotionType.HP:
                    particleColors = Potion.HpColors;
                    break;
                case PotionType.MP:
                    particleColors = Potion.MpColors;
                    break;
                case PotionType.Regen:
                    particleColors = Potion.RegenColors;
                    break;

            }
        }
    }
}
