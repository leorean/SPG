using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Objects.Effects.Emitters;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Level
{
    public class Mushroom : RoomObject
    {
        private float scale = 1f;
        bool bounced;

        public bool Bouncing { get; private set; }

        public Mushroom(float x, float y, Room room) : base(x, y, room)
        {
            BoundingBox = new RectF(0, 8, Globals.TILE, 8);            
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
            var posy = Y + (1 - scale) * Globals.TILE;
            var pos = new Vector2(X, posy);

            sb.Draw(Texture, pos, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
        }
    }
}
