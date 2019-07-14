using Leore.Main;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPG.Map;
using SPG.Objects;
using Leore.Resources;
using Leore.Objects.Effects;

namespace Leore.Objects.Enemies
{
    public class EnemySlurp : Enemy
    {
        public int ShotDelay { get; set; } = 90;
        private int shot;

        public EnemySlurp(float x, float y, Room room) : base(x, y, room)
        {
            Damage = GameResources.EnemySlurp.Damage;
            EXP = GameResources.EnemySlurp.EXP;
            HP = GameResources.EnemySlurp.HP;
            
            DrawOffset = new Vector2(8);
            BoundingBox = new SPG.Util.RectF(-4, -4, 8, 8);

            AnimationTexture = AssetManager.EnemySlurp;

            SetAnimation(0, 3, .1f, true);

            shot = ShotDelay;

            Direction = Direction.NONE;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Direction == Direction.NONE)
            {
                if (GameManager.Current.Map.CollisionTile(X - Globals.T, Y))
                    Direction = Direction.RIGHT;
                if (GameManager.Current.Map.CollisionTile(X + Globals.T, Y))
                    Direction = Direction.LEFT;
            }

            shot = Math.Max(shot - 1, 0);

            if (shot == 0)
            {
                shot = ShotDelay;
                new SingularEffect(X, Y, 7);
                new DefaultEnemyProjectile(X, Y) { XVel = 2 * (int)Direction, YVel = 0 };
            }

            Scale = new Vector2((int)Direction, 1);
        }
    }
}
