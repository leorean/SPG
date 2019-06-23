using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Objects.Effects.Emitters;
using SPG.Util;
using System;

namespace Leore.Objects.Level
{
    public class Mushroom : RoomObject
    {
        private float scale = 1f;
        bool bounced;

        public bool Bouncing { get; private set; }

        public Mushroom(float x, float y, Room room) : base(x, y, room)
        {
            BoundingBox = new RectF(0, 8, Globals.T, 8);            
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

            if (bounced)
            {
                scale = .3f;
                bounced = false;
            }

            scale = Math.Min(scale + .05f, 1);
            Scale = new Vector2(1f, scale);


            Bouncing = (scale < 1f);
            
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            var posy = Y + (1 - scale) * Globals.T;
            var pos = new Vector2(X, posy);

            sb.Draw(Texture, pos, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
        }
    }
}
