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

        // todo: active/inactive

        private int xRange;
        private int yRange;
        private float xVel;
        private float yVel;

        private float xo;
        private float yo;

        private bool moving;
        private int moveTimeout;
        private int maxMoveTimeout = 30;

        public MovingPlatform(float x, float y, float xVel, float yVel, int xRange, int yRange, Room room) : base(x, y, room)
        {
            BoundingBox = new SPG.Util.RectF(0, 0, 2 * Globals.TILE, 1);
            //DrawOffset = new Vector2(Globals.TILE, 0);
            Visible = true;

            Depth = Globals.LAYER_PLAYER - 0.0001f;
            
            Texture = AssetManager.MovingPlatform;

            this.xVel = xVel;
            this.yVel = yVel;
            this.xRange = xRange * Globals.TILE;
            this.yRange = yRange * Globals.TILE;

            xo = X;
            yo = Y;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!moving)
            {
                XVel = 0;
                YVel = 0;

                moveTimeout = Math.Max(moveTimeout - 1, 0);
                if (moveTimeout == 0)
                {
                    moving = true;
                }
            }

            if (moving) {
                Move(XVel, YVel);

                XVel = xVel;
                YVel = yVel;

                if (Math.Abs(X - xo) > xRange)
                {
                    xo = X;
                    xVel = -xVel;
                    moveTimeout = maxMoveTimeout;
                    moving = false;
                }

                if (Math.Abs(Y - yo) > yRange)
                {
                    yo = Y;
                    yVel = -yVel;
                    moveTimeout = maxMoveTimeout;
                    moving = false;
                }
            }

            
        }
    }
}
