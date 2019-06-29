using Leore.Main;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Enemies;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Objects.Level.Obstacles
{
    public class Lava : Obstacle
    {
        public Lava(float x, float y, Room room) : base(x, y, room)
        {
            BoundingBox = new SPG.Util.RectF(0, 4, 16, 12);
            
            Depth = Globals.LAYER_FG + .00001f;

            AnimationTexture = AssetManager.Lava;

            SetAnimation(0, 5, .15f, true);

            TorchEmitter emitter = new TorchEmitter(Center.X, Center.Y - 4);
            emitter.XRange = 16;
            emitter.YRange = 0;
            emitter.Parent = this;
            emitter.Depth = Depth - .0001f;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);            
        }
    }
}
