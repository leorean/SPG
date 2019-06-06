using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Util;

namespace Platformer.Objects
{
    public class Room : GameObject
    {
        // TODO: refactor/optimize: rooms should have a list of roomObjects.

        public int Background { get; set; } = -1;

        public bool SwitchState { get; set; }

        //public List<ICollidable> Colliders { get; set; } = new List<ICollidable>();

        public Room(int x, int y, int width, int height) : base(x, y, "room")
        {
            BoundingBox = new RectF(0, 0, width, height);            
            Visible = false;            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // switches -> state on/off
            /*
            var switches = ObjectManager.Objects.FindAll(o => o is GroundSwitch).Cast<GroundSwitch>();
            bool found = false;
            foreach(var s in switches)
            {
                if (s.Active)
                {
                    found = true;
                    SwitchState = true;
                    break;
                }
            }
            if (!found)
                SwitchState = false;
            */
        }
    }
}
