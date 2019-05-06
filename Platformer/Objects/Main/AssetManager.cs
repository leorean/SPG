using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SPG.Draw;
using SPG.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Main
{
    public static class AssetManager
    {
        // common textures & objects

        public static TextureSet TileSet { get; private set; }

        public static TextureSet Backgrounds { get; private set; }

        public static TextureSet PlayerSprites { get; private set; }
        public static TextureSet EffectSprites { get; private set; }
        public static TextureSet SaveStatueSprites { get; private set; }

        // items etc.

        public static TextureSet ItemSprites { get; private set; }
        public static TextureSet PotionSprites { get; private set; }
        public static TextureSet CoinSprites { get; private set; }

        // HUD

        public static Texture2D HUDSprite { get; private set; }
        public static Texture2D MessageBoxSprite { get; private set; }

        // misc

        public static Texture2D OuchSprite { get; private set; }
        public static Texture2D WhiteCircleSprite { get; private set; }
        public static Texture2D FlashSprite { get; private set; }
        public static Texture2D PlayerGhostSprite { get; private set; }

        // fonts

        public static Font DefaultFont { get; private set; }
        public static Font DamageFont { get; private set; }
        public static Font HUDFont { get; private set; }
        
        public static void InitializeContent(ContentManager content)
        {
            // tileset

            TileSet = content.LoadTextureSet("tiles");

            // sprites etc.

            SaveStatueSprites = content.LoadTextureSet("save");
            PlayerSprites = content.LoadTextureSet("player", 16, 32);
            EffectSprites = content.LoadTextureSet("effects", 32, 32);
            
            OuchSprite = content.Load<Texture2D>("ouch");
            WhiteCircleSprite = content.Load<Texture2D>("whiteCircle");
            FlashSprite = content.Load<Texture2D>("flash");
            PlayerGhostSprite = content.Load<Texture2D>("playerGhost");
            

            MessageBoxSprite = content.Load<Texture2D>("messageBox");
            HUDSprite = content.Load<Texture2D>("hud");

            ItemSprites = content.LoadTextureSet("items");
            PotionSprites = content.LoadTextureSet("potions");
            CoinSprites = content.LoadTextureSet("coins");

            // backgrounds

            Backgrounds = content.LoadTextureSet("background", 16 * Globals.TILE, 9 * Globals.TILE);

            // load fonts

            var defaultFont = content.LoadTextureSet("font", 10, 10);
            var damageFont = content.LoadTextureSet("damageFont", 10, 10);
            var hudFont = content.LoadTextureSet("hudFont", 9, 14);

            DefaultFont = new Font(defaultFont, ' ');
            DamageFont = new Font(damageFont, ' ');
            HUDFont = new Font(hudFont, ' ');

        }
    }
}
