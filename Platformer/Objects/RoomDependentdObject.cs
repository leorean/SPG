using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects
{
    public abstract class RoomDependentdObject : GameObject
    {
        public Room Room { get; set; }
    }
}
