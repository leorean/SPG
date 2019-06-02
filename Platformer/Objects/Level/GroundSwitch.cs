using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Platformer.Objects.Main;
using SPG.Objects;

namespace Platformer.Objects.Level
{
    public class GroundSwitch : RoomObject
    {
        public bool Active { get; set; }

        private bool activateOnce;

        public GroundSwitch(float x, float y, bool activateOnce, Room room, string name = null) : base(x, y, room, name)
        {
            BoundingBox = new SPG.Util.RectF(2, 13, 12, 3);

            this.activateOnce = activateOnce;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.CollisionBounds(GameManager.Current.Player, X, Y)
                || this.CollisionBounds<PushBlock>(X, Y).Count > 0)
            {
                Active = true;
            }
            else if (!activateOnce)
                Active = false;

            var frame = 1 * Convert.ToInt32(Active || Room.SwitchState) + 2 * Convert.ToInt32(activateOnce);

            Texture = AssetManager.GroundSwitch[frame];
            
        }
    }
}
