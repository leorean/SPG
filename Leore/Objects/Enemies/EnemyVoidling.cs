using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Objects.Level;
using Leore.Objects.Projectiles;
using SPG.Objects;
using SPG.Util;
using System;
using System.Linq;
using SPG.Map;
using Leore.Resources;
using Leore.Main;

namespace Leore.Objects.Enemies
{
    public class EnemyVoidling : Enemy
    {

        public class Shield : RoomObject
        {
            public Shield(float x, float y, Room room) : base(x, y, room)
            {
                BoundingBox = new RectF(-3, -5, 6, 12);
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
        private int maxSpottedTimer = 120;

        private Direction directionAfterHit = Direction.NONE;

        private int idleTimer;
        private int walkTimer;
        private int knockbackTimer;

        private int type;

        public EnemyVoidling(float x, float y, Room room, int type) : base(x, y, room)
        {
            this.type = type;

            HP = GameResources.EnemyVoidling[type].HP;
            Damage = GameResources.EnemyVoidling[type].Damage;
            EXP = GameResources.EnemyVoidling[type].EXP;

            BoundingBox = new SPG.Util.RectF(-4, -4, 8, 12);
            DrawOffset = new Vector2(8);
            
            AnimationTexture = AssetManager.EnemyVoidling;
            Direction = RND.Choose(Direction.LEFT, Direction.RIGHT);

            knockback = 0;

            Gravity = .1f;
            
            if (type == 1)
            {
                shield = new Shield(X, Y, Room);
                shield.Parent = this;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // flags

            var onWall = !hit
                && ObjectManager.CollisionPointFirstOrDefault<Solid>(this, X + (.5f * BoundingBox.Width + 1) * Math.Sign((int)Direction), Y) != null;

            var onGround = this.MoveAdvanced(false);
            var player = GameManager.Current.Player;

            knockbackTimer = Math.Max(knockbackTimer - 1, 0);

            if (hit)
            {
                directionAfterHit = (Direction)Math.Sign(GameManager.Current.Player.X - X);
                if (onGround && knockbackTimer == 0)
                {
                    knockbackTimer = 60;
                    XVel = -1 * Math.Sign(GameManager.Current.Player.X - X);
                    YVel = -1;
                    state = State.JUMP;
                    onGround = false;
                }
                playerSpotted = player;
                spottedTimer = maxSpottedTimer;
            }

            if (shield != null)
            {

                var shieldDist = state == State.IDLE ? 7f : 8f;
                shield.Position = new Vector2(shield.X + ((X + shieldDist * Math.Sign((int)Direction)) - shield.Position.X) / 4 + XVel, Y + Convert.ToInt32(AnimationFrame % 2 == 1 || state == State.WALK));
            }

            // ++++ spotting the player ++++
            
            spottedTimer = Math.Max(spottedTimer - 1, 0);
            if (spottedTimer == 0)
                playerSpotted = null;

            if (MathUtil.In(Top, player.Top, player.Bottom) || MathUtil.In(Bottom, player.Top, player.Bottom))
            {
                if (Direction == Direction.LEFT && X > player.X
                    || Direction == Direction.RIGHT && X < player.X)
                {
                    var dist = 5 * Globals.TILE;

                    //if (MathUtil.Euclidean(Center, player.Center) < 8 * Globals.TILE)
                    if ((Direction == Direction.LEFT && this.CollisionRectangleFirstOrDefault<Player>(X - dist, Top, X, Bottom) != null)
                        || (Direction == Direction.RIGHT && this.CollisionRectangleFirstOrDefault<Player>(X, Top, X + dist, Bottom) != null))
                    {
                        if (playerSpotted == null)
                        {
                            var xFree = true;
                            for(var i = 0; i < Math.Abs(X - player.X); i += Globals.TILE)
                            {
                                var tileCollision = GameManager.Current.Map.CollisionTile(X + i * Math.Sign((int)Direction), Y);
                                if (tileCollision)
                                {
                                    xFree = false;
                                    break;
                                }
                            }

                            if (xFree)
                            {
                                playerSpotted = player;
                                spottedTimer = maxSpottedTimer;
                            }
                        }
                        
                    }
                }
            }

            // ++++ state stuff ++++

            switch (state)
            {
                case State.IDLE:

                    XVel = 0;

                    idleTimer = Math.Max(idleTimer - 1, 0);

                    if (playerSpotted != null)
                        idleTimer = 0;

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

                    var noGround = !GameManager.Current.Map.CollisionTile(X + Math.Sign((int)Direction) * 8, Bottom + 2);

                    if (onWall || noGround)
                        Direction = Direction.Reverse();

                    if (playerSpotted != null)
                    {
                        Direction = (Direction)Math.Sign(playerSpotted.X - X);
                        walkTimer++;
                    }

                    if (X < Room.X + 8)
                        Direction = Direction.RIGHT;
                    if (X > Room.X + Room.BoundingBox.Width - 8)
                        Direction = Direction.LEFT;

                    walkTimer = Math.Max(walkTimer - 1, 0);
                    if (walkTimer == 0)
                    {
                        idleTimer = 60 + RND.Int(120);
                        state = State.IDLE;
                    }                    
                    XVel = XVel + .05f * Math.Sign((int)Direction);

                    var maxVel = playerSpotted != null ? .75f : .4f;

                    XVel = Math.Sign(XVel) * Math.Min(Math.Abs(XVel), maxVel);
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

            var cols = 12; // how many columns there are in the sheet
            var row = 0; // which row in the sheet
            var fSpd = 0f; // frame speed
            var fAmount = 4; // how many frames
            var loopAnim = true; // loop animation?            
            
            switch (state)
            {
                case State.IDLE:
                    row = 1;
                    fSpd = 0.045f;
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

            SetAnimation(cols * row + type * 4, cols * row + fAmount - 1 + type * 4, fSpd, loopAnim);            
            var xScale = Math.Sign((int)Direction);
            Scale = new Vector2(xScale, 1);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            if (spottedTimer > 0)
                sb.Draw(AnimationTexture[1], Position + new Vector2(0, -16), null, Color.White, 0, new Vector2(8), Vector2.One, SpriteEffects.None, Depth + .0001f);
        }
    }    
}
