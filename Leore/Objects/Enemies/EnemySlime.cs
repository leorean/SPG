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
        private int regenTimer = 3 * 60;

        double t = 0;
        float xs, ys;
        float drawScale = 1;
        int animationTimer = 0;

        private bool top, bottom, left, right, free;
        
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
            DrawOffset = new Vector2(16, 32);

            AnimationTexture = AssetManager.EnemySlime;

            // other stats
            
            Direction = RND.Choose(Direction.LEFT, Direction.RIGHT);

            UpdateScaleAndBoundingBox();

            mergeTimer = maxMergeTimer;            

            originalID = ID;
            Gravity = .1f;

            AnimationComplete += EnemySlime_AnimationComplete;
        }

        private void EnemySlime_AnimationComplete(object sender, EventArgs e)
        {
            SetAnimation(1, 1, 0, false);
            animationTimer = 60 + RND.Int(60);
        }

        private void SpawnParticles(int spawnRate)
        {
            new SlimeEmitter(X, Y, type) { SpawnRate = spawnRate };
        }

        public override void Hit(int hitPoints, float degAngle)
        {
            regenTimer = 60;

            SpawnParticles(5);

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
            if (HP <= .5f * MaxHP && MaxHP > 2) {
                Split();
            }
        }

        private EnemySlime SpawnSlime(Direction dir)
        {
            var slime = new EnemySlime(X, Y, Room, type);
            slime.Respawn = true;
            slime.XVel = 1 * (int)dir;
            slime.YVel = -1.5f;
            slime.Direction = dir;
            slime.ID = ID + ObjectManager.GlobalIDCounter;
            var newHp = (int)Math.Ceiling(HP * .75f);
            slime.MaxHP = newHp;
            slime.HP = newHp;
            slime.originalID = originalID;
            
            return slime;
        }

        private void Split()
        {
            SpawnParticles(20);

            var slime1 = SpawnSlime(Direction.LEFT);
            var slime2 = SpawnSlime(Direction.RIGHT);
            OnDeath();            
        }

        private void Merge(EnemySlime other)
        {
            if (mergeTimer > 0 || other.mergeTimer > 0 || top || other.top)
                return;

            if (HP >= 2 * GameResources.EnemySlime.HP || other.HP >= 2 * GameResources.EnemySlime.HP)
                return;
            
            MaxHP = Math.Min(HP + other.HP, 2 * GameResources.EnemySlime.HP);
            HP = MaxHP;

            UpdateScaleAndBoundingBox();

            SpawnParticles(20);

            new SingularEffect(X, Y, 9);

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

            //BoundingBox = new SPG.Util.RectF(-6 * scale, -2 * scale, 12 * scale, 14 * scale);
            BoundingBox = new SPG.Util.RectF(-6 * scale, -14 * scale, 12 * scale, 14 * scale);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            invincible = Math.Max(invincible - 1, 0);
            mergeTimer = Math.Max(mergeTimer - 1, 0);
            regenTimer = Math.Max(regenTimer - 1, 0);
            animationTimer = Math.Max(animationTimer - 1, 0);

            if (animationTimer == 0)
            {
                // for the eyes
                SetAnimation(1, 4, .2f, true);                
            }
            
            IgnoreProjectiles = invincible > 0;

            if (!top)
            {
                if (regenTimer == 0)
                {
                    HP = Math.Min(HP + 1, Math.Max(MaxHP, 4));
                    regenTimer = 30;
                }
            }
            else
            {
                //Split();
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

                    xs = (float)(.99 + Math.Sin(t) * .01);
                    //ys = (float)(.995 + Math.Sin(t) * .015);
                    ys = (float)(.95 + Math.Sin(t) * .1);

                    if (idleTimer == 0)
                    {
                        state = State.WALK;
                        walkTimer = 60 + RND.Int(2 * 60);
                        
                        Direction = RND.Choose(Direction.LEFT, Direction.RIGHT);                        
                    }
                    break;
                case State.WALK:
                    walkTimer = Math.Max(walkTimer - 1, 0);

                    if (onGround)
                    {
                        xs = 1.05f;
                        ys = .95f;
                        YVel = -1f;
                    }

                    xs += (1 - xs) / 20f;
                    ys += (1 - ys) / 20f;

                    XVel = Math.Sign((int)Direction) * Math.Min(Math.Abs(XVel) + .1f, .3f);
                    if (walkTimer == 0)
                    {
                        state = State.IDLE;
                        idleTimer = 60 + RND.Int(2 * 60);
                    }
                    break;           
            }

            // ++++ collision ++++

            UpdateScaleAndBoundingBox();

            var inWater = GameManager.Current.Map.CollisionTile(X, Y - 2, GameMap.WATER_INDEX);
            if (inWater && Math.Abs(YVel) <= 1)
                YVel = Math.Max(YVel - Gravity - .1f, -.75f);

            onGround = this.MoveAdvanced(false);
            
            var blocks = this.CollisionBounds<Solid>(X, Y);

            left = false;
            right = false;
            top = false;
            bottom = false;

            foreach (var block in blocks)
            {
                if (this.CollisionBounds(block, X - 8, Y - 1))
                    left = true;
                if (this.CollisionBounds(block, X + 8, Y - 1))
                    right = true;
                if (this.CollisionBounds(block, X, Y - 1))
                    top = true;
                if (this.CollisionBounds(block, X, Y + 2))
                    bottom = true;
            }

            if (right) Move(-1, 0);
            if (left) Move(1, 0);
            if (top) Move(0, 1);
            if (bottom) Move(0, -1);
            
            free = !left && !right && !top && !bottom;

            if (!free) HP = Math.Max(HP - 1, 0);

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
            
            drawScale += (scale - drawScale) / 40;

            Scale = new Vector2((int)Direction * drawScale * .5f * xs, .5f * drawScale * ys);

            Visible = true;

            if (this.IsOutsideCurrentRoom(1 * Globals.T))
                Destroy();
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            if (!Visible)
                return;

            //base.Draw(sb, gameTime);
            sb.Draw(AssetManager.EnemySlime[0], Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);

            var faceScale = new Vector2((int)Direction * drawScale * .5f, .5f * drawScale);

            var eyeFrame = AnimationFrame;//1 + Convert.ToInt32(AnimationFrame == 3);

            sb.Draw(AssetManager.EnemySlime[eyeFrame], Position + new Vector2(0, -.5f * (float)Math.Sin(t)), null, Color.White, Angle, DrawOffset, faceScale, SpriteEffects.None, Depth + .0001f);

            AssetManager.DefaultFont.Draw(sb, X, Bottom + Globals.T, $"{HP}", scale:.5f);
        }

        public void OverrideHP(int hp)
        {
            var m = MaxHP;
            HP = hp;
            MaxHP = m;
        }
    }
}
