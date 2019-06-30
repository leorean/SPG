using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Leore.Objects.Effects;
using Leore.Objects.Items;
using Leore.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using SPG.Util;
using SPG.Map;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Level;

namespace Leore.Objects.Enemies
{
    public class EnemySlime : Enemy
    {
        public enum State { IDLE, WALK }

        private State state;

        private int type;

        private bool onGround;

        private int walkTimer = 60;
        private int idleTimer = 60;

        private long originalID;

        private float scale;
        private int invincible = 30;
        private int mergeTimer = 3 * 60;
        private int regenTimer = 1 * 60;

        double t = 0;
        
        public EnemySlime(float x, float y, Room room, int type, float scale) : base(x, y, room)
        {
            this.type = type;

            // game stats

            HP = GameResources.EnemySlime.HP;
            Damage = GameResources.EnemySlime.Damage;
            EXP = GameResources.EnemySlime.EXP;

            // draw stats

            DebugEnabled = true;
            DrawOffset = new Vector2(16);
            
            AnimationTexture = AssetManager.EnemySlime;

            // other stats

            this.scale = scale;
            Scale = new Vector2(scale);
            Direction = RND.Choose(Direction.LEFT, Direction.RIGHT);

            UpdateScaleAndBoundingBox();

            originalID = ID;
            Gravity = .1f;            
        }

        public override void Hit(int hitPoints, float degAngle)
        {
            if (invincible > 0)
                return;

            base.Hit(hitPoints, degAngle);
            if (onGround)
            {
                YVel = -1f;
                XVel -= 1.5f * Math.Sign(GameManager.Current.Player.X - X);
            }
            else
            {
                XVel *= .8f;
                YVel *= .8f;                
            }
            if (MaxHP > 2) {
                Split();
            }
        }

        private EnemySlime SpawnSlime(Direction dir)
        {
            var newScale = .5f;

            var slime = new EnemySlime(X, Y, Room, type, scale * newScale);
            slime.Respawn = true;
            slime.XVel = 1 * (int)dir;
            slime.YVel = -1.5f;
            slime.Direction = dir;
            slime.ID = ID + ObjectManager.GlobalIDCounter;
            var newHp = (int)Math.Ceiling(HP * .5f);
            slime.MaxHP = newHp;
            slime.HP = newHp;
            slime.originalID = originalID;
            slime.mergeTimer = 3 * 60;

            return slime;
        }

        private void Split()
        {
            var hpr = (HP / (float)MaxHP);

            if (hpr <= .5f)
            {
                var slime1 = SpawnSlime(Direction.LEFT);
                var slime2 = SpawnSlime(Direction.RIGHT);
                OnDeath();
            }
        }

        private void Merge(EnemySlime other)
        {
            if (mergeTimer > 0 || other.mergeTimer > 0)
                return;
            
            MaxHP = HP + other.HP;
            HP = MaxHP;

            UpdateScaleAndBoundingBox();

            //while (GameManager.Current.Map.CollisionTile(this, 0, 0))
            //    Move(0, -1);

            //new SingularEffect(X, Y, 14) { Scale = Scale };
            new WaterSplashEmitter(X, Y) { SpawnRate = 50, ParticleColors = GameResources.HpColors };

            other.Destroy();
        }

        public override void OnDeath()
        {
            var otherSlimes = ObjectManager.FindAll<EnemySlime>();
            var children = 0;
            
            foreach(var slime in otherSlimes)
            {
                if (slime.originalID == originalID && slime != this)
                    children++;
            }

            var exp = (int)Math.Ceiling(EXP * scale);
            SpellEXP.Spawn(X, Y, exp);
            new SingularEffect(X, Y) { Scale = new Vector2(scale) };

            if (children == 0)
                GameManager.Current.NonRespawnableIDs.Add(originalID);

            Destroy();
        }

        private void UpdateScaleAndBoundingBox()
        {

            var mhp = MathUtil.Clamp(HP, .5f * GameResources.EnemySlime.HP, 2 * GameResources.EnemySlime.HP);

            scale = (mhp / GameResources.EnemySlime.HP);

            BoundingBox = new SPG.Util.RectF(-6 * scale, -4 * scale, 12 * scale, 12 * scale);

            var blocks = this.CollisionBounds<Solid>(X, Y);
            
            //bool moveX = false;
            //while (XVel >= 0 && GameManager.Current.Map.CollisionTile(Right + .1f, Y - 1))
            //{
            //    Move(-.1f, 0);
            //    moveX = true;
            //}
            //while (XVel < 0 && GameManager.Current.Map.CollisionTile(Left - .1f, Y - 1))
            //{
            //    Move(.1f, 0);
            //    moveX = true;
            //}
            //if (XVel == 0 && GameManager.Current.Map.CollisionTile(this, XVel, YVel))
            //{
            //    Move(0, -.5f);
            //}
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            UpdateScaleAndBoundingBox();

            invincible = Math.Max(invincible - 1, 0);
            mergeTimer = Math.Max(mergeTimer - 1, 0);
            regenTimer = Math.Max(regenTimer - 1, 0);

            if (MaxHP >= 2 * GameResources.EnemySlime.HP)
                regenTimer = 1;

            if (regenTimer == 0)
            {
                MaxHP++;
                HP++;
                regenTimer = 60;
            }
            if (mergeTimer == 0)
            {
                var other = this.CollisionBoundsFirstOrDefault<EnemySlime>(X + XVel, Y + YVel);

                if (other != null)
                {
                    Merge(other);
                }
            }

            switch (state)
            {
                case State.IDLE:
                    idleTimer = Math.Max(idleTimer - 1, 0);
                    XVel *= .9f;
                    if (idleTimer == 0)
                    {
                        state = State.WALK;
                        walkTimer = 60 + RND.Int(2 * 60);
                        
                        Direction = RND.Choose(Direction.LEFT, Direction.RIGHT);                        
                    }
                    break;
                case State.WALK:
                    walkTimer = Math.Max(walkTimer - 1, 0);
                    
                    XVel = Math.Sign((int)Direction) * Math.Min(Math.Abs(XVel) + .1f, .3f);
                    if (walkTimer == 0)
                    {
                        state = State.IDLE;
                        idleTimer = 60 + RND.Int(2 * 60);
                    }
                    break;           
            }

            onGround = this.MoveAdvanced(false);
            
            // ++++ draw <-> state logic ++++

            //var cols = 5; // how many columns there are in the sheet
            //var row = 0; // which row in the sheet
            //var fSpd = 0f; // frame speed
            //var fAmount = 4; // how many frames
            //var loopAnim = true; // loop animation?
            
            switch (state)
            {
                case State.IDLE:
                    t = (t + .05f) % (2 * Math.PI);
                    break;
                case State.WALK:
                    t = (t + .15f) % (2 * Math.PI);
                    break;                               
            }

            Position = new Vector2(MathUtil.Clamp(X, Room.X + XVel, Room.X + XVel + Room.BoundingBox.Width), Y);

            var xs = (float)(.95 + Math.Sin(t) * .1);
            var ys = 1;

            //SetAnimation(cols * row, cols * row + fAmount - 1, fSpd, loopAnim);
            Scale = new Vector2((int)Direction * scale * .5f * xs, .5f * scale * ys);

            if (this.IsOutsideCurrentRoom(1 * Globals.T))
                Destroy();
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            AssetManager.DefaultFont.Draw(sb, X, Bottom + Globals.T, $"{HP}", scale:.5f);
        }
    }
}
