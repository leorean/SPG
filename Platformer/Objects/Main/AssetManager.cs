using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SPG.Draw;
using SPG.Map;
using SPG.Util;
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

        public static TextureSet Player { get; private set; }
        public static TextureSet Effects { get; private set; }
        public static TextureSet Projectiles { get; private set; }
        public static TextureSet NPCS { get; private set; }
        public static TextureSet SaveStatue { get; private set; }

        // items etc.

        public static TextureSet Items { get; private set; }
        public static TextureSet Potions { get; private set; }
        public static TextureSet Coins { get; private set; }
        public static TextureSet Chest { get; private set; }
        public static TextureSet SpellEXP { get; private set; }

        // HUD

        public static Texture2D HUD { get; private set; }
        public static Texture2D MessageBox { get; private set; }

        // misc

        public static Texture2D Door { get; private set; }
        public static Texture2D StarParticle { get; private set; }
        public static Texture2D WhiteCircle { get; private set; }
        public static Texture2D Flash { get; private set; }
        public static Texture2D Darkness { get; private set; }
        public static Texture2D PlayerGhost { get; private set; }
        public static TextureSet MovingPlatform { get; private set; }
        public static TextureSet GroundSwitch { get; private set; }
        public static TextureSet ToolTip { get; private set; }
        public static Texture2D WaterMill { get; private set; }
        public static Texture2D WaterMillPlatform { get; private set; }

        // enemies

        public static TextureSet EnemyGrassy { get; private set; }

        // orb

        public static TextureSet Orbs { get; private set; }
        
        // fonts

        public static Font DefaultFont { get; private set; }
        public static Font DamageFont { get; private set; }
        public static Font HUDFont { get; private set; }
        
        public static void InitializeContent(ContentManager content)
        {
            // tileset

            TileSet = content.LoadTextureSet("tiles");

            // sprites etc.

            Player = content.LoadTextureSet("player", 16, 32);
            Effects = content.LoadTextureSet("effects", 32, 32);
            Projectiles = content.LoadTextureSet("projectiles", 16, 16);
            NPCS = content.LoadTextureSet("npc", 16, 32);
            SaveStatue = content.LoadTextureSet("save");

            Door = content.Load<Texture2D>("door");
            StarParticle = content.Load<Texture2D>("ouch");
            WhiteCircle = content.Load<Texture2D>("whiteCircle");
            Flash = content.Load<Texture2D>("flash");
            Darkness = content.Load<Texture2D>("darkness");
            PlayerGhost = content.Load<Texture2D>("playerGhost");
            MovingPlatform = content.LoadTextureSet("movingPlatform", 32, 32);
            GroundSwitch = content.LoadTextureSet("groundSwitch");
            ToolTip = content.LoadTextureSet("toolTip", 32, 32);

            var waterMillSheet = content.Load<Texture2D>("watermill");
            WaterMill = waterMillSheet.Crop(new Rectangle(0, 0, 128, 128));
            WaterMillPlatform = waterMillSheet.Crop(new Rectangle(128, 0, 16, 16));

            EnemyGrassy = content.LoadTextureSet("enemyGrassy");

            Orbs = content.LoadTextureSet("orb", 32, 32);

            MessageBox = content.Load<Texture2D>("messageBox");
            HUD = content.Load<Texture2D>("hud");

            Items = content.LoadTextureSet("items", 32, 32);
            Potions = content.LoadTextureSet("potions");
            Coins = content.LoadTextureSet("coins");
            Chest = content.LoadTextureSet("chest");
            SpellEXP = content.LoadTextureSet("spellexp");

            // backgrounds

            Backgrounds = content.LoadTextureSet("backgrounds", 16 * Globals.TILE, 9 * Globals.TILE);

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
