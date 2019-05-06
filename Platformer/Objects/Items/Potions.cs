using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Platformer.Objects.Effects;
using Platformer.Objects.Effects.Emitters;
using Platformer.Objects.Main;

namespace Platformer.Objects.Items
{
    public enum PotionType
    {
        HP, MP//, Regen
    }

    public class Potion : Item
    {
        public static List<Color> HpColors = new List<Color>
        {
            new Color(218, 36, 0),
            new Color(231, 99, 73),
            new Color(255, 90, 0)
        };

        public static List<Color> MpColors = new List<Color>
        {
            new Color(3, 243, 243),
            new Color(143, 255, 249),
            new Color(95, 205, 208)
        };

        public PotionType Type { get; private set; }

        private PotionEmitter potionEmitter;

        public Potion(float x, float y, Room room, PotionType potionType, string name = null) : base(x, y, room, name)
        {
            Type = potionType;
            
            potionEmitter = new PotionEmitter(x, y - 6, potionType);
            potionEmitter.Parent = this;

            if (Type == PotionType.HP) Texture = AssetManager.PotionSprites[0];
            if (Type == PotionType.MP) Texture = AssetManager.PotionSprites[1];
            
            Save = false;
            Respawn = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Taken)
            {
                Destroy();
            }            
        }

        public override void Take(Player player)
        {
            switch (Type)
            {
                case PotionType.HP:
                    player.HP = Math.Min(player.HP + 1, GameManager.Current.SaveGame.gameStats.MaxHP);
                    break;
                case PotionType.MP:
                    player.MP = GameManager.Current.SaveGame.gameStats.MaxMP;
                    break;
            }
            
            var emitter = new PotionBurstEmitter(Center.X, Center.Y, Type);
            var eff = new SingularEffect(Center.X, Center.Y, 1);

            Destroy();
        }
    }
}
