using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using SPG.Objects;
using SPG.Draw;
using SPG;
using Leore.Objects.Effects;
using Leore.Main;
using Leore.Objects.Level;

namespace Leore.Objects.Items
{
    public class ShopItem : RoomObject
    {
        private int type;
        private int price;
        private string displayText;
        private string acquireText;
        private bool respawn;
        private string appearCondition;

        private double t = 0;
        private double z = 0;
        private bool showInfo;

        private Font font;

        private LightSource light;

        public bool Sold { get; private set; }
        public bool CanBeBought { get => !Room.SwitchState && !Sold; }
        
        public ShopItem(int x, int y, Room room, string name, int itemType, int price, string displayText, string acquireText, bool shopItemRespawn, string appearCondition) : base(x, y, room, name)
        {
            DebugEnabled = true;
            BoundingBox = new SPG.Util.RectF(-4, -4, 8, 8);

            DrawOffset = new Vector2(8);
            Depth = Globals.LAYER_FG + .0001f;

            font = AssetManager.DamageFont.Copy();
            font.HighlightColor = Colors.FromHex("FFF382");
            font.Halign = Font.HorizontalAlignment.Center;
            font.Valign = Font.VerticalAlignment.Center;

            this.type = itemType;
            this.price = price;
            this.displayText = displayText;
            this.acquireText = acquireText;
            this.respawn = shopItemRespawn;
            this.appearCondition = appearCondition;

            light = new LightSource(this);
            light.Active = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (GameManager.Current.Player.Stats.Items.ContainsKey(ID))
                Sold = true;

            //// magic can't be bought until player has the orb
            //if ((type == 1 || type == 2) && !GameManager.Current.Player.Stats.Abilities.HasFlag(PlayerAbility.ORB))
            //    Sold = true;

            if (!GameManager.Current.HasStoryFlag(appearCondition))
                Sold = true;

            if (!Sold)
                showInfo = this.CollisionBounds(GameManager.Current.Player, X, Y);
            
            t = (t + .05f) % (2 * Math.PI);
            z = Math.Sin(t);            
        }

        public void Buy()
        {
            var dialog = new MessageDialog(displayText + '|' + "Do you want to buy this?");
            dialog.YesAction = YesAction;
        }

        void YesAction()
        {
            if (GameManager.Current.Player.Stats.Coins >= price)
            {
                Sold = true;
                
                GameManager.Current.Player.Stats.Coins -= price;
                new FollowFont(GameManager.Current.Player.X, GameManager.Current.Player.Y - 12, $"-{price}$");

                GameManager.Current.OverwriteSwitchStateTo(true);

                if (!respawn)
                {
                    // add this only for items that are not already adding the ID when taken
                    //GameManager.Current.Player.Stats.Items.Add(ID, Name);

                    switch (type)
                    {
                        case 0: // feather
                            var featherItem = new AbilityItem(X, Y - Globals.T, Room, Name);
                            featherItem.Texture = AssetManager.Items[5];
                            featherItem.OnObtain = () =>
                            {
                                GameManager.Current.Player.Stats.Abilities |= PlayerAbility.NO_FALL_DAMAGE;
                                GameManager.Current.OverwriteSwitchStateTo(false);
                            };
                            featherItem.Text = acquireText;
                            featherItem.ID = ID;
                            break;
                        case 1: // MP crystal
                            var mpUp = new StatUpItem(X, Y - Globals.T, Room, StatUpItem.StatType.MP, true);                            
                            mpUp.ID = ID;
                            break;
                        case 2: // spell: crimson
                            var crimsonItem = new AbilityItem(X, Y - Globals.T, Room, Name);
                            crimsonItem.Texture = AssetManager.Items[6];
                            crimsonItem.OnObtain = () =>
                            {
                                GameManager.Current.AddSpell(SpellType.CRIMSON_ARC);
                                GameManager.Current.OverwriteSwitchStateTo(false);
                            };
                            crimsonItem.HighlightColor = Colors.FromHex("c80e1f");
                            crimsonItem.Text = acquireText;
                            crimsonItem.ID = ID;
                            break;
                    }
                }

                //new MessageBox("Thank you!|..don't forget to take that item with you!");
                new MessageBox("Thank you!");

            } else
            {
                new MessageBox("I'm afraid you can't afford it..");
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
                    var infoY = -2 * Globals.T;
                    if (Y < RoomCamera.Current.ViewY + 4 * Globals.T)
                        infoY = -1 * Globals.T;
                    font.Draw(sb, X, Y + infoY, $"~{Name}~" + '\n' + $"{price}$");
                }
                else
                {
                    // $
                    if (CanBeBought)
                        sb.Draw(AssetManager.ShopItems, Position + new Vector2(0, -Globals.T + (float)z), new Rectangle(0, 0, 16, 16), Color.White, 0, DrawOffset, Vector2.One, SpriteEffects.None, Depth);
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