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
        private int maxMergeTimer = 3 * 60;
        private int mergeTimer = 0;
        private int regenTimer = 1 * 60;

        double t = 0;
        float xs, ys;
        
        public EnemySlime(float x, float y, Room room, int type) : base(x, y, room)
        {
            this.type = type;

            // game stats

            HP = GameResources.EnemySlime.HP;
            Damage = GameResources.EnemySlime.Damage;
            EXP = GameResources.EnemySlime.EXP;

            // draw stats

            Visible = false;
            DebugEnabled = true;
            DrawOffset = new Vector2(16);
            
            AnimationTexture = AssetManager.EnemySlime;

            // other stats
            
            Direction = RND.Choose(Direction.LEFT, Direction.RIGHT);

            UpdateScaleAndBoundingBox();

            mergeTimer = maxMergeTimer;

            originalID = ID;
            Gravity = .1f;            
        }

        public override void Hit(int hitPoints, float degAngle)
        {
            regenTimer = 60;

            base.Hit(hitPoints, degAngle);
            if (onGround)
            {
                YVel = -1f;
                XVel -= 1.5f * Math.Sign(GameManager.Current.Player.X - X);
            }
            else
            {
                XVel *= .8f;
                YVel *= .4f;                
            }
            if (HP < .5f * MaxHP && MaxHP >= 4) {
                Split();
            }
        }

        private EnemySlime SpawnSlime(Direction dir)
        {
            var slime = new EnemySlime(X, Y - 2, Room, type);
            slime.Respawn = true;
            slime.XVel = 1 * (int)dir;
            slime.YVel = -1.5f;
            slime.Direction = dir;
            slime.ID = ID + ObjectManager.GlobalIDCounter;
            var newHp = (int)Math.Ceiling(HP * .66f);
            slime.MaxHP = newHp;
            slime.HP = newHp;
            slime.originalID = originalID;
            
            return slime;
        }

        private void Split()
        {
            var hpr = (HP / (float)MaxHP);

            //if (hpr >= .75f)
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

            if (HP >= 2 * GameResources.EnemySlime.HP || other.HP >= 2 * GameResources.EnemySlime.HP)
                return;
            
            MaxHP = Math.Min(HP + other.HP, 2 * GameResources.EnemySlime.HP);
            HP = MaxHP;

            UpdateScaleAndBoundingBox();
            
            // TODO
            new WaterSplashEmitter(X, Y) { SpawnRate = 50, ParticleColors = GameResources.HpColors };

            mergeTimer = maxMergeTimer;

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
            
            if (children == 0)
                GameManager.Current.NonRespawnableIDs.Add(originalID);

            Destroy();
        }

        private void UpdateScaleAndBoundingBox()
        {
            var mhp = MathUtil.Clamp(HP + 4, 4, 2 * GameResources.EnemySlime.HP);

            scale = (mhp / GameResources.EnemySlime.HP);

            BoundingBox = new SPG.Util.RectF(-6 * scale, -2 * scale, 12 * scale, 14 * scale);            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            invincible = Math.Max(invincible - 1, 0);
            mergeTimer = Math.Max(mergeTimer - 1, 0);
            regenTimer = Math.Max(regenTimer - 1, 0);

            IgnoreProjectiles = invincible > 0;

            if (regenTimer == 0)
            {
                HP = Math.Min(HP + 1, Math.Max(MaxHP, 4));
                regenTimer = 30;
            }
            var other = this.CollisionBoundsFirstOrDefault<EnemySlime>(X + XVel, Y + YVel);

            if (other != null)
            {
                if (mergeTimer == 0)
                    Merge(other);
                else
                {
                    XVel += Math.Sign(X - other.X) * .5f;
                    other.XVel += Math.Sign(other.X - X) * .5f;

                    XVel = MathUtil.AtMost(XVel, 1f);
                    other.XVel = MathUtil.AtMost(other.XVel, 1f);
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

            UpdateScaleAndBoundingBox();
            onGround = this.MoveAdvanced(false);
            
            var blocks = this.CollisionBounds<Solid>(X, Y);

            bool left = false;
            bool right = false;
            bool bottom = false;

            foreach (var block in blocks)
            {
                if (this.CollisionBounds(block, X - 8, Y - 1))
                    left = true;
                if (this.CollisionBounds(block, X + 8, Y - 1))
                    right = true;
                if (this.CollisionBounds(block, X, Y + 2))
                    bottom = true;
            }

            if (right) Move(-1, 0);
            if (left) Move(1, 0);
            if (bottom)
            {
                Move(0, -2);                
            }
            
            // ++++ draw <-> state logic ++++

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

            xs = (float)(.99 + Math.Sin(t) * .01);
            ys = (float)(.995 + Math.Sin(t) * .015);

            DrawOffset = new Vector2(16, 8 * ys);

            Scale = new Vector2((int)Direction * scale * .5f * xs, .5f * scale * ys);

            Visible = true;

            if (this.IsOutsideCurrentRoom(1 * Globals.T))
                Destroy();
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            if (!Visible)
                return;

            base.Draw(sb, gameTime);
            //sb.Draw(Texture, Position + new Vector2(0, 0), null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);

            AssetManager.DefaultFont.Draw(sb, X, Bottom + Globals.T, $"{HP}", scale:.5f);
        }
    }
}
