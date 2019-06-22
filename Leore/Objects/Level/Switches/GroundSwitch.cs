using System;
using Microsoft.Xna.Framework;
using Leore.Main;
using SPG.Objects;
using Microsoft.Xna.Framework.Graphics;
using Leore.Objects.Level.Blocks;

namespace Leore.Objects.Level.Switches
{
    public class GroundSwitch : RoomObject
    {
        public bool Active { get; set; }
        private bool activateOnce;
        
        public GroundSwitch(float x, float y, bool activateOnce, Room room) : base(x, y, room)
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
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //base.Draw(sb, gameTime);

            if (Texture == null)
                return;

            int active = (Active || (Room.SwitchState && !activateOnce)) ? 15 : 0;
            sb.Draw(Texture, Position + new Vector2(0, active), new Rectangle(0, active, 16, 16 - active), Color, 0, DrawOffset, Scale, SpriteEffects.None, Depth + .0002f);
        }
    }
}
