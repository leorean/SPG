using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Objects.Effects.Emitters;
using SPG.Util;
using System;

namespace Leore.Objects.Level
{
    public class RollBouncer : RoomObject
    {
        public Direction Direction { get; set; }
        public Direction VerticalDirection { get; set; }

        private float scale = 1f;
        bool bounced;

        public bool Bouncing { get; private set; }

        RectF bLeft = new RectF(10, 10, 6, 6);
        RectF bRight = new RectF(0, 10, 6, 6);
        RectF bLeftDown = new RectF(10, 0, 6, 6);
        RectF bRightDown = new RectF(0, 0, 6, 6);

        public RollBouncer(float x, float y, Room room) : base(x, y, room)
        {
            Depth = Globals.LAYER_FG;
            //DebugEnabled = true;
        }

        public void Bounce()
        {
            if (!bounced)
                new StarEmitter(Center.X, Center.Y);
            bounced = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (VerticalDirection == Direction.DOWN)
            {
                BoundingBox = Direction == Direction.LEFT ? bLeftDown : bRightDown;
            }
            else
            {
                BoundingBox = Direction == Direction.LEFT ? bLeft : bRight;
            }

            //BoundingBox = new RectF(6, 6, 4, 4);

            if (bounced)
            {
                scale = .5f;
                bounced = false;
            }

            scale = Math.Min(scale + .05f, 1);
            Scale = new Vector2(Math.Min(scale, 1f), scale);


            Bouncing = (scale < 1f);

        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //base.Draw(sb, gameTime);

            float posx, posy;
            if (Direction == Direction.LEFT)
            {
                posx = X + (1 - scale) * Globals.T;
            }
            else
            {
                posx = X;
            }
            if (VerticalDirection == Direction.DOWN)
            {
                posy = Y;
            }
            else
            {
                posy = Y + (1 - scale) * Globals.T;
            }
            
            var pos = new Vector2(posx, posy);

            sb.Draw(Texture, pos, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
        }
    }
}
