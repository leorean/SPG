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
        Unknown,
        Solid,
        Platform,
        NonBlockable = 2
    }
    // an object at a certain position in a tilemap
    public class Tile
    {
        
        public int ID { get; private set; }
        public TileType TileType { get; set; }
        public TileOptions TileOptions { get; set; }

        public Tile(int id, TileType type = TileType.Unknown, TileOptions options = null)
        {
            ID = id;
            TileType = type;
            TileOptions = options;
        }
    }    
}