using Leore.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Objects.Effects.Ambience
{
    public class AnimatedWaterSurface : RoomObject
    {
        public AnimatedWaterSurface(float x, float y, Room room, int type) : base(x, y, room)
        {
            Depth = Globals.LAYER_WATER;
            AnimationTexture = AssetManager.WaterSurface;
            SetAnimation(6 * type, 6 * type + 5, .1f, true);            
        }
    }
}
