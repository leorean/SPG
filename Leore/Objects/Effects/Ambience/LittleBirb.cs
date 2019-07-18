using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Microsoft.Xna.Framework;
using SPG.Map;
using SPG.Util;
using SPG.Objects;
using Leore.Objects.Level;

namespace Leore.Objects.Effects.Ambience
{
    public class LittleBirb : RoomObject
    {
        private enum State { PICK, HOP, FLY_AWAY }

        private int stateTimer;

        private State state;

        private Player player => GameManager.Current.Player;

        private int type;

        Vector2 origPosition;
        private Direction direction;

        public LittleBirb(float x, float y, Room room) : base(x, y, room)
        {
            type = RND.Choose(0, 1, 2);

            AnimationTexture = AssetManager.LittleStuff;
            BoundingBox = new RectF(-2, 10, 4, 6.5f);
            Depth = Globals.LAYER_PLAYER + .0001f;

            DrawOffset = new Vector2(8, 0);

            //DebugEnabled = true;

            origPosition = Position;
            direction = RND.Choose(Direction.LEFT, Direction.RIGHT);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            bool onGround = this.CollisionBoundsFirstOrDefault<Collider>(X, Y + YVel) != null;
            
            stateTimer = Math.Max(stateTimer - 1, 0);

            switch (state)
            {
                case State.PICK:
                    XVel = 0;
                    YVel = 0;
                    if (stateTimer == 0)
                    {
                        state = State.HOP;
                        stateTimer = 60 + RND.Int(60);
                    }

                    SetAnimation(1 + type * 4, 2 + type * 4, .05f, true);
                    break;
                case State.HOP:
                    YVel += .1f;
                    if (onGround)
                    {
                        if (stateTimer == 0)
                        {
                            state = State.PICK;
                            stateTimer = 30 + RND.Int(120);
                        } else
                        {
                            XVel = RND.Choose(-.25f, .25f);

                            if (Math.Abs(origPosition.X - X) > 6)
                            {
                                XVel = Math.Sign(origPosition.X - X) * Math.Abs(XVel);
                            }

                            YVel = -.5f;
                        }
                    }
                    
                    SetAnimation(1 + type * 4, 1 + type * 4, 0, false);
                    break;
                case State.FLY_AWAY:

                    YVel = Math.Max(YVel - .005f, -1f);
                    XVel = Math.Sign((int)direction) * Math.Min(Math.Abs(XVel) + .06f, 2);

                    SetAnimation(3 + type * 4, 4 + type * 4, .2f, true);
                    break;
            }

            if (state != State.FLY_AWAY)
            {
                if (Math.Abs(XVel) > 0)
                    direction = (Direction)Math.Sign(XVel);

                if (Math.Abs(player.XVel) > .5f && Math.Abs(player.X - X) < 1.5f * Globals.T && Math.Abs(player.Y - Y) < Globals.T)
                {
                    XVel = 0;
                    YVel = 0;

                    state = State.FLY_AWAY;
                    direction = (Direction)Math.Sign(X - player.X);
                }
            }
            Scale = new Vector2((int)direction, 1);

            Move(XVel, YVel);

            if (this.IsOutsideCurrentRoom())
                Destroy();
        }
    }
}
