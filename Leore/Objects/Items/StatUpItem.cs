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

        private PotionEmitter potionEmitter;

        public StatUpItem(float x, float y, Room room, StatType type) : base(x, y, room)
        {
            Type = type;

            //Scale = new Vector2(.5f);

            maxYDist = Globals.TILE;
            flashOnTaken = false;

            switch (type)
            {
                case StatType.HP:
                    potionEmitter = new PotionEmitter(X, Y - 8, PotionType.HP);
                    Texture = AssetManager.Items[1];

                    HighlightColor = GameResources.HpColors.First();

                    Name = "HP-Up";
                    Text = "~Max. HP~ increased by 3.";
                    OnObtain = () => 
                    {
                        player.Stats.MaxHP += 3;
                        player.HP = player.Stats.MaxHP;
                        new PotionBurstEmitter(X, Y, PotionType.HP);
                    };
                    break;
                case StatType.MP:
                    potionEmitter = new PotionEmitter(X, Y - 8, PotionType.MP);
                    Texture = AssetManager.Items[2];

                    HighlightColor = GameResources.MpColors.First();

                    Name = "MP-Up";
                    Text = "~Max. MP~ increased by 5.";
                    OnObtain = () => 
                    {
                        player.Stats.MaxMP += 5;
                        player.MP = player.Stats.MaxMP;
                        new PotionBurstEmitter(X, Y, PotionType.MP);
                    };
                    break;
                case StatType.Regen:
                    potionEmitter = new PotionEmitter(X, Y - 8, PotionType.Regen);
                    Texture = AssetManager.Items[3];

                    HighlightColor = GameResources.RegenColors.First();

                    Name = "MP-Regen-Up";
                    Text = "~MP regeneration~ rate increased.";
                    OnObtain = () =>
                    {
                        player.Stats.MPRegen += .1f;
                        player.MP = player.Stats.MaxMP;
                        new PotionBurstEmitter(X, Y, PotionType.Regen);
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
                potionEmitter.Position = Position + new Vector2(0, -8);
                potionEmitter.Active = !Taken;
            }
        }        
    }    
}
