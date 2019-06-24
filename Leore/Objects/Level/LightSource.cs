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
    public class LightSource : GameObject
    {
        public enum LightState
        {
            Default,
            Bright,
            FullRoom
        }

        public bool Active { get; set; }
        public LightState State { get; set; } = LightState.Default;

        public LightSource(GameObject parent) : base(parent.X, parent.Y)
        {
            this.Parent = parent;
        }

        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);
            //Position = Parent.Position;
        }

        public override void Destroy(bool callGC = false)
        {
            base.Destroy(callGC);
        }
    }
}
