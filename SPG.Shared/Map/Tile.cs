using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SPG.Map
{    
    public class TileOptions
    {
        public bool IsAnimated { get { return AnimationLength > 0 && AnimationDuration > 0; } }
        public int AnimationLength { get; set; }
        public int AnimationDuration;
    }

    public enum TileType
    {
        Block = 0,
        Platform = 1,
        NoBlock = 2

    }
    // an object at a certain position in a tilemap
    public class Tile
    {
        
        public int ID { get; private set; }
        public TileType Type { get; private set; }
        public TileOptions TileOptions { get; private set; }

        public Tile(int id, TileType type, TileOptions options = null)
        {
            ID = id;
            Type = type;
            TileOptions = options;
        }
    }    
}