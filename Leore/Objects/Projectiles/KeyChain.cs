using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Leore.Objects.Effects;
using Leore.Objects.Level;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using SPG.Util;

namespace Leore.Objects.Projectiles
{
    public class KeyChain : GameObject
    {
        private static KeyChain instance;

        private Orb orb => GameManager.Current.Player.Orb;
        private Player player => GameManager.Current.Player;

        private float dist;
        private float maxDist = 3 * Globals.TILE;
        private Direction direction;

        private bool headBack;
        private Key key;

        private KeyChain(float x, float y) : base(x, y)
        {
            orb.Visible = false;
            Depth = orb.Depth + .0001f;
            BoundingBox = new RectF(-4, -4, 8, 8);
            DrawOffset = new Vector2(8);
            direction = player.Direction;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Scale = new Vector2((int)direction, 1);

            if (!headBack)
            {
                if (key == null)
                    key = this.CollisionBoundsFirstOrDefault<Key>(X, Y);

                if (key != null)
                {
                    player.Stats.KeysAndKeyblocks.Add(key.ID);
                    player.Stats.HeldKeys++;
                    headBack = true;
                }

                dist = Math.Min(dist + 2f, maxDist);
                if (dist == maxDist)
                    headBack = true;
            }
            else
            {
                dist = Math.Max(dist - 4f, 0);
                if (dist == 0)
                {
                    direction = player.Direction;
                    if (orb.State != OrbState.ATTACK)
                        Destroy();
                }
            }

            Position = orb.Position + new Vector2((int)direction *  dist, 0);

            if (orb.State != OrbState.ATTACK || player.Direction != direction)
            {
                headBack = true;
                //Destroy();
            }
        }

        public override void Destroy(bool callGC = false)
        {
            //if (!orb.Visible)
            //    new CrimsonBurstEmitter(X, Y).ParticleColors = new List<Color> { Color.White };
            orb.Visible = true;
            instance = null;
            base.Destroy(callGC);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            for (var i = 0; i < dist; i+= 4)
            {
                sb.Draw(AssetManager.Projectiles[7], orb.Position + new Vector2((int)direction * i, 0), null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
            }
            
            sb.Draw(AssetManager.Projectiles[8], Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
        }
        
        public static void Create(float x, float y)
        {
            if (instance == null)
            {
                new SingularEffect(x, y, 6);
                instance = new KeyChain(x, y);
            }
        }
    }
}
