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
using Leore.Objects.Items;
using System.Diagnostics;
using Leore.Objects.Effects;
using Microsoft.Xna.Framework.Graphics;

namespace Leore.Objects.Enemies
{
    public class BossGiantBat : Boss
    {
        private enum State
        {
            FOLLOW_PLAYER,
            SPAWN_MINIONS,
            DIE
        }
        
        private State state;

        private Player player => GameManager.Current.Player;
        
        private int damageTaken;
        private int spawnMinionTimer;
        private int maxMinionsSpawned;
        private int minionsSpawned;
        private int totalMinionsSpawned;
                
        private int deathTimer = 2 * 60;
        private float deathAlpha = 0;

        private float alpha = 1;

        private bool preventDeath;

        private List<EnemyBat> Minions = new List<EnemyBat>();

        public BossGiantBat(float x, float y, Room room, string setCondition) : base(x, y, room, setCondition)
        {
            HP = 120;
            
            AnimationTexture = AssetManager.BossGiantBat;
            DrawOffset = new Vector2(40);
            BoundingBox = new SPG.Util.RectF(-16, -16, 32, 32);
            
            knockback = .3f;

            Direction = Direction.RIGHT;

            state = State.FOLLOW_PLAYER;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
                        
            switch (state)
            {
                case State.FOLLOW_PLAYER:

                    Damage = 1;
                    alpha = Math.Min(alpha + .05f, 1f);

                    IgnoreProjectiles = false;

                    Direction = (Direction)Math.Sign(player.X - X);

                    XVel += Math.Sign(player.X - X) * .01f;
                    YVel += Math.Sign(player.Y - 16 - Y) * .01f;

                    if (damageTaken > 15)
                    {
                        spawnMinionTimer = 0;

                        maxMinionsSpawned = (int)((.5f - (HP / (float)MaxHP)) * 15);
                        maxMinionsSpawned = (int)MathUtil.Clamp(maxMinionsSpawned, 3, 15);
                        Debug.WriteLine(maxMinionsSpawned);

                        state = State.SPAWN_MINIONS;
                    }

                    SetAnimation(0, 3, .2f, true);
                    break;
                case State.SPAWN_MINIONS:

                    Damage = 0;
                    alpha = Math.Max(alpha - .03f, .5f);

                    Direction = (Direction)Math.Sign(player.X - X);

                    IgnoreProjectiles = true;

                    XVel *= .9f;
                    YVel *= .9f;

                    damageTaken = 0;

                    SetAnimation(4, 7, .15f, true);

                    MoveTowards(player.Position + new Vector2((int)Direction * -3 * Globals.T, -2 * Globals.T), 100);

                    if (GameManager.Current.Map.CollisionTile(this, 0, 0))
                    {
                        //Move(0, -1);
                        //MoveTowards(player.Position + new Vector2((int)Direction * 3 * Globals.TILE, -2 * Globals.TILE), 100);
                    }
                    else
                    {
                        spawnMinionTimer = Math.Max(spawnMinionTimer - 1, 0);
                        //if (spawnMinionTimer > 0)
                        {
                            if (spawnMinionTimer % 30 == 0 && minionsSpawned < maxMinionsSpawned)
                            {
                                spawnMinionTimer = 30;
                                var minion = new EnemyBat(X, Y, Room)
                                {
                                    XVel = -.5f + (float)RND.Next * 1,
                                    YVel = -.5f + (float)RND.Next * 1,
                                    State = EnemyBat.EnemyState.FLY_FOLLOW,
                                    Direction = Direction

                                };
                                SpellEXP.Spawn(X, Y, 5);
                                minion.EXP = 18;

                                // TODO: FIND SOLUTION FOR THIS!!

                                //minion.ID = minion.ID + totalMinionsSpawned + 1;

                                minion.Depth = Depth + .0001f;
                                Minions.Add(minion);
                                minionsSpawned++;
                                totalMinionsSpawned++;
                            }
                        }
                        //else
                        {
                            if (Minions.Count == 0)
                            {
                                minionsSpawned = 0;
                                state = State.FOLLOW_PLAYER;
                            }
                        }

                        foreach (var minion in Minions.ToList())
                        {
                            if (minion.HP == 0)
                                Minions.Remove(minion);

                        }
                    }
                    break;
                case State.DIE:

                    preventDeath = false;

                    Move(0, -.2f);

                    XVel = 0;
                    YVel = 0;
                    Damage = 0;
                    deathTimer = Math.Max(deathTimer - 1, 0);
                    if (deathTimer % 5 == 0)
                        new SingularEffect(Left + RND.Int((int)BoundingBox.Width), Top + RND.Int((int)BoundingBox.Height), 8);

                    if (deathTimer == 0)
                    {
                        Coin.Spawn(X, Y, Room, 1000);
                        new FlashEmitter(X, Y, longFlash: true);
                        base.OnDeath();
                        base.Destroy();
                    }

                    break;
            }


            XVel = MathUtil.AtMost(XVel, 1);
            YVel = MathUtil.AtMost(YVel, 1);

            Move(XVel, YVel);
            var tx = 48;
            var ty = 48;
            Position = new Vector2(MathUtil.Clamp(X, Room.X + tx, Room.X + Room.BoundingBox.Width - tx), MathUtil.Clamp(Y, Room.Y + ty, Room.Y + Room.BoundingBox.Height - ty));

            Scale = new Vector2((int)Direction, 1);
            Color = new Color(Color, alpha);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            if (state != State.DIE)
            {
                base.Draw(sb, gameTime);
            }
            else
            {
                deathAlpha = Math.Min(deathAlpha + .01f, 1);

                sb.Draw(AssetManager.BossGiantBat[8], Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
                sb.Draw(AssetManager.BossGiantBat[9], Position, null, new Color(Color, deathAlpha), Angle, DrawOffset, Scale, SpriteEffects.None, Depth + .0001f);
            }
        }

        public override void Hit(int hitPoints, float degAngle)
        {
            damageTaken += hitPoints;
            
            base.Hit(hitPoints, degAngle);            
        }

        public override void Destroy(bool callGC = false)
        {
            if (preventDeath)
                return;
            base.Destroy();
        }

        public override void OnDeath()
        {
            //base.OnDeath();

            preventDeath = true;

            if (state == State.DIE)
                return;
            state = State.DIE;
            
            foreach (var minion in Minions.ToList())
            {
                minion.OnDeath();                
            }
            Minions.Clear();
        }
    }
}
