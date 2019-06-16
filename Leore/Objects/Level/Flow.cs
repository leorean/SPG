using Leore.Objects.Effects.Emitters;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Objects.Level
{
    public class Flow : RoomObject
    {
        private FlowEmitter emitter;
        public Direction Direction { get; private set; }

        public bool Activatable { get; set; }
        public bool Active { get; private set; }

        public Flow(float x, float y, Room room, Direction direction) : base(x, y, room)
        {
            Direction = direction;
            emitter = new FlowEmitter(x, y, direction);
            emitter.Parent = this;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Active = true;
            if (Activatable)
                Active = Room.SwitchState;

            emitter.Active = Active;
        }
    }
}
