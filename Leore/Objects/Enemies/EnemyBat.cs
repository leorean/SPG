using Microsoft.Xna.Framework;
using SPG.Util;
using System;
using SPG.Map;
using Leore.Resources;
using Leore.Main;
using Leore.Objects;

namespace Leore.Objects.Enemies
{
    public class EnemyBat : Enemy, IMovable
    {
        public enum EnemyState
        {
            IDLE,
            FLY,
            FLY_FOLLOW
        }
        public EnemyState State { get; set; } = EnemyState.FLY;

        private double t = .5f * Math.PI;
        private int initDelay = 3;

        public EnemyBat(float x, float y, Room room) : base(x, y, room)
        {
            BoundingBox = new SPG.Util.RectF(-4, -4, 8, 8);
            DrawOffset = new Vector2(8);
            
            AnimationTexture = AssetManager.EnemyBat;

            HP = GameResources.EnemyBat.HP;
            Damage = GameResources.EnemyBat.Damage;
            EXP = GameResources.EnemyBat.EXP;

            Direction = Direction.RIGHT;
            
        }

        public override void EndUpdate(GameTime gameTime)
        {
            base.EndUpdate(gameTime);

            if (initDelay > 0)
            {
                initDelay = Math.Max(initDelay - 1, 0);
                if (initDelay == 0)
                {

                    if (GameManager.Current.Map.CollisionTile(X, Y - Globals.T))
                    {
                        State = EnemyState.IDLE;
                        Move(0, -2);
                    }
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (initDelay > 0)
                return;
            
            var player = GameManager.Current.Player;

            if (X != player.X)
                Direction = (Direction)(Math.Sign(player.X - X));

            if (State == EnemyState.IDLE)
            {
                //XVel = 0;
                //YVel = 0;
                SetAnimation(0, 0, 0, false);

                if (MathUtil.Euclidean(Position, player.Position) < 3 * Globals.T || hit)
                {
                    State = EnemyState.FLY_FOLLOW;
                }
            }

            if (State == EnemyState.FLY)
            {
                SetAnimation(1, 4, .2f, true);

                XVel *= .9f;

                t = (t + .08f) % (2 * Math.PI);
                YVel = (float)Math.Sin(t);
                
                var colY = GameManager.Current.Map.CollisionTile(this, 0, YVel) || GameManager.Current.Map.CollisionTile(Position.X, Position.Y, GameMap.WATER_INDEX);
                if (colY)
                    YVel = 0;

                if (hit)
                    State = EnemyState.FLY_FOLLOW;
            }

            if (State == EnemyState.FLY_FOLLOW)
            {
                XVel += Math.Sign(player.X - X) * .005f;
                YVel += Math.Sign(player.Y - Y) * .005f;

                var colX = GameManager.Current.Map.CollisionTile(this, XVel, 0);
                if (colX)
                    XVel = -Math.Sign(XVel) * .1f;

                var colY = GameManager.Current.Map.CollisionTile(this, 0, YVel) || GameManager.Current.Map.CollisionTile(Position.X, Position.Y, GameMap.WATER_INDEX);
                if (colY)
                    YVel = -Math.Sign(YVel) * .1f;

                XVel = MathUtil.Limit(XVel, 2);
                YVel = MathUtil.Limit(YVel, 2);
                
                SetAnimation(1, 4, .3f, true);
            }

            if (hit)
            {
                XVel *= .6f;
                YVel *= .6f;
            }

            XVel = MathUtil.Limit(XVel, 1);
            YVel = MathUtil.Limit(YVel, 1);
            {
                var colX = GameManager.Current.Map.CollisionTile(this, XVel, 0) || GameManager.Current.Map.CollisionTile(Position.X, Position.Y, GameMap.WATER_INDEX);
                if (!colX)
                    Move(XVel, 0);
                else
                    XVel = 0;

                var colY = GameManager.Current.Map.CollisionTile(this, 0, YVel) || GameManager.Current.Map.CollisionTile(Position.X, Position.Y, GameMap.WATER_INDEX);
                if (!colY)
                    Move(0, YVel);
                else
                    YVel = 0;

                //this.MoveAdvanced(false);
                //Move(XVel, YVel);
            }
            Scale = new Vector2(Math.Sign((int)Direction), 1);
        }
    }
}
