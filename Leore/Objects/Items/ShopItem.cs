using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using SPG.Objects;
using SPG.Draw;
using SPG;
using Leore.Objects.Effects;
using Leore.Main;
using Leore.Objects.Level;
using System.Diagnostics;

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
        private Font previewFont;

        private LightSource light;

        public bool Sold { get; private set; }
        public bool CanBeBought { get => !Room.SwitchState && !Sold; }
        
        public ShopItem(int x, int y, Room room, string name, int itemType, int price, string displayText, string acquireText, bool shopItemRespawn, string appearCondition) : base(x, y, room, name)
        {
            DebugEnabled = true;
            BoundingBox = new SPG.Util.RectF(-4, -4, 8, 8);

            DrawOffset = new Vector2(8);
            Depth = Globals.LAYER_FG + .0001f;

            previewFont = AssetManager.DefaultFont.Copy();
            previewFont.HighlightColor = Colors.FromHex("FFF382");
            previewFont.Halign = Font.HorizontalAlignment.Center;
            previewFont.Valign = Font.VerticalAlignment.Center;

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

            if (GameManager.Current.Player.Stats.Items.ContainsKey(ID) && !respawn)
                Sold = true;
            
            if (!GameManager.Current.HasStoryFlag(appearCondition))
                Sold = true;

            if (!Sold)
                showInfo = this.CollisionBounds(GameManager.Current.Player, X, Y);

            switch (type)
            {
                case 2: // hp
                    var hpCount = GameManager.Current.GetStatUpItemCount("hp");                    
                    price = 500 + 500 * hpCount + 1500 * hpCount + 2000 * Math.Max(hpCount - 3, 0);                    
                    break;
                case 3: // mp
                    var mpCount = GameManager.Current.GetStatUpItemCount("mp");
                    price = 2500 + 400 * mpCount + 1600 * mpCount + 2000 * Math.Max(mpCount - 3, 0);                    
                    break;
                case 4: // regen
                    var regenCount = GameManager.Current.GetStatUpItemCount("regen");
                    price = 500 * (regenCount + 1) + 1500 * regenCount + 2000 * Math.Max(regenCount - 3, 0);
                    break;
            }

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
                if (!respawn)
                    Sold = true;
                
                GameManager.Current.Player.Stats.Coins -= price;
                new FollowFont(GameManager.Current.Player.X, GameManager.Current.Player.Y - 12, $"-{price}$");

                GameManager.Current.OverwriteSwitchStateTo(true);

                //if (!respawn)
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
                        case 1: // spell: crimson
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
                        case 2: // HP up
                            var hpUp = new StatUpItem(X, Y - Globals.T, Room, StatUpItem.StatType.HP, true);                            
                            hpUp.Respawn = true;
                            hpUp.Save = false;
                            GameManager.Current.AddStatUpItemCount("hp");
                            break;
                        case 3: // MP up
                            var mpUp = new StatUpItem(X, Y - Globals.T, Room, StatUpItem.StatType.MP, true);                            
                            mpUp.Respawn = true;
                            mpUp.Save = false;
                            GameManager.Current.AddStatUpItemCount("mp");
                            break;
                        case 4: // Regen up
                            var regenUp = new StatUpItem(X, Y - Globals.T, Room, StatUpItem.StatType.Regen, true);                            
                            regenUp.Respawn = true;
                            regenUp.Save = false;
                            GameManager.Current.AddStatUpItemCount("regen");
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
                if (showInfo && CanBeBought)
                {
                    var infoX = RoomCamera.Current.ViewX + RoomCamera.Current.ViewWidth * .5f;
                    var infoY = RoomCamera.Current.ViewY + RoomCamera.Current.ViewHeight - 1 * Globals.T;

                    int count = 0;
                    if (type == 2) count = GameManager.Current.GetStatUpItemCount("hp");
                    if (type == 3) count = GameManager.Current.GetStatUpItemCount("mp");
                    if (type == 4) count = GameManager.Current.GetStatUpItemCount("regen");

                    if (count > 0)
                        font.Draw(sb, infoX, infoY, $"~{Name} ({count})~" + '\n' + $"{price}$", depth: Depth + .0001f);
                    else
                        font.Draw(sb, infoX, infoY, $"~{Name}~" + '\n' + $"{price}$", depth: Depth + .0001f);
                    
                    // $
                    sb.Draw(AssetManager.ShopItems, Position + new Vector2(0, Globals.T + (float)z), new Rectangle(0, 0, 16, 16), Color.White, 0, DrawOffset, Vector2.One, SpriteEffects.None, Depth + .0001f);

                }
            }
            
            if (!Sold)
            {
                if (CanBeBought)
                {
                    sb.Draw(AssetManager.ShopItems, new Vector2(X, Y - 16 - (float)z), new Rectangle(16 * type, 16, 16, 32), Color.White, 0, DrawOffset, Vector2.One, SpriteEffects.None, Depth);
                } else
                {
                    // silhouette
                    sb.Draw(AssetManager.ShopItems, new Vector2(X, Y - 16), new Rectangle(16 * type, 16, 16, 32), new Color(Color.Black, .2f), 0, DrawOffset, Vector2.One, SpriteEffects.None, Depth);
                }

                // podest
                sb.Draw(AssetManager.ShopItems, new Vector2(X, Y), new Rectangle(32, 0, 16, 16), Color.White, 0, DrawOffset, Vector2.One, SpriteEffects.None, Depth - .0001f);                
            } else
            {
                // sold-out 
                sb.Draw(AssetManager.ShopItems, new Vector2(X, Y), new Rectangle(16, 0, 16, 16), Color.White, 0, DrawOffset, Vector2.One, SpriteEffects.None, Depth + .0001f);

                // silhouette
                sb.Draw(AssetManager.ShopItems, new Vector2(X, Y - 16), new Rectangle(16 * type, 16, 16, 32), new Color(Color.Black, .2f), 0, DrawOffset, Vector2.One, SpriteEffects.None, Depth);
            }
        }
    }
}