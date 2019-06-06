using Platformer.Objects.Level;
using SPG.Objects;

namespace Platformer.Objects
{
    public interface IMovable : ICollidable
    {
        Collider MovingPlatform { get; set; }
    }
}
