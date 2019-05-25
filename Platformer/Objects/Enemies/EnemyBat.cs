using Microsoft.Xna.Framework;
using Platformer.Objects.Main;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPG.Map;

namespace Platformer.Objects.Enemies
{
    public class EnemyBat : Enemy
    {
        public enum State
        {
            IDLE,
            FLY,
            FLY_FOLLOW
        }
        private State state = State.FLY;

        private double t;
        private bool initialized;

        public EnemyBat(float x, float y, Room room) : base(x, y, room)
        {
            BoundingBox = new SPG.Util.RectF(-4, -4, 8, 8);
            DrawOffset = new Vector2(8);
            
            Depth = Globals.LAYER_ENEMY;
            AnimationTexture = AssetManager.EnemyBat;
            
            HP = 5;
            Damage = 1;
            EXP = 3;

            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!initialized)
            {
                if (GameManager.Current.Map.CollisionTile(X, Y - Globals.TILE))
                {
                    state = State.IDLE;
                    Move(0, -2);
                }

                initialized = true;
                return;
            }

            var player = GameManager.Current.Player;

            Direction = (Direction)(Math.Sign(player.X - X));

            if (state == State.IDLE)
            {
                XVel = 0;
                YVel = 0;
                SetAnimation(0, 0, 0, false);

                if (MathUtil.Euclidean(Position, player.Position) < 3 * Globals.TILE || hit)
                {
                    state = State.FLY_FOLLOW;
                }
            }

            if (state == State.FLY)
            {
                SetAnimation(1, 4, .4f, true);

                t = (t + .1f) % (2 * Math.PI);
                YVel = (float)Math.Sin(t);

                var colY = GameManager.Current.Map.CollisionTile(this, 0, YVel);
                if (!colY)
                    Move(0, YVel);
                else
                    YVel = 0;

                if (hit)
                    state = State.FLY_FOLLOW;
            }

            if (state == State.FLY_FOLLOW)
            {
                XVel += Math.Sign(player.X - X) * .005f;
                YVel += Math.Sign(player.Y - Y) * .005f;

                var colX = GameManager.Current.Map.CollisionTile(this, XVel, 0);
                if (!colX)
                    Move(XVel, 0);
                else
                    XVel = -Math.Sign(XVel) * .1f;

                var colY = GameManager.Current.Map.CollisionTile(this, 0, YVel);
                if (!colY)
                    Move(0, YVel);
                else
                    YVel = -Math.Sign(YVel) * .1f;

                XVel = MathUtil.Limit(XVel, 2);
                YVel = MathUtil.Limit(YVel, 2);
                
                SetAnimation(1, 4, .4f, true);
            }

            if (hit)
            {
                XVel *= .6f;
                YVel *= .6f;
            }

            Scale = new Vector2(Math.Sign((int)Direction), 1);
        }
    }
}
