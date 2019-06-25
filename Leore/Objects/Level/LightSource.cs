using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Objects.Level
{
    public class LightObject : RoomObject
    {
        private LightSource light;

        public LightObject(float x, float y, Room room) : base(x, y, room)
        {
            light = new LightSource(this);
            light.State = LightSource.LightState.Default;
            light.Active = true;
        }
    }

    public class AmbientLightSource : RoomObject
    {
        private LightSource light;

        public AmbientLightSource(float x, float y, Room room) : base(x, y, room)
        {
            light = new LightSource(this);
            light.State = LightSource.LightState.Ambient;
        }
    }

    public class LightSource : GameObject
    {
        public enum LightState
        {
            Default, // <- doesn't change lightness in room
            Bright, // <- adds to the lightness in the room (1 bright = .15 light)
            Ambient, // <- adds to the lightness in the room regardless of active/inactive (1 ambient = .3 light)
            FullRoom // <- if active, full room is bright (1 fullRoom = 1 light)
        }

        public bool Active { get; set; }
        public LightState State { get; set; } = LightState.Default;

        public LightSource(GameObject parent) : base(parent.Center.X, parent.Center.Y)
        {
            this.Parent = parent;
        }

        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);            
        }        
    }
}
