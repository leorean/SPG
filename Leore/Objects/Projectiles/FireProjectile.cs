using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using SPG.Map;

namespace Leore.Objects.Projectiles
{
    public class FireProjectile : PlayerProjectile
    {
        private bool dead;

        private Player player => GameManager.Current.Player;
        private Orb orb => GameManager.Current.Player.Orb;

        public FireProjectile(float x, float y, SpellLevel level) : base(x, y, level)
        {
            Depth = player.Depth + .0002f;

            Texture = AssetManager.Projectiles[10];
            
            DrawOffset = new Vector2(8);
            BoundingBox = new SPG.Util.RectF(-3, -3, 6, 6);

            DebugEnabled = true;            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            XVel *= .93f;

            if (Math.Abs(XVel) < .1f)
                Destroy();

            var xCol = GameManager.Current.Map.CollisionTile(X + XVel, Y);
            var yCol = GameManager.Current.Map.CollisionTile(X, Y + YVel);

            if (!xCol)
                Move(XVel, 0);
            else
            {
                Destroy();
                //XVel = -XVel;
            }

            if (!yCol)
            {
                Move(0, YVel);
            }
            else
            {
                Destroy();
            }
        }

        public override void Destroy(bool callGC = false)
        {
            base.Destroy(callGC);

            if (!dead)
            {
                dead = true;

                // TODO
            }
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

        }

        public override void HandleCollision(GameObject obj)
        {
            //
        }
    }
}
