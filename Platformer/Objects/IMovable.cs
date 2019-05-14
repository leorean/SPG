using Platformer.Objects.Level;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects
{
    public interface IMovable : ICollidable
    {
        Collider MovingPlatform { get; set; }
    }
}
