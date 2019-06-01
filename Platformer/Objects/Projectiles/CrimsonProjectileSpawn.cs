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
using SPG.Draw;
using Platformer.Objects.Effects.Emitters;
using SPG;

namespace Platformer.Objects.Projectiles
{
    public class CrimsonBurstEmitter :SaveBurstEmitter
    {
        public CrimsonBurstEmitter(float x, float y) : base(x, y)
        {
            ParticleColors = new List<Color>()
            {
                Colors.FromHex("c80e1f"),
                Colors.FromHex("454545"),
                Colors.FromHex("aa21a3")

            };
        }
    }

    public class CrimsonProjectileSpawn : GameObject
    {
        private Orb orb;

        float power = 0;
        float maxPower = 0;

        private int level;

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
            Texture = AssetManager.Projectiles[3];
            orb.Visible = false;

            maxPower = 30 * (int)orb.Level;

            level = Math.Min(MathUtil.Div(power, 30), (int)orb.Level - 1);

            power = Math.Min(power + 1, maxPower);

            Angle = (float)MathUtil.VectorToAngle(new Vector2(X - orb.Parent.X, Y - orb.Parent.Y), true);

            if (orb.State != OrbState.ATTACK || GameManager.Current.Player.MP < orb.MpCost[SpellType.CRIMSON][orb.Level])
            {
                if (power > 5)
                {
                    var degAngle = MathUtil.VectorToAngle(new Vector2(X - orb.Parent.X, Y - orb.Parent.Y));
                    
                    switch (level)
                    {
                        case 0:
                            SpawnProjectile(degAngle);
                            break;

                        case 1:
                            SpawnProjectile(degAngle);
                            SpawnProjectile(degAngle, 4.5f);
                            break;

                        case 2:
                            SpawnProjectile(degAngle);
                            SpawnProjectile(degAngle, 4.5f);
                            SpawnProjectile(degAngle, 5f);
                            break;

                    }
                }

                new CrimsonBurstEmitter(X, Y);

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
            crimsonProj.Texture = Texture = AssetManager.Projectiles[3 + (int)(orb.Level)];
            crimsonProj.Depth = Depth;

            var coilX = (float)MathUtil.LengthDirX(degAngle);
            var coilY = (float)MathUtil.LengthDirY(degAngle);

            float rel = power / maxPower;

            crimsonProj.XVel = coilX * Math.Max(rel, .65f) * vel;
            crimsonProj.YVel = coilY * Math.Max(rel, .65f) * vel;
        }


        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //base.Draw(sb, gameTime);

            if (Texture != null)
            {
                sb.Draw(Texture, Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
                //var arrow = AssetManager.Projectiles[3 + (int)(orb.Level)];

                var arrow = AssetManager.Projectiles[4 + level];
                
                var f = 8 - power / maxPower * 8;
                var recoil = new Vector2(f * (float)MathUtil.LengthDirX(MathUtil.RadToDeg(Angle)), f * (float)MathUtil.LengthDirY(MathUtil.RadToDeg(Angle)));

                // bow-string

                var start = new Vector2((0 - 8) * (float)MathUtil.LengthDirX(MathUtil.RadToDeg(Angle)), (0 - 8) * (float)MathUtil.LengthDirY(MathUtil.RadToDeg(Angle)));
                var left = new Vector2(8 * (float)MathUtil.LengthDirX(MathUtil.RadToDeg(Angle) - 90), 8 * (float)MathUtil.LengthDirY(MathUtil.RadToDeg(Angle) - 90));
                var right = new Vector2(8 * (float)MathUtil.LengthDirX(MathUtil.RadToDeg(Angle) + 90), 8 * (float)MathUtil.LengthDirY(MathUtil.RadToDeg(Angle) + 90));

                sb.DrawLine(Position.X + left.X - .5f, Position.Y + left.Y - .5f, Position.X + recoil.X + start.X - .5f, Position.Y + recoil.Y + start.Y - .5f, Color.White, Depth + .0002f);
                sb.DrawLine(Position.X + right.X - .5f, Position.Y + right.Y - .5f, Position.X + recoil.X + start.X - .5f, Position.Y + recoil.Y + start.Y - .5f, Color.White, Depth + .0002f);

                // arrow

                sb.Draw(arrow, Position + recoil, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth + .0001f);

            }
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
