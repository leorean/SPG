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
    public class LittleWorm : RoomObject
    {
        private Player player => GameManager.Current.Player;
        
        enum State { CRAWL, IDLE }

        State state;

        Vector2 origPosition;
        private Direction direction;
        float spd;
        int stateTimer;

        public LittleWorm(float x, float y, Room room) : base(x, y, room)
        {
            AnimationTexture = AssetManager.LittleStuff;
            BoundingBox = new RectF(-2, 10, 4, 6.5f);
            Depth = Globals.LAYER_PLAYER + .0001f;

            DrawOffset = new Vector2(8, 0);

            //DebugEnabled = true;

            origPosition = Position;
            direction = RND.Choose(Direction.LEFT, Direction.RIGHT);

            spd = GetSpd();
        }

        float GetSpd()
        {
            return RND.Choose(.1f, .2f, .25f);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            stateTimer = Math.Max(stateTimer - 1, 0);

            if(stateTimer == 0)
            {
                stateTimer = RND.Choose(20, 40, 60, 80);
                state = RND.Choose(State.IDLE, State.CRAWL);
            }

            switch (state)
            {
                case State.CRAWL:

                    XVel = (int)direction * spd;

                    if (Math.Abs(origPosition.X - X) > 6)
                    {
                        direction = (Direction)Math.Sign(origPosition.X - X);
                        spd = GetSpd();
                    }

                    SetAnimation(13, 16, .5f * spd, true);
                    
                    break;
                case State.IDLE:

                    XVel = 0;
                    SetAnimation(17, 17, 0, false);

                    break;
            }
            
            Scale = new Vector2((int)direction, 1);

            Move(XVel, YVel);

            if (this.IsOutsideCurrentRoom())
                Destroy();
        }
    }
}
