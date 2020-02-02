using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Leore.Objects;
using SPG.Save;

namespace Leore.Main
{
    [Serializable]
    public class GameStats
    {
        public int MaxHP { get; set; } = 3;
        public int MaxMP { get; set; } = 30;
        public float MPRegen { get; set; } = .1f;

        public PlayerAbility Abilities { get; set; } = PlayerAbility.NONE;

        public float Coins { get; set; } = 0;

        public Dictionary<SpellType, SpellLevel> Spells { get; set; } = new Dictionary<SpellType, SpellLevel> { { SpellType.NONE, SpellLevel.ONE } };
        public Dictionary<SpellType, int> SpellEXP { get; set; } = new Dictionary<SpellType, int> { { SpellType.NONE, 0 } };
        public int SpellIndex;

        // ID, Typename
        public Dictionary<ID, string> Items { get; set; } = new Dictionary<ID, string>();
        public List<ID> KeysAndKeyblocks { get; set; } = new List<ID>();

        // ID, x, y
        public List<ID> Teleporters { get; set; } = new List<ID>();

        public List<ID> Bosses { get; set; } = new List<ID>();

        public List<string> StoryFlags { get; set; } = new List<string>();

        public Dictionary<string, int> HeldKeys { get; set; } = new Dictionary<string, int>();

        public Dictionary<string, int> ItemsBought { get; set; } = new Dictionary<string, int>();        

    }

    [Serializable]
    public class SaveGame : ISaveGame
    {
        public string FileName { get; }
        
        // public, but no getter/setter since it's serializable

        public Vector2 playerPosition;
        public string levelName;
        public Direction playerDirection;

        public GameStats gameStats;

        public long playTime;

        public int currentBG = -1;
        public int currentWeather = -1;
        
        // methods

        public SaveGame(string fileName)
        {
            FileName = fileName;
            //items = new List<int>();
            gameStats = new GameStats();
        }

        public string GetName()
        {
            return FileName;
        }
    }    
}
