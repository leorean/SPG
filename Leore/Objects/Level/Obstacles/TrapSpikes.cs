using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Objects.Level.Obstacles
{

    public interface ITrapSpike
    {
        bool IsOut { get; }
    }

    public class TrapSpike : SpikeBottom, ITrapSpike
    {
        protected int timer;
        protected int maxTimer;

        protected float offset;
        protected int maxOffset = 6;
        protected int change;

        public bool IsOut { get; protected set; }

        public TrapSpike(float x, float y, Room room, bool isOut, int maxTimer) : base(x, y, room)
        {
            Depth = Globals.LAYER_FG - .0001f;
            IsOut = isOut;
            this.maxTimer = maxTimer;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            timer = Math.Max(timer - 1, 0);

            if (timer == 0)
            {
                timer = maxTimer;
                change = IsOut ? 1 : -1;
                offset = IsOut ? 0 : maxOffset;
            }
            
            if (change == -1)
            {
                offset = Math.Max(offset - .5f, 0);
                if (offset == 0)
                {
                    change = 0;
                    IsOut = true;
                }
            }
            if (change == 1)
            {                
                offset = Math.Min(offset + .5f, maxOffset);
                if (offset == maxOffset)
                {   
                    change = 0;
                    IsOut = false;
                }
            }
            
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //base.Draw(sb, gameTime);
            //sb.Draw(Texture, Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
            sb.Draw(Texture, Position + new Vector2(0, (int)offset), new Rectangle(0, 0, 16, 16 - (int)offset), Color, 0, DrawOffset, Scale, SpriteEffects.None, Depth);
        }
    }
}
