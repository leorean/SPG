﻿using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Enemy
{
    public abstract class Obstacle : RoomDependentdObject
    {
        public int Damage { get; protected set; } = 1;
    }

    public class SpikeBottom : Obstacle
    {
        public SpikeBottom(float x, float y, Room room)
        {
            Name = "SpikeBottom";
            Position = new Vector2(x, y);
            Room = room;

            BoundingBox = new RectF(0, 8, 16, 8);
            Depth = Globals.LAYER_FG;// + 0.0010f;
            //DebugEnabled = true;
        }
    }
}