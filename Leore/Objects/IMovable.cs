using Leore.Objects.Level;
using SPG.Objects;

namespace Leore.Objects
{
    public interface IMovable : ICollidable
    {
        Collider MovingPlatform { get; set; }
    }
}
