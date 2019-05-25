using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Main;
using Platformer.Objects.Effects.Emitters;
using Platformer.Objects.Level;
using Platformer.Objects.Main;
using Platformer.Objects.Main.Orbs;
using SPG.Map;
using SPG.Objects;
using SPG.Util;

namespace Platformer.Objects.Projectiles
{
    public class StarProjectile : PlayerProjectile
    {
        private Vector2 origin;
        private int maxDist;
        
        public StarProjectile(float x, float y, SpellLevel level) : base(x, y)
        {            
            BoundingBox = new RectF(-2, -2, 4, 4);
            DrawOffset = new Vector2(8, 8);

            switch (level)
            {
                case SpellLevel.ONE:
                    Texture = AssetManager.Projectiles[0];
                    maxDist = 3 * Globals.TILE;
                    Damage = 1;
                    break;
                case SpellLevel.TWO:
                    Texture = AssetManager.Projectiles[1];
                    maxDist = 5 * Globals.TILE;
                    Damage = 2;
                    break;
                case SpellLevel.THREE:
                    Texture = AssetManager.Projectiles[2];
                    maxDist = 8 * Globals.TILE;
                    Damage = 1;
                    break;
            }
            
            origin = Position;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Angle = (float)MathUtil.VectorToAngle(new Vector2(XVel, YVel), true);

            Move(XVel, YVel);

            var solid = GameManager.Current.Map.CollisionTile(X, Y) || GameManager.Current.Map.CollisionTile(X, Y, GameMap.WATER_INDEX);

            if (!solid)
            {
                solid = ObjectManager.CollisionPointFirstOrDefault<Solid>(X, Y) != null;
            }

            var tooFar = MathUtil.Euclidean(Position, origin) > maxDist;
            
            if (solid || tooFar)
            {
                HandleCollision();
            }
        }
        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //base.Draw(sb, gameTime);
            sb.Draw(Texture, Position - new Vector2(2f * XVel, 2f * YVel), null, new Color(Color, .4f), Angle, DrawOffset, Scale, SpriteEffects.None, Depth - .0002f);
            sb.Draw(Texture, Position - new Vector2(1f * XVel, 1f * YVel), null, new Color(Color, .7f), Angle, DrawOffset, Scale, SpriteEffects.None, Depth - .0001f);
            sb.Draw(Texture, Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
        }

        public override void HandleCollision()
        {
            //new Effects.SingularEffect(X, Y, 3);
            new StarEmitter(X, Y, 2, 0);
            Destroy();
        }
    }
}
