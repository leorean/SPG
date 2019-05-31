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

            // spell can't be bought until player has the orb
            if (type == 2 && !GameManager.Current.Player.Stats.Abilities.HasFlag(PlayerAbility.ORB))
                Sold = true;

            if (!Sold)
                showInfo = this.CollisionBounds(GameManager.Current.Player, X, Y);
            
            t = (t + .05f) % (2 * Math.PI);
            z = Math.Sin(t);
        }

        public void Buy()
        {
            var dialog = new MessageDialog(text + '|' + "Do you want to buy this?");
            dialog.YesAction = YesAction;
        }

        void YesAction()
        {
            Sold = true;
            //new MessageBox("Thank you.");
            if (GameManager.Current.Player.Stats.Coins >= price || true)
            {
                GameManager.Current.Player.Stats.Coins -= price;
                new FollowFont(GameManager.Current.Player.X, GameManager.Current.Player.Y - 12, $"-{price}$");

                if (!respawn)
                {
                    // add this only for items that are not already adding the ID when taken
                    //GameManager.Current.Player.Stats.Items.Add(ID, Name);

                    switch (type)
                    {
                        case 0: // feather
                            var featherItem = new AbilityItem(X, Y - Globals.TILE, Room, Name);
                            featherItem.Texture = AssetManager.Items[5];
                            featherItem.OnObtain = () =>
                            {
                                GameManager.Current.Player.Stats.Abilities |= PlayerAbility.NO_FALL_DAMAGE;
                            };
                            featherItem.Text = $"You got the ~Feather~!\nIt prevents fall damage.";
                            featherItem.ID = ID;
                            break;
                        case 1: // MP crystal
                            var mpUp = new StatUpItem(X, Y - Globals.TILE, Room, StatUpItem.StatType.MP);
                            mpUp.ID = ID;
                            break;
                        case 2: // spell: crimson
                            var crimsonItem = new AbilityItem(X, Y - Globals.TILE, Room, Name);
                            crimsonItem.Texture = AssetManager.Items[6];
                            crimsonItem.OnObtain = () =>
                            {
                                GameManager.Current.AddSpell(Main.Orbs.SpellType.CRIMSON);
                            };
                            crimsonItem.HighlightColor = Colors.FromHex("9f0011");
                            crimsonItem.Text = $"Learned spell: ~Crimson~.";
                            crimsonItem.ID = ID;
                            break;
                    }

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
                    if (Y < RoomCamera.Current.ViewY + 4 * Globals.TILE)
                        infoY = -1 * Globals.TILE - 8;
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
                sb.Draw(AssetManager.ShopItems, new Vector2(X, Y), new Rectangle(16 * type, 16, 16, 16), Color.White, 0, DrawOffset, Vector2.One, SpriteEffects.None, Depth);
            } else
            {
                sb.Draw(AssetManager.ShopItems, new Vector2(X, Y), new Rectangle(16, 0, 16, 16), Color.White, 0, DrawOffset, Vector2.One, SpriteEffects.None, Depth);
            }
        }
    }
}