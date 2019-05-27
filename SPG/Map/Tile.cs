using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SPG.Map
{    
    public class TileOptions
    {
        //public bool IsAnimated { get { return AnimationLength > 0 && AnimationDuration > 0; } }
        //public int AnimationLength { get; set; }
        //public int AnimationDuration;
        public bool Visible { get; set; } = true;
        public bool Solid { get; set; } = true;
        public float Depth { get; set; } = -1;
    }
    
    /// <summary>
    /// a visual, collidable object at a certain position in a TileMap
    /// </summary>
    public class Tile
    {   
        public int ID { get; private set; }

        private TileOptions tileOptions = new TileOptions();

        /// <summary>
        /// Gets or sets the TileOptions. Can't be set to null (that would simply reset it)
        /// </summary>
        public TileOptions TileOptions {
            get => tileOptions;
            set => tileOptions = value ?? new TileOptions();
        }

        public Tile(int id, TileOptions options = null)
        {
            ID = id;
            TileOptions = options;
        }        
    }    
}