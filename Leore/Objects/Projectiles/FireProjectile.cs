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
using Leore.Objects.Effects.Emitters;

namespace Leore.Objects.Projectiles
{
    public class FireProjectile : PlayerProjectile
    {
        private bool dead;

        private Player player => GameManager.Current.Player;
        private Orb orb => GameManager.Current.Player.Orb;

        private TorchEmitter torchEmitter;

        public FireProjectile(float x, float y, SpellLevel level) : base(x, y, level)
        {
            Depth = player.Depth + .0002f;
            
            Texture = AssetManager.Projectiles[10];
            Scale = new Vector2(.5f * (int)player.Direction, .5f);

            DrawOffset = new Vector2(8);
            BoundingBox = new SPG.Util.RectF(-3, -3, 6, 6);
            
            torchEmitter = new TorchEmitter(X, Y);
            torchEmitter.XRange = 8;
            torchEmitter.YRange = 8;
            torchEmitter.SpawnRate = 20;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            torchEmitter.Position = Position;
            torchEmitter.SpawnRate = 1;
            torchEmitter.Scale = new Vector2(.75f);

            XVel *= .96f;
            YVel *= .96f;

            if (Math.Abs(XVel) < .1f)
                Destroy();

            var xCol = GameManager.Current.Map.CollisionTile(X + XVel, Y);
            var yCol = GameManager.Current.Map.CollisionTile(X, Y + YVel);

            if (!xCol)
                Move(XVel, 0);
            else
            {
                XVel = -XVel * .5f;                
            }

            if (!yCol)
            {
                YVel += Gravity;
                Move(0, YVel);
            }
            else
            {
                YVel = Math.Max(YVel * -.75f, -2.5f);

                if (Math.Abs(YVel) < .5f)
                    Destroy();
            }

            if (Math.Max(Math.Abs(XVel), Math.Abs(YVel)) < .5f)
                Destroy();
        }

        public override void Destroy(bool callGC = false)
        {
            base.Destroy(callGC);

            torchEmitter.Kill();

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
            Destroy();
        }
    }
}
