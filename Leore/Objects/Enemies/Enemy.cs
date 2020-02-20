using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Main;
using Leore.Objects.Effects;
using Leore.Objects.Items;
using Leore.Objects.Level;
using Leore.Objects.Projectiles;
using SPG.Draw;
using SPG.Objects;
using SPG.Util;
using Leore.Objects.Level.Obstacles;

namespace Leore.Objects.Enemies
{
    public abstract class Enemy : Obstacle , IMovable
    {
        public int MaxHP { get; protected set; }
        private int hp;
        public int HP {
            get
            {
                return hp;
            }protected set
            {
                MaxHP = Math.Max(value, MaxHP);
                hp = value;
            }
        }
        public int EXP { get; set; } = 3;

        public bool Respawn { get; set; } = false;

        public Direction Direction { get; set; }

        public Collider MovingPlatform { get; set; }
        
        private bool onDeathCalled;

        // hit variables
        protected bool hit = false;
        protected float knockback = .5f;

        Font hitFont;

        public bool IgnoreProjectiles { get; set; }

        public Enemy(float x, float y, Room room) : base(x, y, room)
        {
            HP = 1;

            Depth = Globals.LAYER_ENEMY;

            hitFont = AssetManager.DamageFont.Copy();
            hitFont.Halign = Font.HorizontalAlignment.Center;
            hitFont.Valign = Font.VerticalAlignment.Top;
            hitFont.Color = new Color(255, 0, 0);
        }

        /// <summary>
        /// Hits the enemy with an object (playerprojectile) or simply a damage number in a certain angle.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hitObj"></param>
        /// <param name="degAngle"></param>
        public virtual void Hit<T>(T hitObj, float degAngle)
        {
            int hitPoints = 0;

            if (hitObj is int h)
            {
                hitPoints = h;
            }

            if (hitObj is PlayerProjectile projectile)
            {
                hitPoints = projectile.Damage;
            }

            if (hitPoints == 0)
                return;

            MovingPlatform = null;
                        
            var hpPrev = HP;

            HP = Math.Max(HP - hitPoints, 0);
            
            new FallingFont(Center.X, Center.Y, $"-{hitPoints}", new Color(255, 0, 55), new Color(255, 255, 0));

            var ldx = MathUtil.LengthDirX((float)degAngle) * knockback;
            var ldy = MathUtil.LengthDirY((float)degAngle) * knockback;

            XVel += (float)ldx;
            YVel += (float)ldy;

            hit = true;            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!Respawn && GameManager.Current.NonRespawnableIDs.Contains(ID))
            {
                Destroy();
                return;
            }

            // ++++ getting hit ++++
            
            if (HP > 0)
            {
                var obstacle = this.CollisionBoundsFirstOrDefault<Obstacle>(X, Y);

                if (!(this is Boss) && (obstacle is Lava || obstacle is BigSpike))
                {
                    Hit(999, 90);
                }

                if (!IgnoreProjectiles)
                {
                    var projectile = this.CollisionBoundsFirstOrDefault<PlayerProjectile>(X, Y);

                    if (projectile != null)
                    {
                        var vec = Position - (projectile.Position + new Vector2(0, 0));
                        var angle = vec.VectorToAngle();
                        Hit(projectile, (float)angle);

                        projectile.HandleCollision(this);
                    }
                }
            }
            else // death
            {
                if (!onDeathCalled)
                {
                    OnDeath();
                    Destroy();
                }
                onDeathCalled = true;
            }
        }

        public virtual void OnDeath()
        {
            if (GameManager.Current.Player.Stats.Abilities.HasFlag(PlayerAbility.ORB))
                SpellEXP.Spawn(X, Y, EXP);

            new SingularEffect(X, Y);

            if (!Respawn)
                GameManager.Current.NonRespawnableIDs.Add(ID);            
        }

        public override void EndUpdate(GameTime gameTime)
        {
            base.EndUpdate(gameTime);

            hit = false;
        }
    }
}
