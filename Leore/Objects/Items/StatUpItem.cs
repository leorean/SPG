using Microsoft.Xna.Framework;
using Leore.Main;
using Leore.Objects.Effects.Emitters;
using System.Linq;
using Leore.Resources;

namespace Leore.Objects.Items
{
    public class StatUpItem : AbilityItem
    {
        public enum StatType
        {
            HP,
            MP,
            Regen
        }

        public StatType Type {get; private set; }

        protected PotionEmitter potionEmitter;

        private bool fromShop;

        public StatUpItem(float x, float y, Room room, StatType type, bool fromShop = false) : base(x, y, room)
        {
            Type = type;

            this.fromShop = fromShop;

            maxYDist = Globals.T;
            flashOnTaken = false;

            string statColorHex = "f8e038";

            switch (type)
            {
                case StatType.HP:
                    potionEmitter = new PotionEmitter(X, Y - 8, PotionType.HP);
                    Texture = AssetManager.Items[1];

                    HighlightColor = GameResources.HpColors.First();

                    Name = "HP-Up";
                    Text = $"~Max. HP~ increased by [{statColorHex}]~1~.";
                    OnObtain = () => 
                    {
                        player.Stats.MaxHP += 1;
                        player.HP = player.Stats.MaxHP;
                        new PotionBurstEmitter(X, Y, PotionType.HP);
                        if (fromShop)
                            GameManager.Current.OverwriteSwitchStateTo(false);
                    };
                    break;
                case StatType.MP:
                    potionEmitter = new PotionEmitter(X, Y - 8, PotionType.MP);
                    Texture = AssetManager.Items[2];

                    HighlightColor = GameResources.MpColors.First();

                    Name = "MP-Up";
                    Text = $"~Max. MP~ increased by [{statColorHex}]~2~.";
                    OnObtain = () => 
                    {
                        player.Stats.MaxMP += 2;
                        player.MP = player.Stats.MaxMP;
                        new PotionBurstEmitter(X, Y, PotionType.MP);
                        if (fromShop)
                            GameManager.Current.OverwriteSwitchStateTo(false);
                    };
                    break;
                case StatType.Regen:
                    potionEmitter = new PotionEmitter(X, Y - 8, PotionType.Regen);
                    Texture = AssetManager.Items[3];

                    HighlightColor = GameResources.RegenColors.First();

                    Name = "MP-Regen-Up";
                    Text = $"~MP regeneration~ rate increased by [{statColorHex}]~5%~.";
                    OnObtain = () =>
                    {
                        player.Stats.MPRegen += .05f;
                        player.MP = player.Stats.MaxMP;
                        new PotionBurstEmitter(X, Y, PotionType.Regen);
                        if (fromShop)
                            GameManager.Current.OverwriteSwitchStateTo(false);
                    };
                    break;
            }
            if (potionEmitter != null) potionEmitter.Parent = this;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            //ObtainShineEmitter.Active = false;
            ObtainShineEmitter.Active = true;
            ObtainShineEmitter.GlowScale = .5f;            

            obtainParticleEmitter.Active = false;

            if (potionEmitter != null)
            {
                potionEmitter.Position = Position + new Vector2(0, -12);
                potionEmitter.Active = !Taken;
            }
        }        
    }    
}
