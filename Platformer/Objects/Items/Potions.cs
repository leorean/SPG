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
        HP, MP, Regen
    }

    public class Potion : Item
    {
        public static List<Color> HpColors = new List<Color>
        {
            new Color(248, 40, 40),
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

        public static List<Color> RegenColors = new List<Color>
        {
            new Color(242, 255, 156),
            new Color(170, 233, 60),
            new Color(104, 197, 100)
        };

        public PotionType Type { get; private set; }

        private int amount;

        private PotionEmitter potionEmitter;

        public Potion(float x, float y, Room room, PotionType potionType) : base(x, y, room)
        {
            Type = potionType;

            DrawOffset = new Vector2(8);
            
            potionEmitter = new PotionEmitter(x, y - 6, potionType);
            potionEmitter.Parent = this;

            if (Type == PotionType.HP)
            {
                amount = 5;
                Texture = AssetManager.Potions[0];
            }
            if (Type == PotionType.MP)
            {
                amount = 20;
                Texture = AssetManager.Potions[1];
            }
            if (Type == PotionType.Regen)
            {
                throw new NotImplementedException("A MP regen potion doesn't make sense at this point!");
            }

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
                    player.HP = Math.Min(player.HP + amount, GameManager.Current.SaveGame.gameStats.MaxHP);

                    var fntHp = new FollowFont(player.X, player.Y - Globals.TILE, $"+{amount} HP");
                    fntHp.Target = player;

                    fntHp.Color = HpColors.First();

                    break;
                case PotionType.MP:
                    player.MP = Math.Min(player.MP + amount, GameManager.Current.SaveGame.gameStats.MaxMP);

                    var fntMp = new FollowFont(player.X, player.Y - Globals.TILE, $"+{amount} MP");
                    fntMp.Target = player;

                    fntMp.Color = MpColors.First();

                    //player.MP = GameManager.Current.SaveGame.gameStats.MaxMP;
                    break;
            }
            
            var emitter = new PotionBurstEmitter(Center.X, Center.Y, Type);
            var eff = new SingularEffect(Center.X, Center.Y, 1);
            
            Destroy();
        }        
    }
}
