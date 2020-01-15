﻿using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Main
{
    public struct ID : IID {
        float x;
        float y;
        float mapIndex;

        public ID(float x, float y, float mapIndex)
        {
            this.x = x;
            this.y = y;
            this.mapIndex = mapIndex;
        }

        public static bool operator ==(ID obj1, ID obj2) => obj1.IDEquals(obj2);
        public static bool operator !=(ID obj1, ID obj2) => !obj1.IDEquals(obj2);

        public bool IDEquals(IID other)
        {
            if (other.GetType() != GetType())
                return false;

            var o = (ID)other;

            return o.x == x && o.y == y & o.mapIndex == mapIndex;
        }

        public static ID Unknown { get; } = new ID(-1, -1, -1);
    }

    public class MapCoherentGameObject : GameObject
    {
        public ID ID { get; set; }

        public MapCoherentGameObject(float x, float y, int mapIndex, string name = null) : base(x, y, name)
        {
            ID = new ID(x, y, mapIndex);
        }

        public override bool IDEquals(IID other)
        {
            return ID.Equals(other);
        }
    }
}
