using Leore.Main;
using Microsoft.Xna.Framework;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPG.Map;
using Leore.Objects.Effects.Emitters;

namespace Leore.Objects.Enemies
{
    public class BossGiantBat : Boss
    {
        private enum State
        {
            FOLLOW_PLAYER,
            SPAWN_MINIONS
        }
        
        private State state;

        private Player player => GameManager.Current.Player;
        
        private int invincible;
        private int damageTaken;
        private int spawnMinionTimer;
        private int totalMinionsSpawned;

        private List<EnemyBat> Minions = new List<EnemyBat>();

        public BossGiantBat(float x, float y, Room room, string setCondition) : base(x, y, room, setCondition)
        {
            HP = 200;

            AnimationTexture = AssetManager.BossGiantBat;
            DrawOffset = new Vector2(32);
            BoundingBox = new SPG.Util.RectF(-16, -16, 32, 32);
            DebugEnabled = true;

            Direction = Direction.RIGHT;

            state = State.FOLLOW_PLAYER;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            invincible = Math.Max(invincible - 1, 0);
            
            switch (state)
            {
                case State.FOLLOW_PLAYER:

                    Direction = (Direction)Math.Sign(player.X - X);

                    XVel += Math.Sign(player.X - X) * .01f;
                    YVel += Math.Sign(player.Y - Y) * .01f;

                    if (damageTaken > 20)
                    {
                        spawnMinionTimer = 150;
                        state = State.SPAWN_MINIONS;
                    }

                    SetAnimation(0, 2, .2f, true);
                    break;
                case State.SPAWN_MINIONS:

                    invincible = 1;

                    XVel *= .9f;
                    YVel *= .9f;

                    damageTaken = 0;

                    SetAnimation(3, 5, .2f, true);

                    if (GameManager.Current.Map.CollisionTile(this, 0, 0))
                    {
                        Move(0, -1);
                        return;
                    }

                    spawnMinionTimer = Math.Max(spawnMinionTimer - 1, 0);
                    if (spawnMinionTimer > 0)
                    {
                        if (spawnMinionTimer % 30 == 0)
                        {
                            var minion = new EnemyBat(X, Y, Room)
                            {
                                XVel = -.5f + (float)RND.Next * 1, YVel = -.5f + (float)RND.Next * 1,
                                State = EnemyBat.EnemyState.FLY_FOLLOW,
                                Direction = Direction
                                
                            };
                            minion.ID = minion.ID + totalMinionsSpawned + 1;
                            Minions.Add(minion);
                            totalMinionsSpawned++;
                        }
                    } else
                    {
                        if (Minions.Count == 0)
                            state = State.FOLLOW_PLAYER;
                    }

                    foreach(var minion in Minions.ToList())
                    {
                        if (minion.HP == 0)
                            Minions.Remove(minion);

                    }
                    
                    break;
            }


            XVel = MathUtil.Limit(XVel, 1);
            YVel = MathUtil.Limit(YVel, 1);

            Move(XVel, YVel);
            var thresh = 32;
            Position = new Vector2(MathUtil.Clamp(X, Room.X + thresh, Room.X + Room.BoundingBox.Width - thresh), MathUtil.Clamp(Y, Room.Y + thresh, Room.Y + Room.BoundingBox.Height - thresh));

            Scale = new Vector2((int)Direction, 1);

        }

        public override void Hit(int hitPoints, float degAngle)
        {
            if (invincible > 0)
                return;

            invincible = 0;
            damageTaken += hitPoints;
            
            base.Hit(hitPoints, degAngle);

            XVel *= .4f;
            YVel *= .4f;            
        }


        public override void OnDeath()
        {
            base.OnDeath();

            new FlashEmitter(X, Y);
            foreach(var minion in Minions.ToList())
            {
                minion.OnDeath();                
            }
            Minions.Clear();
        }
    }
}
