﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Leore.Objects;
using SPG.Save;

namespace Leore.Main
{
    [Serializable]
    public class GameStats
    {
        public int MaxHP { get; set; } = 5;
        public int MaxMP { get; set; } = 30;
        public float MPRegen { get; set; } = .1f;

        public PlayerAbility Abilities { get; set; } = PlayerAbility.NONE;

        public float Coins { get; set; } = 0;

        public Dictionary<SpellType, SpellLevel> Spells { get; set; } = new Dictionary<SpellType, SpellLevel> { { SpellType.NONE, SpellLevel.ONE } };
        public Dictionary<SpellType, int> SpellEXP { get; set; } = new Dictionary<SpellType, int> { { SpellType.NONE, 0 } };
        public int SpellIndex;

        // ID, Typename
        public Dictionary<int, string> Items { get; set; } = new Dictionary<int, string>();
        public List<int> KeysAndKeyblocks { get; set; } = new List<int>();

        // ID, x, y
        public Dictionary<int, Point> Teleporters { get; set; } = new Dictionary<int, Point>();

        public List<int> Bosses { get; set; } = new List<int>();
    }

    [Serializable]
    public class SaveGame : ISaveGame
    {
        public string FileName { get; }
        
        // public, but no getter/setter since it's serializable

        public Vector2 playerPosition;
        public Direction playerDirection;

        public GameStats gameStats;
        
        //public List<int> items;

        public int currentBG = -1;
        
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