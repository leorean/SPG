using Microsoft.Xna.Framework;
using Leore.Main;
using Leore.Objects.Effects.Emitters;
using System.Linq;
using Leore.Resources;
using static Leore.Objects.Effects.Transition;
using SPG.Util;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Leore.Objects.Items
{
    public class StoryItem : AbilityItem
    {
        public StoryItem(float x, float y, Room room, string name = null, string setCondition = null, string appearCondition = null) : base(x, y, room, name, setCondition, appearCondition)
        {
            BoundingBox = new RectF(-4, -4, 8, 8);
            DrawOffset = new Vector2(16, 16);

            flashOnTaken = false;
            idleOnTaken = false; // <- lieing down from transition
            OnObtain = Obtain;

            p0 = Position;

            p1 = new Vector2(-3, -4);
        }

        void Obtain()
        {
            if (player.Orb != null)
            {
                player.Orb.Visible = false;
            }

            var px = MathUtil.Div(X, Globals.T) * Globals.T + 8;
            var py = MathUtil.Div(Y, Globals.T) * Globals.T + 8;

            var pos = new Vector2(px + Tx * Globals.T, py + Ty * Globals.T);
            RoomCamera.Current.ChangeRoomsToPosition(pos, TransitionType.VERY_LONG_LIGHT, GameManager.Current.Player.Direction, null);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            xVel1 += (p0.X - p1.X) / 800f;
            yVel1 += (p0.Y - p1.Y) / 800f;

            p1 = new Vector2(p1.X + xVel1, p1.Y + yVel1);
            
            //t = (t + .1) % 9000000000;

            //t1 = .1f * t;
            //t2 = .3f * t;
            //t3 = .2f * t;

            //sin1 = Math.Sin(t1 % (2 * Math.PI));

        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //base.Draw(sb, gameTime);
            
            sb.Draw(Texture, Position + p1, null, Color, Angle, DrawOffset, new Vector2(.25f), SpriteEffects.None, Depth);
            //sb.Draw(Texture, Position + new Vector2(x2, y3), null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
            //sb.Draw(Texture, Position + new Vector2(x3, y3), null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);

        }

        //public override void Update(GameTime gameTime)
        //{
        //    base.Update(gameTime);

        //    ObtainShineEmitter.Active = true;
        //    ObtainShineEmitter.GlowScale = 1.5f;

        //    obtainParticleEmitter.Active = false;
        //}
    }    
}
