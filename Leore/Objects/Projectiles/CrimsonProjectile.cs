using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Main;
using Leore.Objects.Effects;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Level;
using SPG.Map;
using SPG.Objects;
using SPG.Util;

namespace Leore.Objects.Projectiles
{
    public class CrimsonProjectile : PlayerProjectile
    {
        public CrimsonProjectile(float x, float y, SpellLevel level) : base(x, y, level)
        {
            BoundingBox = new SPG.Util.RectF(-2, -2, 4, 4);
            DrawOffset = new Vector2(8);
            
            switch (level)
            {
                case SpellLevel.ONE:
                    Damage = 1;
                    break;
                case SpellLevel.TWO:
                    Damage = 1;
                    break;
                case SpellLevel.THREE:
                    Damage = 2;
                    break;
            }

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            YVel += .06f;

            var emitter = new SingleParticleEmitter(X, Y);
            var colors = CrimsonBurstEmitter.CrimsonColors;
            var colorIndex = RND.Int(colors.Count - 1);
            emitter.Color = colors[colorIndex];
            

            var solid = GameManager.Current.Map.CollisionTile(X - XVel, Y - YVel) || GameManager.Current.Map.CollisionTile(X - XVel, Y - YVel, GameMap.WATER_INDEX);
            if (!solid)
            {
                solid = ObjectManager.CollisionPointFirstOrDefault<Solid>(X - XVel, Y - YVel) != null;
            }

            if (solid)
            {
                new SingularEffect(X, Y, 3);
                Destroy();
            }

            /*switch (level)
            {
                case SpellLevel.ONE:
                    if (solid)
                        Destroy();
                    break;
            }*/

            Angle = (float)MathUtil.VectorToAngle(new Vector2(XVel, YVel), true) + (float)MathUtil.DegToRad(45);

            Move(XVel, YVel);

            if (this.IsOutsideCurrentRoom())
                Destroy();
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);
            //sb.Draw(AssetManager.Projectiles[6 + (int)level], Position, null, Color, Angle - (float)MathUtil.DegToRad(45), DrawOffset, Scale, SpriteEffects.None, Depth - .0001f);
        }

        public override void HandleCollision(GameObject obj)
        {
            if (obj is Enemies.EnemyVoidling.Shield)
            {
                new SingularEffect(X, Y, 3);
                Destroy();
            }
            //throw new NotImplementedException();
        }
    }
}
