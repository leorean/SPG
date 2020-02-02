using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Resources;
using SPG.Draw;
using System;
using System.Linq;
using Leore.Objects.Enemies;
using SPG.Objects;

namespace Leore.Main
{
    public class HUD
    {
        private float hp;
        private float maxHP;
        private float mp;
        private float maxMP;
        private float coins;

        public Texture2D Texture { get; set; }

        private bool visible;

        private Font font;
        private Font hudFont;

        private Boss boss;
        internal void SetBoss(Boss boss)
        {
            this.boss = boss;
        }

        private Player player => GameManager.Current.Player;
        
        public HUD()
        {
            //font = AssetManager.HUDFontSmall;
            font = AssetManager.DamageFont;
            hudFont = AssetManager.HUDFont;

            visible = true;
        }
        
        public void Update(GameTime gameTime)
        {
            if (player == null)
                return;

            var stats = player.Stats;
            
            hp = player.HP;
            maxHP = stats.MaxHP;
            mp = (float)Math.Floor(player.MP);
            maxMP = stats.MaxMP;
            coins = stats.Coins;            
        }

        public void Draw(SpriteBatch sb, GameTime gameTime)
        {
            if (!visible)
                return;

            if (player == null)
                return;

            var x = RoomCamera.Current.ViewX;
            var y = RoomCamera.Current.ViewY;
            
            font.Valign = Font.VerticalAlignment.Top;

            // ----- left -----

            // HP

            var hpx = x + RoomCamera.Current.ViewWidth - 64;
            var hpy = y;

            font.Halign = Font.HorizontalAlignment.Center;

            sb.Draw(AssetManager.HUD, new Vector2(hpx - 2, hpy + 2), new Rectangle(0, 0, 64, 16), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_UI + .00001f);
            sb.Draw(AssetManager.HUD, new Vector2(hpx - 2 + 4, hpy + 2), new Rectangle(64 + 4, 0, (int)((64 - 8) * hp / maxHP), 16), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_UI + .00002f);

            sb.Draw(AssetManager.HUD, new Vector2(hpx - 2, hpy + 16 + 2), new Rectangle(0, 32, 64, 16), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_UI + .00001f);
            
            font.Draw(sb, hpx + 32 - 1, hpy + 5f, $"{hp}/{maxHP}", depth: Globals.LAYER_UI + .00003f);

            // ----- right -----

            var mpx = x + 2;
            var mpy = y;

            // COIN

            sb.Draw(AssetManager.HUD, new Vector2(hpx + 48, hpy + 18), new Rectangle(48, 96, 16, 16), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_UI + .00001f);

            font.Halign = Font.HorizontalAlignment.Right;
            font.Draw(sb, hpx + 48, hpy + 21, $"{coins}", depth: Globals.LAYER_UI + .00003f);

            // KEYS (held)

            if (player.HasAtLeastOneKey())
            {
                sb.Draw(AssetManager.HUD, new Vector2(hpx + 48, hpy + 34), new Rectangle(64, 96, 16, 16), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_UI + .00001f);                
                font.Draw(sb, hpx + 54, hpy + 36, $"{player.Stats.HeldKeys[GameManager.Current.Map.Name]}x", depth: Globals.LAYER_UI + .00003f);

            }

            if (player.Orb != null)
            {

                var spell = player.Stats.Spells.ElementAt(player.Stats.SpellIndex).Key;
                var spellLevel = (int)player.Stats.Spells[spell] - 1;

                int maxExp = GameResources.MaxEXP[spell][player.Stats.Spells[spell]];
                float expRatio = 0;
                if (maxExp != 0)
                    expRatio = player.Stats.SpellEXP[spell] / (float)maxExp;

                // MP

                font.Halign = Font.HorizontalAlignment.Left;

                sb.Draw(AssetManager.HUD, new Vector2(mpx + 16, mpy + 2), new Rectangle(0, 16, 64, 16), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_UI + .00001f);
                sb.Draw(AssetManager.HUD, new Vector2(mpx + 16 + 4, mpy + 2), new Rectangle(64 + 4, 16 + 16 * spellLevel, (int)((64 - 8) * mp / maxMP), 16), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_UI + .00003f);
                font.Draw(sb, mpx + 21, mpy + 5f, $"{mp}", depth: Globals.LAYER_UI + .00004f);

                // EXP
                sb.Draw(AssetManager.HUD, new Vector2(mpx, mpy + 6), new Rectangle(0, 64, 80, 16), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_UI + .00002f);
                sb.Draw(AssetManager.HUD, new Vector2(mpx, mpy + 6), new Rectangle(0, 80, (int)(80 * expRatio), 16), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_UI + .00003f);

                // LV icon
                sb.Draw(AssetManager.HUD, new Vector2(mpx + 1, mpy + 3), new Rectangle(16 * spellLevel, 96 + 16 * (int)spell, 16, 16), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_UI + .00002f);
            }

            if (boss != null && !ObjectManager.Exists<MessageBox>())
            {
                // icon
                sb.Draw(AssetManager.HUD, new Vector2(x + 48, y + 128), new Rectangle(128, 32, 16, 16), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_UI + .00001f);

                sb.Draw(AssetManager.HUD, new Vector2(x + 64, y + 128), new Rectangle(128, 0, 128, 16), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_UI + .00001f);
                sb.Draw(AssetManager.HUD, new Vector2(x + 64 + 4, y + 128), new Rectangle(128 + 4, 16, (int)((128 - 8) * boss.HP / (float)boss.MaxHP), 16), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_UI + .00002f);
            }
        }

        public void SetVisible(bool visible)
        {
            this.visible = visible;
        }
    }
}
