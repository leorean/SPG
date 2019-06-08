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

namespace Leore.Objects.Enemies
{
    public abstract class Enemy : Obstacle , IMovable
    {
        public int HP { get; protected set; }
        public int EXP { get; protected set; } = 3;

        public Direction Direction { get; set; }

        public Collider MovingPlatform { get; set; }

        // hit variables
        protected bool hit = false;
        protected float knockback = .5f;

        Font hitFont;

        public Enemy(float x, float y, Room room) : base(x, y, room)
        {
            Damage = 1;
            HP = 20;

            Depth = Globals.LAYER_ENEMY;

            hitFont = AssetManager.DamageFont.Copy();
            hitFont.Halign = Font.HorizontalAlignment.Center;
            hitFont.Valign = Font.VerticalAlignment.Top;
            hitFont.Color = new Color(255, 0, 0);
        }

        public virtual void Hit(int hitPoints, float degAngle)
        {
            MovingPlatform = null;

            var hpPrev = HP;

            HP = Math.Max(HP - hitPoints, 0);

            new SingularEffect(X - 4 + (float)(RND.Next * 8), Y - 4 + (float)(RND.Next * 8), 5);

            new FallingFont(X, Y, $"-{hitPoints}", new Color(255, 0, 55), new Color(255, 255, 0));

            var ldx = MathUtil.LengthDirX((float)degAngle) * knockback;
            var ldy = MathUtil.LengthDirY((float)degAngle) * knockback;

            XVel += (float)ldx;
            YVel += (float)ldy;

            hit = true;            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (GameManager.Current.NonRespawnableIDs.Contains(ID))
            {
                Destroy();
                return;
            }

            // ++++ getting hit ++++
            
            if (HP > 0)
            {
                var projectile = ObjectManager.CollisionBoundsFirstOrDefault<PlayerProjectile>(this, X, Y);

                if (projectile != null)
                {
                    var vec = Position - (projectile.Position + new Vector2(0, 0));
                    var angle = vec.VectorToAngle();
                    Hit(projectile.Damage, (float)angle);
                    
                    projectile.HandleCollision(this);
                }                
            }
            else // death
            {
                if (GameManager.Current.Player.Stats.Abilities.HasFlag(PlayerAbility.ORB))
                    SpellEXP.Spawn(X, Y, EXP);

                new SingularEffect(X, Y);

                //if (RND.Next * 100 < 20)
                //    new Potion(X, Y, Room, PotionType.HP);

                GameManager.Current.NonRespawnableIDs.Add(ID);

                Destroy();
            }            
        }

        public override void EndUpdate(GameTime gameTime)
        {
            base.EndUpdate(gameTime);

            hit = false;
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);            
        }
    }
}
