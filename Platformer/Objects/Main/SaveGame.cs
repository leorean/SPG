using System;
using Microsoft.Xna.Framework;
using Platformer.Objects;
using Platformer.Objects.Main;
using SPG.Save;

namespace Platformer.Main
{
    [Serializable]
    public class SaveGame : ISaveGame
    {
        private string fileName;
        
        // public, but no getter/setter since it's serializable

        public Vector2 playerPosition;
        public Direction playerDirection;

        public PlayerStats playerStats;

        public int currentBG = -1;

        // methods

        public SaveGame(string fileName)
        {
            this.fileName = fileName;
        }

        public string GetName()
        {
            return fileName;
        }
    }    
}
