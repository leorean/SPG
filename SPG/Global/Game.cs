﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Map;
using SPG.Objects;
using System;

namespace SPG
{
    public abstract class Game : Microsoft.Xna.Framework.Game
    {
        public abstract GraphicsDeviceManager GraphicsDeviceManager { get; }
        public abstract SpriteBatch SpriteBatch { get; }
        
        public GameMap Map { get; protected set; }
    }
}