using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SPG.Objects;
using SPG.Util;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Platformer
{
    public class Player : GameObject
    {

        public enum PlayerState
        {
            IDLE,
            WALK,
            JUMP
        }

        private int _framesPerRow = 8;

        public PlayerState State { get; set; }

        public Player(int x, int y)
        {
            Name = "Player";
            Position = new Vector2(x, y);
            
            DrawOffset = new Vector2(8, 24);
            BoundingBox = new RectF(-4, -4, 8, 12);
            Depth = Globals.LAYER_FG + 0.0010f;
            State = PlayerState.IDLE;
            Gravity = .1f;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Input

            var keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.Up))
            {
                YVel = -2f;
                State = PlayerState.JUMP;
            }
            if (keyboard.IsKeyDown(Keys.Left))
            {
                XVel = -1f;
                State = PlayerState.WALK;
            }
            if (keyboard.IsKeyDown(Keys.Right))
            {
                XVel = 1f;
                State = PlayerState.WALK;
            }
            
            // collision & movement

            YVel += Gravity;

            var colY = ObjectManager.Find(this, X, Y + YVel, typeof(Solid));
            if (colY.Count == 0)
            {
                Move(0, YVel);
            }
            else
            {
                if(YVel > Gravity)
                    State = PlayerState.IDLE;
                
                var b = colY.Min(x => x.Y);
                var bottom = colY.Where(o => o.Y == b).First();

                var newY = bottom.Y + BoundingBox.Y + BoundingBox.Height - bottom.BoundingBox.Height - Gravity;

                Position = new Vector2(X, newY);
                YVel = 0;
            }
            

            var colX = ObjectManager.Find(this, X + XVel, Y, typeof(Solid));

            if (colX.Count == 0)
            {
                Move(XVel, 0);
            } else
            {
                XVel = 0;
            }

            // draw <-> state logic

            switch (State)
            {
                case PlayerState.IDLE:
                    SetAnimation(0, 3, 0.03, true);
                    break;
                case PlayerState.WALK:
                    SetAnimation(6, 9, 0.1, true);
                    break;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);            
        }
    }
}