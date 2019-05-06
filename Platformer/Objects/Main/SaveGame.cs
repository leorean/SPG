using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Platformer.Objects;
using Platformer.Objects.Items;
using Platformer.Objects.Main;
using SPG.Save;

namespace Platformer.Main
{
    [Serializable]
    public class SaveGame : ISaveGame
    {
        public string FileName { get; }
        
        // public, but no getter/setter since it's serializable

        public Vector2 playerPosition;
        public Direction playerDirection;

        public GameStats gameStats;
        
        public List<int> items;

        public int currentBG = -1;
        
        // methods

        public SaveGame(string fileName)
        {
            FileName = fileName;
            items = new List<int>();
            gameStats = new GameStats();
        }

        public string GetName()
        {
            return FileName;
        }
    }    
}
