using Microsoft.Xna.Framework;
using Platformer.Objects.Main;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPG.Map;

namespace Platformer.Objects.Enemy
{
    class EnemyGrassy : Enemy
    {
        public enum State
        {
            HIDING,
            FOLLOW_PLAYER
        }

        private State state;

        private bool seenPlayer;
        private bool stuck = true;

        public EnemyGrassy(float x, float y, Room room) : base(x, y, room)
        {
            BoundingBox = new SPG.Util.RectF(-4, -4, 8, 12);
            DrawOffset = new Vector2(8);
            
            //DebugEnabled = true;

            AnimationTexture = AssetManager.EnemyGrassy;
            Direction = Direction.RIGHT;

            HP = 20;
            Damage = 2;

            Gravity = .1f;

            Move(0, 5);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var player = GameManager.Current.Player;

            var onGround = false;
            
            switch (state)
            {
                case State.HIDING:
                    if (!seenPlayer)
                    {
                        XVel = 0;
                        YVel = 0;

                        if (MathUtil.Euclidean(Position, player.Position) < 3 * Globals.TILE || hit)
                        {
                            YVel = -3;
                            seenPlayer = true;                            
                        }
                        SetAnimation(0, 1, .05, true);
                    }
                    else
                    {
                        SetAnimation(2, 2, 0, false);
                        if (stuck)
                        {
                            YVel += Gravity;
                            Move(XVel, YVel);
                            if (!GameManager.Current.Map.CollisionTile(this, XVel, YVel))
                                stuck = false;
                        }
                        else
                        {
                            onGround = this.MoveAdvanced(false);
                            if (onGround)
                                state = State.FOLLOW_PLAYER;
                        }
                    }
                    Depth = Globals.LAYER_FG - .0001f;                    
                    break;
                case State.FOLLOW_PLAYER:

                    onGround = this.MoveAdvanced(false);

                    Direction = (Direction)Math.Sign(player.X - X);

                    if (X > player.X)
                        XVel = Math.Max(XVel - .01f, -.65f);
                    if (X < player.X)
                        XVel = Math.Min(XVel + .01f, .65f);

                    Depth = player.Depth + .00001f;
                    SetAnimation(3, 6, .2, true);
                    break;                    
            }

            if (hit)
                XVel *= .3f;

            Scale = new Vector2(Math.Sign((int)Direction), 1);
        }
    }
}
