using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Leore.Main;
using Leore.Objects.Effects;
using Leore.Objects.Effects.Emitters;
using Leore.Resources;

namespace Leore.Objects.Items
{
    public enum PotionType
    {
        HP, MP, Regen
    }

    public class Potion : Item
    {
        public PotionType Type { get; private set; }

        private int amount;

        private PotionEmitter potionEmitter;

        public Potion(float x, float y, Room room, PotionType potionType) : base(x, y, room)
        {
            Type = potionType;
            Depth = Globals.LAYER_FG + .0001f;
            DrawOffset = new Vector2(8);
            
            potionEmitter = new PotionEmitter(x, y - 3, potionType);
            potionEmitter.Parent = this;
            potionEmitter.SpawnTimeout = 20;
            potionEmitter.Radius = 4;

            if (Type == PotionType.HP)
            {
                amount = 5;
                //Texture = AssetManager.Potions[0];
            }
            if (Type == PotionType.MP)
            {
                amount = 20;
                //Texture = AssetManager.Potions[1];
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

                    var fntHp = new FollowFont(player.X, player.Y - Globals.T, $"+{amount} HP");
                    fntHp.Target = player;

                    fntHp.Color = GameResources.HpColors.First();

                    break;
                case PotionType.MP:
                    //player.MP = Math.Min(player.MP + amount, GameManager.Current.SaveGame.gameStats.MaxMP);

                    //var fntMp = new FollowFont(player.X, player.Y - Globals.T, $"+{amount} MP");
                    //fntMp.Target = player;

                    //fntMp.Color = GameResources.MpColors.First();

                    SpellEXP.Spawn(Center.X, Center.Y, amount);
                    
                    var fntMp = new FollowFont(player.X, player.Y - Globals.T, $"+{amount} MP");
                    fntMp.Target = player;

                    fntMp.Color = GameResources.MpColors.First();

                    //player.MP = GameManager.Current.SaveGame.gameStats.MaxMP;
                    break;
            }
            
            var emitter = new PotionBurstEmitter(Center.X, Center.Y, Type);
            var eff = new SingularEffect(Center.X, Center.Y, 1);
            
            Destroy();
        }        
    }
}
