using Microsoft.Xna.Framework;
using Platformer.Main;
using Platformer.Objects.Effects.Emitters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Level
{
    public class Teleporter : RoomObject
    {
        public bool Active { get; set; }

        private ObtainParticleEmitter emitter;

        public Teleporter(float x, float y, Room room) : base(x, y, room)
        {
            AnimationTexture = AssetManager.Teleporter;

            Depth = Globals.LAYER_FG - 0.0001f;

            DrawOffset = new Vector2(32);
            BoundingBox = new SPG.Util.RectF(-32, -32, 64, 64);

            emitter = new ObtainParticleEmitter(X, Y, 0);
            emitter.Parent = this;
            emitter.Active = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            emitter.Active = Active;

            if (Active)
            {
                SetAnimation(0, 3, .1f, false);

                if (!GameManager.Current.Player.Stats.Teleporters.Contains(ID))
                    GameManager.Current.Player.Stats.Teleporters.Add(ID);
            }
            else
            {
                SetAnimation(4, 7, .1f, false);
            }

            //Angle = (Angle + .01f) % (float)(2 * Math.PI);
        }
    }
}
