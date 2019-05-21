using Microsoft.Xna.Framework;
using Platformer.Objects.Effects.Emitters;
using Platformer.Objects.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Items
{
    public class StatUpItem : AbilityItem
    {
        public enum StatType
        {
            HP,
            MP,
            //MPRegen
        }

        public StatType Type {get; private set; }

        private PotionEmitter potionEmitter;

        public StatUpItem(float x, float y, Room room, StatType type) : base(x, y, room)
        {
            Type = type;

            maxYDist = Globals.TILE;
            flashOnTaken = false;

            switch (type)
            {
                case StatType.HP:
                    potionEmitter = new PotionEmitter(X, Y - 8, PotionType.HP);                    
                    Texture = AssetManager.Items[3];
                    Name = "HP-Up";
                    Text = "Increased max. HP by 5.";
                    OnObtain = () => 
                    {
                        player.Stats.MaxHP += 5;
                        player.HP = player.Stats.MaxHP;
                        new PotionBurstEmitter(X, Y, PotionType.HP);
                    };
                    break;
                case StatType.MP:
                    potionEmitter = new PotionEmitter(X, Y - 8, PotionType.MP);
                    Name = "MP-Up";
                    Texture = AssetManager.Items[4];
                    Text = "Increased max. MP by 10.";
                    OnObtain = () => 
                    {
                        player.Stats.MaxMP += 10;
                        player.MP = player.Stats.MaxMP;
                        new PotionBurstEmitter(X, Y, PotionType.MP);
                    };
                    break;
            }

            potionEmitter.Parent = this;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            ObtainShineEmitter.Active = false;
            obtainParticleEmitter.Active = false;

            potionEmitter.Position = Position + new Vector2(0, -8);
            potionEmitter.Active = !Taken;
        }
    }    
}
