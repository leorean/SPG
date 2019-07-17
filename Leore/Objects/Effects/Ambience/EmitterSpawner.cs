using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Objects.Effects.Ambience
{
    public class EmitterSpawner<T> : RoomObject where T : ParticleEmitter
    {
        private T emitter;
        public EmitterSpawner(float x, float y, Room room) : base(x, y, room)
        {
            emitter = (T)Activator.CreateInstance(typeof(T), x, y);
            emitter.Parent = this;
        }
    }
}
