using Microsoft.Xna.Framework;
using Platformer.Objects.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Level
{
    public class MovingPlatform : Platform
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

        public MovingPlatform(float x, float y, float xVel, float yVel, int xRange, int yRange, bool activatable, Room room) : base(x, y, room)
        {
            BoundingBox = new SPG.Util.RectF(0, 0, 2 * Globals.TILE, 1);
            Visible = true;
            Depth = Globals.LAYER_PLAYER - 0.0001f;            
            Texture = AssetManager.MovingPlatform;

            Activatable = activatable;

            this.xv = xVel;
            this.yv = yVel;
            this.xRange = xRange * Globals.TILE;
            this.yRange = yRange * Globals.TILE;

            xSign = Math.Sign(xVel);
            ySign = Math.Sign(yVel);

            xo = X;
            yo = Y;

            if (Activatable)
                Active = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Activatable)
                Active = Room.SwitchState;

            if (!Active)
            {
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
    }
}
