using Leore.Main;
using Microsoft.Xna.Framework;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPG.Map;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Items;
using System.Diagnostics;
using Leore.Objects.Effects;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;

namespace Leore.Objects.Enemies
{
    public class BossShadowLizard : Boss
    {
        private enum State
        {
            IDLE,
            FOLLOW_PLAYER,
            ATTACK,
            DIE
        }
        
        private State state;

        private Player player => GameManager.Current.Player;
        
        //private bool preventDeath;
        
        public BossShadowLizard(float x, float y, Room room, string setCondition) : base(x, y, room, setCondition)
        {
            HP = 200;
            
            AnimationTexture = AssetManager.BossShadowLizard;
            DrawOffset = new Vector2(48);
            BoundingBox = new SPG.Util.RectF(-16, -16, 32, 32);

            Depth = Globals.LAYER_BG2 + .0001f;
            
            knockback = .0f;

            Direction = Direction.RIGHT;

            state = State.FOLLOW_PLAYER;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            switch (state)
            {
                case State.IDLE:
                    SetAnimation(0, 3, .1f, true);
                    break;
                case State.FOLLOW_PLAYER:
                    SetAnimation(4, 7, .2f, true);

                    XVel = (player.X - X) / 200;
                    YVel = (player.Y - Y) / 200;

                    XVel = MathUtil.AtMost(XVel, .5f);
                    YVel = MathUtil.AtMost(YVel, .5f);

                    //Angle = (float)MathUtil.VectorToAngle(new Vector2(player.X - X, player.Y - Y), true);
                    Angle = (float)MathUtil.VectorToAngle(new Vector2(XVel, YVel), true);

                    break;
            }
            
            Move(XVel, YVel);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            if (state != State.DIE)
            {
                base.Draw(sb, gameTime);
            }
            else
            {
            //    deathAlpha = Math.Min(deathAlpha + .01f, 1);

            //    sb.Draw(AssetManager.BossGiantBat[8], Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
            //    sb.Draw(AssetManager.BossGiantBat[9], Position, null, new Color(Color, deathAlpha), Angle, DrawOffset, Scale, SpriteEffects.None, Depth + .0001f);
            }
        }

        public override void Hit(int hitPoints, float degAngle)
        {
            base.Hit(hitPoints, degAngle);            
        }

        public override void Destroy(bool callGC = false)
        {
            base.Destroy();
        }

        public override void OnDeath()
        {
            //base.OnDeath();
            base.OnDeath();
            
        }
    }
}
