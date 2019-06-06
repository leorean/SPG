using Platformer.Objects.Items;

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
