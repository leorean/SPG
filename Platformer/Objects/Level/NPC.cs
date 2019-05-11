using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Platformer.Objects.Main;
using SPG.Util;

namespace Platformer.Objects.Level
{
    public class NPC : RoomObject
    {
        private string text;
        private int type;

        private Direction direction;

        private Player p;

        public NPC(float x, float y, Room room, int type, string text, string name = null) : base(x, y, room, name)
        {
            this.text = text;
            this.type = type;

            AnimationTexture = AssetManager.NPCS;

            DrawOffset = new Vector2(8, 24);
            BoundingBox = new RectF(-4, -4, 8, 12);
            Depth = Globals.LAYER_PLAYER - 0.001f;
            
            direction = Direction.RIGHT;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (p != null)
            {
                p.Direction = (Direction)(-(int)direction);
                var tx = (X - p.X + Math.Sign((int)direction) * 8) / 20f;
                p.XVel = tx;

                if (Math.Abs(X - p.X) < 1f)
                    p.XVel = -1;
            }

            // ++++ draw <-> state logic ++++

            var cols = 4; // how many columns there are in the sheet            
            var fSpd = .04f; // frame speed
            var fAmount = 2; // how many frames
            var loopAnim = true; // loop animation?
            
            SetAnimation(cols * type, cols * type + fAmount - 1, fSpd, loopAnim);

            if (GameManager.Current.Player.X < X)
            {
                direction = Direction.LEFT;
            } else
            {
                direction = Direction.RIGHT;
            }

            var xScale = Math.Sign((int)direction);
            Scale = new Vector2(xScale, 1);
        }

        public virtual void Interact(Player player)
        {
            p = player;

            p.State = Player.PlayerState.DOOR;

            var msgBox = new MessageBox(text, "");
            msgBox.OnCompleted = () =>
            {
                p.State = Player.PlayerState.IDLE;                
                p = null;
            };
        }
    }
}
