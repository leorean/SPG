using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Leore.Objects.Level;
using Leore.Objects.Level.Obstacles;
using Leore.Resources;
using Microsoft.Xna.Framework;
using SPG.Map;
using SPG.Objects;
using SPG.Util;

namespace Leore.Objects.Enemies
{
    public class EnemyDash : Enemy
    {
        public enum DashDirection { HORIZONTAL, VERTICAL }
        public enum State { IDLE, DASH }

        private DashDirection dashDirection;
        private State state;

        public EnemyDash(float x, float y, Room room, DashDirection dashDirection) : base(x, y, room)
        {
            knockback = 0;

            AnimationTexture = AssetManager.EnemyDash;

            HP = GameResources.EnemyDash.HP;
            Damage = GameResources.EnemyDash.Damage;
            EXP = GameResources.EnemyDash.EXP;

            this.dashDirection = dashDirection;
            state = State.IDLE;

            DebugEnabled = true;
            DrawOffset = new Vector2(16);
            BoundingBox = new RectF(-6, -6, 12, 12);
            Visible = true;

            switch (dashDirection)
            {
                case DashDirection.VERTICAL:
                    Direction = (this.CollisionBoundsFirstOrDefault<Collider>(X, Y - 8) != null) ? Direction.DOWN : Direction.UP;
                    break;
                case DashDirection.HORIZONTAL:
                    Direction = (this.CollisionBoundsFirstOrDefault<Collider>(X - 8, Y) != null) ? Direction.RIGHT : Direction.LEFT;
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            switch (state)
            {

                case State.IDLE:
                    
                    SetAnimation(0, 0, 0, false);
                    XVel = 0;
                    YVel = 0;

                    var castAng = 0;

                    switch (Direction)
                    {
                        case Direction.DOWN: castAng = 90; break;
                        case Direction.UP: castAng = 270; break;
                        case Direction.LEFT: castAng = 180; break;
                        case Direction.RIGHT: castAng = 0; break;
                    }

                    var rayCastSolid = this.RayCast<Solid>(castAng, 3, 8 * Globals.T);
                    var rayCastPlayer = this.RayCast<Player>(castAng, 3, 8 * Globals.T);

                    if (rayCastPlayer.Item1 != null && (rayCastSolid.Item1 == null || rayCastSolid.Item2 > rayCastPlayer.Item2))
                    {
                        state = State.DASH;
                        //Debug.WriteLine(rayCastPlayer.Item2);
                    }


            break;
                case State.DASH:
                    SetAnimation(1, 1, 0, false);

                    var maxVel = 3f;
                    var acc = .15f;

                    switch (Direction)
                    {
                        case Direction.DOWN:
                            YVel = MathUtil.AtMost(YVel + acc, maxVel);                            
                            break;
                        case Direction.UP:
                            YVel = MathUtil.AtMost(YVel - acc, maxVel);
                            break;
                        case Direction.LEFT:
                            XVel = MathUtil.AtMost(XVel - acc, maxVel);
                            break;
                        case Direction.RIGHT:
                            XVel = MathUtil.AtMost(XVel + acc, maxVel);
                            break;
                    }

                    if (this.CollisionBoundsFirstOrDefault<Collider>(X + XVel, Y + YVel) != null)
                    {
                        XVel = 0;
                        YVel = 0;
                        float tx = Globals.T * MathUtil.Div(Center.X, Globals.T) + 8;
                        float ty = Globals.T * MathUtil.Div(Center.Y, Globals.T) + 8;

                        Position = new Vector2(tx, ty);
                        state = State.IDLE;
                        Direction = Direction.Reverse();
                    }

                    break;
            }

            Move(XVel, YVel);

            switch (Direction)
            {
                case Direction.DOWN:
                    Angle = (float)MathUtil.DegToRad(90);
                    break;
                case Direction.UP:
                    Angle = (float)MathUtil.DegToRad(270);
                    break;
                case Direction.RIGHT:
                    Angle = (float)MathUtil.DegToRad(0);
                    break;
                case Direction.LEFT:
                    Angle = (float)MathUtil.DegToRad(180);
                    break;
            }

            if (this.IsOutsideCurrentRoom(2 * Globals.T))
                Destroy();
        }

        public override void Hit(int hitPoints, float degAngle)
        {
            base.Hit(hitPoints, degAngle);

        }
    }
}
