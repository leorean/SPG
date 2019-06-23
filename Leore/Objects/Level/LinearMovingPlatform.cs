using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Main;
using System;

namespace Leore.Objects.Level
{
    public class LinearMovingPlatform : MovingPlatform
    {

        public bool Active { get; set; } = true;
        public bool Activatable { get; set; } = false;
        
        private int xRange;
        private int yRange;
        private float xv;
        private float yv;

        private float xo;
        private float yo;

        private bool movingX;
        private bool movingY;

        private int xSign;
        private int ySign;

        private int moveTimeout;
        private int maxMoveTimeout = 30;
        
        public LinearMovingPlatform(float x, float y, float xVel, float yVel, int xRange, int yRange, bool activatable, int timeout, Room room) : base(x, y, room)
        {
            BoundingBox = new SPG.Util.RectF(0, 0, 32, 1);
            DrawOffset = new Vector2(0, 16);
            Visible = true;
            
            AnimationTexture = AssetManager.MovingPlatform;

            Activatable = activatable;

            this.xv = Math.Abs(xVel);
            this.yv = Math.Abs(yVel);
            this.xRange = xRange * Globals.T;
            this.yRange = yRange * Globals.T;

            xSign = Math.Sign(xVel);
            ySign = Math.Sign(yVel);

            xo = X;
            yo = Y;

            if (Activatable)
                Active = false;

            if (timeout != -1)
                maxMoveTimeout = timeout;

            moveTimeout = maxMoveTimeout;
        }

        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);

            if (Activatable)
                Active = Room.SwitchState;

            //Texture = Active? AssetManager.MovingPlatform[1] : Texture = AssetManager.MovingPlatform[0];

            if (Active)
            {
                SetAnimation(1, 3, .3f, true);
            }
            else
            {
                SetAnimation(1, 1, 0, false);
                XVel = 0;
                YVel = 0;                
                return;
            }

            if (!movingX)
            {
                xo = X;
                XVel = 0;
                if (moveTimeout == 0)
                    movingX = true;
            }
            else
                XVel = xSign * xv;

            if (!movingY)
            {
                yo = Y;
                YVel = 0;
                if (moveTimeout == 0)
                    movingY = true;
            }
            else
                YVel = ySign * yv;

            // only happens when they reach their goal at the same time
            if (!movingX && !movingY)
            {
                moveTimeout = Math.Max(moveTimeout - 1, 0);
                if (moveTimeout == 0)
                {
                    movingX = true;
                    movingY = true;
                }
            }

            if (movingX)
            {
                if (Math.Abs(X - xo) >= xRange)
                {
                    xSign *= -1;
                    moveTimeout = maxMoveTimeout;
                    movingX = false;
                }
                else
                    Move(XVel, 0);
            }
            if (movingY)
            {
                if (Math.Abs(Y - yo) >= yRange)
                {
                    ySign *= -1;
                    moveTimeout = maxMoveTimeout;
                    movingY = false;
                }
                else
                    Move(0, YVel);
            }            
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            if (Texture != null)
            {
                sb.Draw(Texture, Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Globals.LAYER_BG + 0.0001f);
                sb.Draw(AnimationTexture[0], Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Globals.LAYER_FG - 0.0001f);
                sb.Draw(AnimationTexture[0], Position, null, new Color(Color, 0.5f), Angle, DrawOffset, Scale, SpriteEffects.None, Globals.LAYER_FG + 0.0001f);
            }

        }
    }
}
