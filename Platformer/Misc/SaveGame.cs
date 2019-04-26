using System;
using Microsoft.Xna.Framework;
using SPG.Save;

namespace Platformer.Misc
{
    [Serializable]
    public class SaveGame : ISaveGame
    {
        private string fileName;
        
        public Vector2 playerPosition;
        public Direction playerDirection;

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
