using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Objects.Main;
using System;
using SPG.Objects;
using SPG.Draw;
using SPG;
using Platformer.Objects.Effects;
using Platformer.Main;

namespace Platformer.Objects.Items
{
    public class ShopItem : RoomObject
    {
        private int type;
        private int price;
        private string text;
        private bool respawn;

        private double t = 0;
        private double z = 0;
        private bool showInfo;

        private Font font;

        public bool Sold { get; private set; }

        public ShopItem(int x, int y, Room room, string itemName, int shopItemType, int shopItemPrice, string shopItemText, bool shopItemRespawn) : base(x, y, room, itemName)
        {
            DebugEnabled = true;
            BoundingBox = new SPG.Util.RectF(-4, -4, 8, 8);

            DrawOffset = new Vector2(8);
            Depth = Globals.LAYER_FG;

            font = AssetManager.DamageFont.Copy();
            font.HighlightColor = Colors.FromHex("FFF382");
            font.Halign = Font.HorizontalAlignment.Center;
            font.Valign = Font.VerticalAlignment.Center;

            this.type = shopItemType;
            this.price = shopItemPrice;
            this.text = shopItemText;
            this.respawn = shopItemRespawn;            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (GameManager.Current.Player.Stats.Items.ContainsKey(ID))
                Sold = true;

            if (!Sold)
                showInfo = this.CollisionBounds(GameManager.Current.Player, X, Y);
            
            t = (t + .03f) % (2 * Math.PI);
            z = 2 * Math.Sin(t);
        }

        public void Buy()
        {
            var dialog = new MessageDialog(text + '\n' + "Do you want to buy this?");
            dialog.YesAction = YesAction;
        }

        void YesAction()
        {
            new MessageBox("Thank you.");
            if (GameManager.Current.Player.Stats.Coins >= price || true)
            {
                GameManager.Current.Player.Stats.Coins -= price;
                new FollowFont(GameManager.Current.Player.X, GameManager.Current.Player.Y - 12, $"-{price}$");

                if (!respawn)
                {
                    GameManager.Current.Player.Stats.Items.Add(ID, Name);
                    //Sold = true;
                }
            }
            
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //base.Draw(sb, gameTime);
            
            if (!Sold)
            {
                // info
                if (showInfo)
                {
                    var infoY = -2 * Globals.TILE;
                    if (Y < RoomCamera.Current.ViewY + 3 * Globals.TILE)
                        infoY = 1 * Globals.TILE + 4;
                    font.Draw(sb, X, Y + infoY, $"~{Name}~" + '\n' + $"{price}$");
                }
                else
                {
                    // $
                    sb.Draw(AssetManager.ShopItems, Position + new Vector2(0, -Globals.TILE + (float)z), new Rectangle(0, 0, 16, 16), Color.White, 0, DrawOffset, Vector2.One, SpriteEffects.None, Depth);
                }
            }
            
            if (!Sold)
            {
                sb.Draw(AssetManager.ShopItems, new Vector2(X, Y), new Rectangle(0, 16, 16, 16), Color.White, 0, DrawOffset, Vector2.One, SpriteEffects.None, Depth);
            } else
            {
                sb.Draw(AssetManager.ShopItems, new Vector2(X, Y), new Rectangle(16, 0, 16, 16), Color.White, 0, DrawOffset, Vector2.One, SpriteEffects.None, Depth);
            }
        }
    }
}