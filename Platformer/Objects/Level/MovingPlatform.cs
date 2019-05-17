using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Level
{
    public abstract class MovingPlatform : Platform
    {
        public MovingPlatform(float x, float y, Room room) : base(x, y, room)
        {
        }
    }
}
