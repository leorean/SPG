using Leore.Main;
using Leore.Objects.Effects;
using Microsoft.Xna.Framework;
using SPG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Objects.Level
{
    public class CoinStatue : NPC
    {
        private int amount;

        private LightSource light;

        public CoinStatue(float x, float y, Room room, int amount) : base(x, y, room, 0, null, null, null, null, false, Direction.NONE, null, null, null)
        {
            text = $"Do you want to offer [fbdf74]~{amount}~ coins?";

            this.amount = amount;

            Depth = Globals.LAYER_BG - .0001f;
            AnimationTexture = AssetManager.CoinStatue;
            
            DrawOffset = new Vector2(16, 24);
            BoundingBox = new SPG.Util.RectF(-8, -8, 16, 16);

            light = new LightSource(this);
            forceDialog = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (GameManager.Current.Player.Stats.KeysAndKeyblocks.Contains(ID))
            {
                SetAnimation(1, 1, 0, false);
                GameManager.Current.OverwriteSwitchStateTo(true);
                Active = false;
            }

            light.Active = !Active;
        }

        public override void Interact(Player player)
        {            
            base.Interact(player);

        }

        public override void EndOfConversation()
        {
            if (decision)
            {
                if (player.Stats.Coins >= amount)
                {
                    player.Stats.Coins -= amount;
                    new FollowFont(player.Center.X, player.Center.Y - 12, $"-{amount}$") { /*Color = Colors.FromHex("fbdf74")*/ };
                    player.Stats.KeysAndKeyblocks.Add(ID);
                    Active = false;
                }
                else
                {
                    new MessageBox("You don't have enough..");
                }
            }
            base.EndOfConversation();
        }
    }
}
