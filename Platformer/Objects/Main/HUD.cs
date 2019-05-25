using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Main;
using SPG.Draw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Main
{
    public class HUD
    {
        private float hp;
        private float maxHP;
        private float mp;
        private float maxMP;
        private float coins;

        public Texture2D Texture { get; set; }


        private Font font;
        private Font hudFont;
        private Player player;
        
        public HUD()
        {
            //font = AssetManager.HUDFontSmall;
            font = AssetManager.DamageFont;
            hudFont = AssetManager.HUDFont;
        }

        internal void SetTarget(Player player)
        {
            this.player = player;
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
            var x = RoomCamera.Current.ViewX;
            var y = RoomCamera.Current.ViewY;

            var s = 1f;

            font.Valign = Font.VerticalAlignment.Top;

            // ----- left -----

            // HP

            var hpx = x + RoomCamera.Current.ViewWidth - 64 * s;
            var hpy = y;

            font.Halign = Font.HorizontalAlignment.Center;

            sb.Draw(AssetManager.HUD, new Vector2(hpx - 2, hpy + 2), new Rectangle(0, 0, 64, 16), Color.White, 0, Vector2.Zero, new Vector2(s), SpriteEffects.None, Globals.LAYER_UI + .00001f);
            sb.Draw(AssetManager.HUD, new Vector2(hpx - 2 + 4, hpy + 2), new Rectangle(64 + 4, 0, (int)((64 - 8) * hp / maxHP), 16), Color.White, 0, Vector2.Zero, new Vector2(s), SpriteEffects.None, Globals.LAYER_UI + .00002f);

            sb.Draw(AssetManager.HUD, new Vector2(hpx - 2, hpy + 16 + 2), new Rectangle(0, 32, 64, 16), Color.White, 0, Vector2.Zero, new Vector2(s), SpriteEffects.None, Globals.LAYER_UI + .00001f);
            
            font.Draw(sb, hpx + 32 - 1, hpy, $"{hp}/{maxHP}", depth: Globals.LAYER_UI + .00003f);

            // ----- right -----

            var mpx = x + 2;
            var mpy = y;

            // COIN

            sb.Draw(AssetManager.HUD, new Vector2(hpx + 48, hpy + 18), new Rectangle(0, 112, 16, 16), Color.White, 0, Vector2.Zero, new Vector2(s), SpriteEffects.None, Globals.LAYER_UI + .00001f);

            font.Halign = Font.HorizontalAlignment.Right;
            font.Draw(sb, hpx + 48, hpy + 21, $"{coins}", scale: s, depth: Globals.LAYER_UI + .00003f);

            if (player.Orb != null)
            {

                var spell = player.Stats.Spells.ElementAt(player.Stats.SpellIndex).Key;
                var spellLevel = (int)player.Stats.Spells[spell] - 1;

                int maxExp = player.Orb.MaxEXP[spell][player.Stats.Spells[spell]];
                float expRatio = 0;
                if (maxExp != 0)
                    expRatio = player.Stats.SpellEXP[spell] / (float)maxExp;

                // MP

                font.Halign = Font.HorizontalAlignment.Left;

                sb.Draw(AssetManager.HUD, new Vector2(mpx + 16, mpy + 2), new Rectangle(0, 16, 64, 16), Color.White, 0, Vector2.Zero, new Vector2(s), SpriteEffects.None, Globals.LAYER_UI + .00001f);
                sb.Draw(AssetManager.HUD, new Vector2(mpx + 16 + 4, mpy + 2), new Rectangle(64 + 4, 16 + 16 * spellLevel, (int)((64 - 8) * mp / maxMP), 16), Color.White, 0, Vector2.Zero, new Vector2(s), SpriteEffects.None, Globals.LAYER_UI + .00003f);
                font.Draw(sb, mpx + 21, mpy + 5f, $"{mp}", scale: s, depth: Globals.LAYER_UI + .00004f);

                // EXP
                sb.Draw(AssetManager.HUD, new Vector2(mpx, mpy + 5), new Rectangle(0, 64, 80, 16), Color.White, 0, Vector2.Zero, new Vector2(s), SpriteEffects.None, Globals.LAYER_UI + .00002f);
                sb.Draw(AssetManager.HUD, new Vector2(mpx, mpy + 5), new Rectangle(0, 80, (int)(80 * expRatio), 16), Color.White, 0, Vector2.Zero, new Vector2(s), SpriteEffects.None, Globals.LAYER_UI + .00003f);

                // LV
                sb.Draw(AssetManager.HUD, new Vector2(mpx, mpy + 2), new Rectangle(16 * spellLevel, 96, 16, 16), Color.White, 0, Vector2.Zero, new Vector2(s), SpriteEffects.None, Globals.LAYER_UI + .00002f);
            }
        }

        // OLD (.5x scaled HUD)

        //public void Draw(SpriteBatch sb, GameTime gameTime)
        //{
        //    var x = RoomCamera.Current.ViewX;
        //    var y = RoomCamera.Current.ViewY;

        //    var s = 1f;

        //    font.Valign = Font.VerticalAlignment.Top;

        //    // ----- left -----

        //    // HP

        //    var hpx = x + RoomCamera.Current.ViewWidth - 80 * s - 4 * s;
        //    var hpy = y;

        //    font.Halign = Font.HorizontalAlignment.Center;

        //    sb.Draw(AssetManager.HUD, new Vector2(hpx + 2 * s, hpy), new Rectangle(0, 0, 80, 32), Color.White, 0, Vector2.Zero, new Vector2(s), SpriteEffects.None, Globals.LAYER_UI + .00001f);
        //    sb.Draw(AssetManager.HUD, new Vector2(hpx + 2 * s + 8 * s, hpy), new Rectangle(80, 0, (int)(64 * hp / maxHP), 32), Color.White, 0, Vector2.Zero, new Vector2(s), SpriteEffects.None, Globals.LAYER_UI + .00002f);

        //    //if (hp > 0)
        //        //font.Draw(sb, hpx + 80 * s - 6 * s, hpy + 11 * s, $"{hp}", scale: .5f, depth: Globals.LAYER_UI + .00003f);
        //    font.Draw(sb, hpx + 42 * s, hpy + 4 * s, $"{hp}/{maxHP}", scale: s, depth: Globals.LAYER_UI + .00003f);

        //    // ----- right -----

        //    var mpx = x + 2;
        //    var mpy = y;

        //    // COIN

        //    sb.Draw(AssetManager.HUD, new Vector2(hpx + 80 * s - 16 * s, hpy + 32 * s), new Rectangle(0, 112, 16, 16), Color.White, 0, Vector2.Zero, new Vector2(s), SpriteEffects.None, Globals.LAYER_UI + .00001f);

        //    hudFont.Valign = Font.VerticalAlignment.Top;
        //    hudFont.Halign = Font.HorizontalAlignment.Right;
        //    hudFont.Draw(sb, hpx + 80 * s - 16 * s, hpy + 33 * s, $"{coins}", scale: s, depth: Globals.LAYER_UI + .00003f);

        //    if (player.Orb != null)
        //    {

        //        var spell = player.Stats.Spells.ElementAt(player.Stats.SpellIndex).Key;
        //        var spellLevel = (int)player.Stats.Spells[spell] - 1;

        //        int maxExp = player.Orb.MaxEXP[spell][player.Stats.Spells[spell]];
        //        float expRatio = 0;
        //        if (maxExp != 0)
        //            expRatio = player.Stats.SpellEXP[spell] / (float)maxExp;

        //        // MP

        //        font.Halign = Font.HorizontalAlignment.Left;

        //        sb.Draw(AssetManager.HUD, new Vector2(mpx + 30 * s, mpy), new Rectangle(0, 32, 80, 32), Color.White, 0, Vector2.Zero, new Vector2(s), SpriteEffects.None, Globals.LAYER_UI + .00001f);
        //        sb.Draw(AssetManager.HUD, new Vector2(mpx + 30 * s + 8 * s, mpy), new Rectangle(160, 32 + 32 * spellLevel, (int)(64 * mp / maxMP), 32), Color.White, 0, Vector2.Zero, new Vector2(s), SpriteEffects.None, Globals.LAYER_UI + .00003f);
        //        font.Draw(sb, mpx + 40 * s, mpy + 11 * s, $"{mp}", scale: s, depth: Globals.LAYER_UI + .00004f);

        //        // EXP
        //        sb.Draw(AssetManager.HUD, new Vector2(mpx, mpy + 14 * s), new Rectangle(0, 64, 112, 16), Color.White, 0, Vector2.Zero, new Vector2(s), SpriteEffects.None, Globals.LAYER_UI + .00002f);
        //        sb.Draw(AssetManager.HUD, new Vector2(mpx, mpy + 14 * s), new Rectangle(0, 80, (int)(112 * expRatio), 16), Color.White, 0, Vector2.Zero, new Vector2(s), SpriteEffects.None, Globals.LAYER_UI + .00003f);

        //        // LV
        //        sb.Draw(AssetManager.HUD, new Vector2(mpx + 8 * s, mpy + 8 * s), new Rectangle(16 * spellLevel, 96, 16, 16), Color.White, 0, Vector2.Zero, new Vector2(s), SpriteEffects.None, Globals.LAYER_UI + .00002f);                
        //    }            
        //}
    }
}
