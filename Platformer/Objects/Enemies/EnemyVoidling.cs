using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Objects.Level;
using Platformer.Objects.Main;
using Platformer.Objects.Projectiles;
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

        public class Shield : RoomObject
        {
            public Shield(float x, float y, Room room) : base(x, y, room)
            {
                BoundingBox = new RectF(-4, -6, 8, 12);
                DrawOffset = new Vector2(8);
                Texture = AssetManager.EnemyVoidling[0];
            }

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);

                var proj = this.CollisionBoundsFirstOrDefault<PlayerProjectile>(X, Y);

                if (proj != null)
                {
                    proj.HandleCollision(this);
                }

                Scale = new Vector2(Math.Sign(X - Parent.X), 1);
                Depth = Parent.Depth + .0001f;
            }
        }

        public enum State
        {
            IDLE,
            WALK,
            JUMP
        }

        private State state;

        private Shield shield;

        private Player playerSpotted;
        private int spottedTimer;

        private Direction directionAfterHit = Direction.NONE;

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

            shield = new Shield(X, Y, Room);
            shield.Parent = this;
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // flags

            var onWall = !hit && ObjectManager.CollisionPointFirstOrDefault<Solid>(this, X + (.5f * BoundingBox.Width + 1) * Math.Sign((int)Direction), Y) != null;
            var onGround = this.MoveAdvanced(false);

            if (onGround)
            {
                if (hit)
                {
                    XVel = -1 * Math.Sign(GameManager.Current.Player.X - X);
                    YVel = -1;
                    directionAfterHit = (Direction)Math.Sign(GameManager.Current.Player.X - X);
                    walkTimer = 120;
                    state = State.JUMP;
                    onGround = false;
                }
            }

            shield.Position = shield.Position + new Vector2(((X + 8f * Math.Sign((int)Direction)) - shield.Position.X) / 2, 0);
            //shield.Position = Position + new Vector2(8f * Math.Sign((int)Direction), 0);

            // state stuff
            
            var player = GameManager.Current.Player;

            if (MathUtil.In(Top, player.Top, player.Bottom) || MathUtil.In(Bottom, player.Top, player.Bottom))
            {
                if (Direction == Direction.LEFT && X > player.X
                    || Direction == Direction.RIGHT && X < player.X)
                {
                    if (MathUtil.Euclidean(Center, player.Center) < 8 * Globals.TILE)
                    {
                        if (playerSpotted == null)
                            playerSpotted = player;
                        spottedTimer = 60;
                    }
                }
            }
            spottedTimer = Math.Max(spottedTimer - 1, 0);
            if (spottedTimer == 0)
                playerSpotted = null;

            switch (state)
            {
                case State.IDLE:

                    XVel = 0;

                    idleTimer = Math.Max(idleTimer - 1, 0);
                    if (idleTimer == 0)
                    {
                        if (playerSpotted == null)
                            Direction = RND.Choose(Direction.LEFT, Direction.RIGHT);
                        else
                            Direction = (Direction)Math.Sign(playerSpotted.X - X);

                        walkTimer = 60 + RND.Int(120);
                        state = State.WALK;
                    }                    
                    break;
                case State.WALK:

                    if (onWall)
                        Direction = Direction.Reverse();

                    if (playerSpotted != null)
                    {
                        Direction = (Direction)Math.Sign(playerSpotted.X - X);
                        walkTimer++;
                    }

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
                    XVel *= .9f;
                    if (onGround)
                    {
                        if (directionAfterHit != Direction.NONE)
                            Direction = directionAfterHit;

                        directionAfterHit = Direction.NONE;
                        state = State.WALK;
                    }
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
                    row = 1;
                    fSpd = 0.07f;
                    break;
                case State.WALK:
                    row = 2;
                    fSpd = 0.15f;
                    break;
                case State.JUMP:
                    row = 3;
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

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            if (playerSpotted != null)
                sb.Draw(AnimationTexture[1], Position + new Vector2(0, -16), null, Color.White, 0, new Vector2(8), Vector2.One, SpriteEffects.None, Depth + .0001f);
        }
    }    
}
