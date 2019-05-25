using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Objects.Effects;
using Platformer.Objects.Effects.Emitters;
using Platformer.Objects.Items;
using Platformer.Objects.Level;
using Platformer.Objects.Main;
using Platformer.Objects.Projectiles;
using SPG.Draw;
using SPG.Objects;
using SPG.Util;

namespace Platformer.Objects.Enemies
{
    public abstract class Enemy : Obstacle , IMovable
    {
        public int HP { get; protected set; }
        public int EXP { get; protected set; } = 3;

        public Direction Direction { get; set; }

        public Collider MovingPlatform { get; set; }

        // hit variables
        protected bool hit = false;

        //private float hitCounter;
        //public float HitCounter
        //{
        //    get => hitCounter;
        //    set
        //    {
        //        hitCounter = value;
        //        hitTimeout = maxHitTimeout;
        //    }
        //}

        private int hitPointsReceived;

        int hitTimeout;
        int maxHitTimeout = 30;
        
        Font hitFont;

        public Enemy(float x, float y, Room room) : base(x, y, room)
        {
            Damage = 1;
            HP = 20;

            hitFont = AssetManager.DamageFont.Copy();
            hitFont.Halign = Font.HorizontalAlignment.Center;
            hitFont.Valign = Font.VerticalAlignment.Top;
            hitFont.Color = new Color(255, 0, 0);
        }

        public void Hit(int hitPoints, float angle)
        {
            MovingPlatform = null;

            var hpPrev = HP;

            HP = Math.Max(HP - hitPoints, 0);

            new SingularEffect(X - 4 + (float)(RND.Next * 8), Y - 4 + (float)(RND.Next * 8), 5);

            new FallingFont(X, Y, $"-{hitPoints}", new Color(255, 0, 55), new Color(255, 255, 0));

            var ldx = MathUtil.LengthDirX((float)angle) * .5f;
            var ldy = MathUtil.LengthDirY((float)angle) * .5f;

            XVel += (float)ldx;
            YVel += (float)ldy;

            hit = true;

            hitPointsReceived += hitPoints;
            hitTimeout = maxHitTimeout;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // ++++ getting hit ++++
            
            if (HP > 0)
            {
                var projectile = ObjectManager.CollisionBoundsFirstOrDefault<PlayerProjectile>(this, X, Y);

                if (projectile != null)
                {
                    var vec = Position - (projectile.Position + new Vector2(0, 0));
                    var angle = vec.VectorToAngle();
                    Hit(projectile.Damage, (float)angle);
                    
                    projectile.HandleCollision();
                }                
            }
            else // death
            {
                hitTimeout = 0;
                hitPointsReceived = 0;

                SpellEXP.Spawn(X, Y, EXP);

                new SingularEffect(X, Y);

                if (RND.Next * 100 < 20)
                    new Potion(X, Y, Room, PotionType.HP);

                Destroy();
            }
            
            // ++++ hit counter ++++

            hitTimeout = Math.Max(hitTimeout - 1, 0);
            if (hitTimeout == 0)
            {
                if (hitPointsReceived > 0)
                {
                    //var font = new FollowFont(X, Top - 8, $"-{hitPointsReceived}");
                    //font.Target = this;
                    //font.Color = Color.Red;
                }
                hitPointsReceived = 0;
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

            //if (HitCounter > 0)
            //{
            //    float hitAlpha = hitTimeout / (.5f * maxHitTimeout);
            //    hitFont.Color = new Color(hitFont.Color, hitAlpha);
            //    hitFont.Draw(sb, X, Y - Globals.TILE, $"-{HitCounter}");
            //}
        }
    }
}
