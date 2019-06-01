using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Objects.Main;
using Platformer.Objects.Main.Orbs;
using Platformer.Util;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Projectiles
{
    public class CrimsonProjectileSpawn : GameObject
    {
        private Orb orb;

        float power = 0;
        float maxPower = 60;

        public CrimsonProjectileSpawn(Orb orb) : base(orb.X, orb.Y)
        {
            this.orb = orb;
            DrawOffset = new Vector2(8);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Position = orb.Position;
            
            Depth = orb.Depth + .0001f;
            Texture = AssetManager.Projectiles[3 + (int)(orb.Level) - 1];
            orb.Visible = false;

            power = Math.Min(power + 1, maxPower);

            Angle = (float)MathUtil.VectorToAngle(new Vector2(X - orb.Parent.X, Y - orb.Parent.Y), true);

            if (orb.State != OrbState.ATTACK || GameManager.Current.Player.MP < orb.MpCost[SpellType.CRIMSON][orb.Level])
            {
                if (power > 20)
                {
                    var degAngle = MathUtil.VectorToAngle(new Vector2(X - orb.Parent.X, Y - orb.Parent.Y));

                    switch (orb.Level)
                    {
                        case SpellLevel.ONE:
                            SpawnProjectile(degAngle);
                            break;

                        case SpellLevel.TWO:
                            SpawnProjectile(degAngle);
                            SpawnProjectile(degAngle, 4.5f);
                            break;

                        case SpellLevel.THREE:
                            SpawnProjectile(degAngle);
                            SpawnProjectile(degAngle, 4.5f);
                            SpawnProjectile(degAngle, 5f);
                            break;

                    }
                }
                Destroy();
            }

            if (this.IsOutsideCurrentRoom())
                Destroy();
        }

        public override void Destroy(bool callGC = false)
        {
            orb.Visible = true;
            base.Destroy(callGC);            
        }

        private void SpawnProjectile(double degAngle, float vel = 4)
        {
            var crimsonProj = new CrimsonProjectile(orb.Parent.X, orb.Parent.Y, orb.Level);
            crimsonProj.Texture = Texture;
            crimsonProj.Depth = Depth;

            var coilX = (float)MathUtil.LengthDirX(degAngle);
            var coilY = (float)MathUtil.LengthDirY(degAngle);

            float rel = power / maxPower;

            crimsonProj.XVel = coilX * Math.Max(rel, .65f) * vel;
            crimsonProj.YVel = coilY * Math.Max(rel, .65f) * vel;
        }


        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            //sb.Draw(Texture, Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);

            float rel = power / maxPower;

            var r = (int)(255);
            var g = 0 + (int)(rel * 255);
            var b = (int)(55 - rel * 55);

            //new Color(255, 0, 55), new Color(255, 255, 0)

            if (rel >= .25f)
                sb.DrawBar(Position + new Vector2(0, -12), 16, rel, new Color(r, g, b), Color.Black, orb.Depth + 0.0001f, 1, false);
        }
    }
}
