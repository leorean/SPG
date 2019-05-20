﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Platformer.Main;
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
                    Texture = AssetManager.Projectiles[0];
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
                new Effects.SingularEffect(X, Y, 3);
                Destroy();
            }
        }
    }
}
