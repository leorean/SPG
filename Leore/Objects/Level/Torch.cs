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
using Leore.Objects.Effects;
using Leore.Resources;

namespace Leore.Objects.Level
{
    public class Torch : RoomObject
    {
        public bool Active { get; set; }
        
        protected LightSource light;
        protected TorchEmitter emitter;

        public bool TriggerSwitch { get; private set; }

        public Torch(float x, float y, Room room, bool active, LightSource.LightState lightState, bool triggerSwitch) : base(x, y, room)
        {
            Depth = Globals.LAYER_BG + .002f;

            AnimationTexture = AssetManager.Torch;

            this.TriggerSwitch = triggerSwitch;

            this.Active = active;
            light = new LightSource(this);
            light.State = lightState;

            emitter = new TorchEmitter(X + 8, Y + 6);
            emitter.Parent = this;
            emitter.Depth = Depth;

            if (TriggerSwitch)
                emitter.ParticleColors = GameResources.MpColors;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var projectile = this.CollisionBoundsFirstOrDefault<PlayerProjectile>(X, Y);

            if (projectile != null)
            {
                if (projectile.Element == SpellElement.FIRE)
                {
                    if (!Active)
                        new SingularEffect(Center.X, Center.Y, 7);
                    Active = true;
                }

                if (projectile.Element == SpellElement.ICE 
                    || projectile.Element == SpellElement.DARK
                    || projectile.Element == SpellElement.WIND)
                {
                    if (Active)
                        new SingularEffect(Center.X, Center.Y, 7);

                    Active = false;
                }
            }

            int off = !TriggerSwitch ? 0 : 5;

            if (!Active)
            {
                SetAnimation(0 + off, 1 + off, 0, false);
            }
            if (Active)
            {
                SetAnimation(1 + off, 4 + off, .2f, true);
            }

            light.Active = Active;
            emitter.Active = Active;
        }
    }
}
