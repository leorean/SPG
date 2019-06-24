using Leore.Main;
using Leore.Objects.Projectiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPG.Objects;
using Leore.Objects.Effects.Emitters;

namespace Leore.Objects.Level
{
    public class Torch : RoomObject
    {
        public bool Active { get; set; }

        private bool isBright;

        protected LightSource light;
        protected TorchEmitter emitter;

        public Torch(float x, float y, Room room, bool active, LightSource.LightState lightState) : base(x, y, room)
        {
            Depth = Globals.LAYER_BG + .002f;

            AnimationTexture = AssetManager.Torch;

            this.Active = active;
            light = new LightSource(this);
            light.State = lightState;

            emitter = new TorchEmitter(X + 8, Y + 8);
            emitter.Parent = this;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.CollisionBoundsFirstOrDefault<PlayerProjectile>(X, Y) != null)
            {
                Active = true;
            }

            if (!Active)
            {
                SetAnimation(0, 1, 0, false);
            }
            if (Active)
            {
                SetAnimation(1, 4, .2f, true);
            }

            light.Active = Active;
            emitter.Active = Active;
        }
    }
}
