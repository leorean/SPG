using Leore.Main;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Objects.Level
{
    public class Torch : RoomObject
    {
        public bool Active { get; set; }

        private LightSource light;

        public Torch(float x, float y, Room room, bool active) : base(x, y, room)
        {
            Depth = Globals.LAYER_BG + .002f;

            AnimationTexture = AssetManager.Torch;

            this.Active = active;
            light = new LightSource(this);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!Active)
            {
                SetAnimation(0, 1, 0, false);
            }
            if (Active)
            {
                SetAnimation(1, 4, .2f, true);
            }

            light.Active = Active;
        }
    }
}
