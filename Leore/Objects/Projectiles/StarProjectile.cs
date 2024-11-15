﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Main;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Level;
using SPG.Map;
using SPG.Objects;
using SPG.Util;

namespace Leore.Objects.Projectiles
{
    public class StarProjectile : PlayerProjectile
    {
        private Vector2 origin;
        private int maxDist;

        private LightSource light;

        private Orb orb => GameManager.Current.Player.Orb;

        public StarProjectile(float x, float y, SpellLevel level) : base(x, y, level)
        {            
            BoundingBox = new RectF(-2, -2, 4, 4);
            DrawOffset = new Vector2(8, 8);

            switch (level)
            {
                case SpellLevel.ONE:
                    Texture = AssetManager.Projectiles[0];
                    maxDist = 3 * Globals.T;
                    Damage = 1;
                    orb.Cooldown = 15;
                    break;
                case SpellLevel.TWO:
                    Texture = AssetManager.Projectiles[1];
                    maxDist = 5 * Globals.T;
                    Damage = 2;
                    orb.Cooldown = 15;
                    break;
                case SpellLevel.THREE:
                    Texture = AssetManager.Projectiles[2];
                    maxDist = 8 * Globals.T;
                    Damage = 1;
                    orb.Cooldown = 8;
                    break;
            }

            light = new LightSource(this) { Active = true, Scale = new Vector2(.45f) };

            origin = Position;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Angle = (float)MathUtil.VectorToAngle(new Vector2(XVel, YVel), true);

            Move(XVel, YVel);

            var solid = GameManager.Current.Map.CollisionTile(X, Y);

            if (!solid)
            {
                solid = ObjectManager.CollisionPointFirstOrDefault<Solid>(X, Y) != null;
            }

            var tooFar = MathUtil.Euclidean(Position, origin) > maxDist;
            
            if (solid || tooFar)
            {
                HandleCollision(null);
            }
        }
        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //base.Draw(sb, gameTime);
            sb.Draw(Texture, Position - new Vector2(2f * XVel, 2f * YVel), null, new Color(Color, .4f), Angle, DrawOffset, Scale, SpriteEffects.None, Depth - .0002f);
            sb.Draw(Texture, Position - new Vector2(1f * XVel, 1f * YVel), null, new Color(Color, .7f), Angle, DrawOffset, Scale, SpriteEffects.None, Depth - .0001f);
            sb.Draw(Texture, Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
        }

        public override void HandleCollision(GameObject obj)
        {
            new StarEmitter(X, Y, 2, 0);
            Destroy();
        }

        public override void Destroy(bool callGC = false)
        {
            FadeOutLight.Create(this, light.Scale);

            base.Destroy(callGC);
        }
    }
}
