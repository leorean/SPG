using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Main;
using Leore.Objects.Effects;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Level;
using SPG.Map;
using SPG.Objects;
using SPG.Util;
using Leore.Resources;
using Leore.Objects.Level.Blocks;

namespace Leore.Objects.Projectiles
{
    public class CrimsonProjectile : PlayerProjectile
    {
        private int pierce;

        public CrimsonProjectile(float x, float y, SpellLevel level) : base(x, y, level)
        {
            BoundingBox = new SPG.Util.RectF(-2, -2, 4, 4);
            DrawOffset = new Vector2(8);
            
            switch (level)
            {
                case SpellLevel.ONE:
                    pierce = 2;
                    Damage = 3;
                    break;
                case SpellLevel.TWO:
                    pierce = 3;
                    Damage = 2;
                    break;
                case SpellLevel.THREE:
                    pierce = 5;
                    Damage = 1;
                    break;
            }

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            YVel += .06f;

            var emitter = new SingleParticleEmitter(X, Y);
            var colors = GameResources.CrimsonColors;
            var colorIndex = RND.Int(colors.Count - 1);
            emitter.Color = colors[colorIndex];
            
            if (GameManager.Current.Map.CollisionTile(X - XVel, Y - YVel, GameMap.WATER_INDEX))
            {
                XVel *= .9f;
                YVel *= .9f;
            }

            var solid = GameManager.Current.Map.CollisionTile(X - XVel, Y - YVel);// || GameManager.Current.Map.CollisionTile(X - XVel, Y - YVel, GameMap.WATER_INDEX);
            if (!solid)
            {
                solid = ObjectManager.CollisionPointFirstOrDefault<Solid>(X - XVel, Y - YVel) != null;
            }

            if (solid)
            {
                new SingularEffect(X, Y, 3);
                Destroy();
            }
            
            Angle = (float)MathUtil.VectorToAngle(new Vector2(XVel, YVel), true) + (float)MathUtil.DegToRad(45);

            Move(XVel, YVel);

            if (this.IsOutsideCurrentRoom(Globals.T))
                Destroy();
        }
        

        public override void HandleCollision(GameObject obj)
        {
            if (!(obj is DestroyBlock))
                pierce--;
            
            if (pierce == 0 || obj is Enemies.EnemyVoidling.Shield)
            {
                new SingularEffect(X, Y, 3);
                Destroy();
            }            
        }
    }
}
