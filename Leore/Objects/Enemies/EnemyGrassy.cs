using Microsoft.Xna.Framework;
using SPG.Util;
using System;
using SPG.Map;
using Leore.Resources;
using Leore.Main;
using SPG.Objects;
using Leore.Objects.Level;

namespace Leore.Objects.Enemies
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
        private bool stuck = false;

        public EnemyGrassy(float x, float y, Room room) : base(x, y, room)
        {
            BoundingBox = new SPG.Util.RectF(-4, -4, 8, 12);
            DrawOffset = new Vector2(8);
            
            //DebugEnabled = true;

            AnimationTexture = AssetManager.EnemyGrassy;
            Direction = Direction.RIGHT;

            HP = GameResources.EnemyGrassy.HP;
            Damage = GameResources.EnemyGrassy.Damage;
            EXP = GameResources.EnemyGrassy.EXP;
            
            Gravity = .1f;
            Depth = Globals.LAYER_ENEMY;
            //Move(0, 5);
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
                        
                        if (MathUtil.Euclidean(Position, player.Position) < 3 * Globals.T || hit)
                        {
                            YVel = -3;
                            seenPlayer = true;                            
                        }
                        SetAnimation(0, 1, .025, true);
                    }
                    else
                    {
                        SetAnimation(2, 2, 0, false);
                        if (stuck)
                        {
                            YVel += Gravity;
                            Move(XVel, YVel);
                            if (!GameManager.Current.Map.CollisionTile(this, XVel, YVel))
                            {                                
                                stuck = false;
                            }
                        }
                        else
                        {
                            onGround = this.MoveAdvanced(false);
                            if (onGround)
                                state = State.FOLLOW_PLAYER;
                        }
                    }
                    break;
                case State.FOLLOW_PLAYER:

                    onGround = this.MoveAdvanced(false);

                    // correction
                    if (this.CollisionRectangleFirstOrDefault<Solid>(Left, Bottom - 1, Right, Bottom - 1) != null)
                        Move(0, -1);

                    if (onGround && MathUtil.Euclidean(Center, player.Center) < 1.5f * Globals.T)
                    {
                        XVel *= .97f;
                    }

                    if (Math.Abs(player.X - X) > 8)
                        Direction = (Direction)Math.Sign(player.X - X);

                    if (X > player.X)
                        XVel = Math.Max(XVel - .01f, -.65f);
                    if (X < player.X)
                        XVel = Math.Min(XVel + .01f, .65f);

                    
                    SetAnimation(3, 6, .2, true);
                    break;                    
            }

            if (hit)
                XVel *= .3f;

            Scale = new Vector2(Math.Sign((int)Direction), 1);
        }
    }
}
