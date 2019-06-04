using Microsoft.Xna.Framework;
using Platformer.Objects.Level;
using Platformer.Objects.Main;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Enemies
{
    public class EnemyVoidling : Enemy
    {

        public enum State
        {
            IDLE,
            WALK,
            JUMP
        }

        private State state;

        private int idleTimer;
        private int walkTimer;

        public EnemyVoidling(float x, float y, Room room) : base(x, y, room)
        {
            HP = 20;
            Damage = 3;
            EXP = 20;

            BoundingBox = new SPG.Util.RectF(-4, -4, 8, 12);
            DrawOffset = new Vector2(8);
            
            AnimationTexture = AssetManager.EnemyVoidling;
            Direction = Direction.LEFT;

            Gravity = .1f;

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var onWall = !hit && ObjectManager.CollisionPointFirstOrDefault<Solid>(this, X + (.5f * BoundingBox.Width + 1) * Math.Sign((int)Direction), Y) != null;

            var onGround = this.MoveAdvanced(false);

            if (onGround)
            {
                if (hit)
                {
                    XVel = -1 * Math.Sign(GameManager.Current.Player.X - X);
                    YVel = -2;
                    Direction = (Direction)Math.Sign(GameManager.Current.Player.X - X);
                    walkTimer = 120;
                    state = State.JUMP;
                }
            }

            switch (state)
            {
                case State.IDLE:

                    XVel = 0;

                    idleTimer = Math.Max(idleTimer - 1, 0);
                    if (idleTimer == 0)
                    {
                        Direction = RND.Choose(Direction.LEFT, Direction.RIGHT);
                        walkTimer = 60 + RND.Int(120);
                        state = State.WALK;
                    }                    
                    break;
                case State.WALK:

                    if (onWall)
                        Direction = Direction.Reverse();

                    walkTimer = Math.Max(walkTimer - 1, 0);
                    if (walkTimer == 0)
                    {
                        idleTimer = 60 + RND.Int(120);
                        state = State.IDLE;
                    }                    
                    XVel = XVel + .05f * Math.Sign((int)Direction);
                    XVel = Math.Sign(XVel) * Math.Min(Math.Abs(XVel), .5f);
                    break;
                case State.JUMP:
                    break;
                default:
                    break;
            }
            
            // ++++ draw <-> state logic ++++

            var cols = 4; // how many columns there are in the sheet
            var row = 0; // which row in the sheet
            var fSpd = 0f; // frame speed
            var fAmount = 4; // how many frames
            var loopAnim = true; // loop animation?
            
            switch (state)
            {
                case State.IDLE:
                    row = 0;
                    fSpd = 0.07f;
                    break;
                case State.WALK:
                    row = 1;
                    fSpd = 0.15f;
                    break;
                case State.JUMP:
                    row = 2;
                    fSpd = 0;
                    loopAnim = false;
                    break;
                default:
                    break;
            }

            SetAnimation(cols * row, cols * row + fAmount - 1, fSpd, loopAnim);            
            var xScale = Math.Sign((int)Direction);
            Scale = new Vector2(xScale, 1);
        }
    }
}
