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

        public GroundSwitch(float x, float y, Room room, string name = null) : base(x, y, room, name)
        {
            BoundingBox = new SPG.Util.RectF(2, 13, 12, 3);            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.CollisionBounds(GameManager.Current.Player, X, Y)
                || this.CollisionBounds<PushBlock>(X, Y).Count > 0)
            {
                Active = true;
            }
            else
                Active = false;

            if (!Active)
                Texture = AssetManager.GroundSwitch[0];
            else
                Texture = AssetManager.GroundSwitch[1];
        }
    }
}
