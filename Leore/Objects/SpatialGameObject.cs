using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Main
{
    [Serializable]
    public struct ID : IID {
        public float x;
        public float y;
        public int mapIndex;
        
        public ID(float x, float y, int mapIndex)
        {
            this.x = x;
            this.y = y;
            this.mapIndex = mapIndex;
        }

        public override string ToString()
        {
            return $"[{x}, {y}, {mapIndex}]";
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

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    
    public class SpatialGameObject : GameObject
    {
        public ID ID { get; set; }

        public SpatialGameObject(float x, float y, int mapIndex, string name = null) : base(x, y, name)
        {
            ID = new ID(x, y, mapIndex);
        }

        public override bool IDEquals(IID other)
        {
            return ID.Equals(other);
        }
    }
}
