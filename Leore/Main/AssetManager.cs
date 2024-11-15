﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SPG.Draw;
using SPG.Map;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Leore.Main
{
    public static class Sounds
    {
        // sounds
        public static SoundEffect Coin0 { get; private set; }
        public static SoundEffect MsgChar { get; private set; }
        public static SoundEffect MsgChoose { get; private set; }
        public static SoundEffect MsgSelectYes { get; private set; }
        public static SoundEffect MsgSelectNo { get; private set; }
        public static SoundEffect MsgNextPage { get; private set; }
        public static SoundEffect Jump { get; private set; }
        public static SoundEffect Hurt { get; private set; }
        public static SoundEffect HitGround { get; private set; }
        public static SoundEffect Boing { get; private set; }
        public static SoundEffect Snap { get; private set; }
        public static SoundEffect LetGo { get; private set; }
        public static SoundEffect WaterSplash { get; private set; }
        public static SoundEffect WaterSwim { get; private set; }
        public static SoundEffect WaterSplashBubble { get; private set; }
        public static SoundEffect Walk { get; private set; }

        public static void Load(SoundEffect sound) 
        {
            var prop = typeof(Sounds).GetProperties().Where(x => x.Name.Equals(sound.Name, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (prop != null)
            {
                prop.SetValue(null, sound);
            }
        }
    }
    public static class AssetManager
    {
        // common textures & objects

        public static Texture2D TitleMenu { get; private set; }
        public static TextureSet Scenes { get; private set; }

        public static TextureSet TileSet { get; private set; }

        public static TextureSet Backgrounds { get; private set; }

        public static Texture2D Darkness { get; private set; }
        public static Texture2D DarknessMask { get; set; }

        public static TextureSet Player { get; private set; }
        public static TextureSet Effects { get; private set; }
        public static TextureSet Projectiles { get; private set; }
        public static TextureSet NPCS { get; private set; }
        public static TextureSet SaveStatue { get; private set; }
        public static Texture2D Teleporter { get; private set; }
        
        // items etc.

        public static TextureSet Items { get; private set; }
        public static TextureSet Potions { get; private set; }
        public static TextureSet Coins { get; private set; }
        public static TextureSet Chest { get; private set; }
        public static TextureSet SpellEXP { get; private set; }
        public static Texture2D ShopItems { get; private set; }

        // HUD

        public static Texture2D HUD { get; private set; }
        public static Texture2D MessageBox { get; private set; }

        // misc

        public static TextureSet StoryWarp { get; private set; }
        public static Texture2D FireBall { get; private set; }
        public static Texture2D Door { get; private set; }
        public static TextureSet Torch { get; private set; }
        public static TextureSet Particles { get; private set; }
        public static TextureSet WhiteCircle { get; private set; }
        public static TextureSet VoidCircle { get; private set; }
        public static Texture2D Flash { get; private set; }
        public static TextureSet Transition { get; private set; }
        public static Texture2D PlayerGhost { get; private set; }
        public static TextureSet MovingPlatform { get; private set; }
        public static TextureSet TimeSwitch { get; private set; }
        public static TextureSet ToggleSwitch { get; private set; }
        public static TextureSet ToolTip { get; private set; }
        public static Texture2D WaterMill { get; private set; }
        public static Texture2D WaterMillPlatform { get; private set; }
        public static TextureSet Laser { get; private set; }
        public static TextureSet Lava { get; private set; }
        public static TextureSet WaterFall { get; private set; }
        public static TextureSet CoinStatue { get; private set; }
        public static TextureSet MirrorBossBG { get; private set; }
        public static TextureSet Collectable { get; private set; }

        // ambience

        public static TextureSet WaterSurface { get; private set; }
        public static TextureSet LittleStuff { get; private set; }

        // enemies

        public static TextureSet EnemyGrassy { get; private set; }
        public static TextureSet EnemyBat { get; private set; }
        public static TextureSet EnemyVoidling { get; private set; }
        public static TextureSet EnemySlime { get; private set; }
        public static TextureSet EnemySlurp { get; private set; }
        public static TextureSet EnemyDash { get; private set; }

        public static TextureSet BossGiantBat { get; private set; }
        public static TextureSet BossShadowLizard { get; private set; }

        // orb

        public static TextureSet Orbs { get; private set; }
        
        // fonts

        public static Font DefaultFont { get; private set; }
        public static Font MessageFont { get; private set; }
        public static Font DamageFont { get; private set; }
        public static Font HUDFont { get; private set; }
        public static Font HUDFontSmall { get; private set; }

        public static void InitializeContent(ContentManager content)
        {
            TitleMenu = content.Load<Texture2D>("titleMenu");
            Scenes = content.LoadTextureSet("scenes", 256, 144);

            // tileset

            TileSet = content.LoadTextureSet("tiles");

            // sprites etc.

            Darkness = content.Load<Texture2D>("darkness");
            DarknessMask = content.Load<Texture2D>("darknessMask");

            Player = content.LoadTextureSet("player", 16, 32);
            Effects = content.LoadTextureSet("effects", 32, 32);
            Projectiles = content.LoadTextureSet("projectiles", 16, 16);
            NPCS = content.LoadTextureSet("npc", 16, 32);
            SaveStatue = content.LoadTextureSet("save");
            Teleporter = content.Load<Texture2D>("teleporter");

            FireBall = content.Load<Texture2D>("fireball");
            Door = content.Load<Texture2D>("door");
            Torch = content.LoadTextureSet("torch");
            Particles = content.LoadTextureSet("particles", 16, 16);
            WhiteCircle = content.LoadTextureSet("whiteCircle", 64, 64);
            VoidCircle = content.LoadTextureSet("voidCircle", 64, 64);
            Flash = content.Load<Texture2D>("flash");
            Transition = content.LoadTextureSet("transition", 256, 144);
            PlayerGhost = content.Load<Texture2D>("playerGhost");
            MovingPlatform = content.LoadTextureSet("movingPlatform", 32, 32);
            TimeSwitch = content.LoadTextureSet("timeSwitch");
            ToggleSwitch = content.LoadTextureSet("toggleSwitch");
            ToolTip = content.LoadTextureSet("toolTip", 32, 32);
            StoryWarp = content.LoadTextureSet("storyWarp", 32, 144);

            MirrorBossBG = content.LoadTextureSet("mirrorBossBg", 256, 144);
            Collectable = content.LoadTextureSet("collectable", 16, 16);

            var waterMillSheet = content.Load<Texture2D>("watermill");
            WaterMill = waterMillSheet.Crop(new Rectangle(0, 0, 128, 128));
            WaterMillPlatform = waterMillSheet.Crop(new Rectangle(128, 0, 16, 16));

            Laser = content.LoadTextureSet("laser", 32, 32);
            Lava = content.LoadTextureSet("lava");
            WaterFall = content.LoadTextureSet("waterFall", 16, 32);
            CoinStatue = content.LoadTextureSet("coinStatue", 32, 32);

            WaterSurface = content.LoadTextureSet("waterSurface", 16, 16);
            LittleStuff = content.LoadTextureSet("littleStuff", 16, 16);

            EnemyGrassy = content.LoadTextureSet("enemyGrassy");
            EnemyBat = content.LoadTextureSet("enemyBat");
            EnemyVoidling = content.LoadTextureSet("enemyVoidling");
            EnemySlime = content.LoadTextureSet("enemySlime", 32, 64);
            BossGiantBat = content.LoadTextureSet("bossGiantBat", 80, 80);
            BossShadowLizard = content.LoadTextureSet("bossShadowLizard", 96, 96);
            EnemySlurp = content.LoadTextureSet("enemySlurp");
            EnemyDash = content.LoadTextureSet("enemyDash", 32, 32);

            Orbs = content.LoadTextureSet("orb", 16, 16);

            MessageBox = content.Load<Texture2D>("messageBox");
            HUD = content.Load<Texture2D>("hud");

            Items = content.LoadTextureSet("items", 32, 32);
            Potions = content.LoadTextureSet("potions");
            Coins = content.LoadTextureSet("coins");
            Chest = content.LoadTextureSet("chest");
            SpellEXP = content.LoadTextureSet("spellexp");
            ShopItems = content.Load<Texture2D>("shopItems");

            // backgrounds

            Backgrounds = content.LoadTextureSet("backgrounds", 16 * Globals.T, 9 * Globals.T);

            // load fonts

            var defaultFont = content.LoadTextureSet("font", 10, 10);
            var messageFont = content.LoadTextureSet("messageFont", 20, 20);
            var damageFont = content.LoadTextureSet("damageFont", 10, 10);
            var hudFont = content.LoadTextureSet("hudFont", 9, 14);
            var hudFontSmall = content.LoadTextureSet("hudFontSmall", 10, 10);

            DefaultFont = new Font(defaultFont, ' ');
            MessageFont = new Font(messageFont, ' ');
            DamageFont = new Font(damageFont, ' ');
            HUDFont = new Font(hudFont, ' ');
            HUDFontSmall = new Font(hudFontSmall, ' ');

            // sounds

            // if a sound fails to load here, build the project with "release", then with "debug" again

            foreach(var prop in typeof(Sounds).GetProperties())
            {
                var lowerFirstString = Char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1);
                Sounds.Load(content.Load<SoundEffect>(lowerFirstString));
            }

            //Sounds.Load(content.Load<SoundEffect>("coin0"));
            //Sounds.Load(content.Load<SoundEffect>("msgChar"));
            //Sounds.Load(content.Load<SoundEffect>("msgChoose"));
            //Sounds.Load(content.Load<SoundEffect>("msgSelectYes"));
            //Sounds.Load(content.Load<SoundEffect>("msgSelectNo"));
            //Sounds.Load(content.Load<SoundEffect>("msgNextPage"));
            //Sounds.Load(content.Load<SoundEffect>("jump"));
            //Sounds.Load(content.Load<SoundEffect>("hurt"));
            //Sounds.Load(content.Load<SoundEffect>("hitGround"));
            //Sounds.Load(content.Load<SoundEffect>("boing"));
            //Sounds.Load(content.Load<SoundEffect>("snap"));
            //Sounds.Load(content.Load<SoundEffect>("letGo"));
            //Sounds.Load(content.Load<SoundEffect>("waterSplash"));
            //Sounds.Load(content.Load<SoundEffect>("waterSwim"));

            // music

        }
    }
}
