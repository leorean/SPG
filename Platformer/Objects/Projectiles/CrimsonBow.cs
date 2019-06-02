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
        public static List<Color> CrimsonColors = new List<Color>()
        {
            Colors.FromHex("c80e1f"),
            Colors.FromHex("df6f64"),
            Colors.FromHex("900717")
        };

        public CrimsonBurstEmitter(float x, float y) : base(x, y)
        {
            ParticleColors = CrimsonColors;
        }
    }

    public class CrimsonBow : GameObject
    {
        private Orb orb;

        float power = 0;
        float maxPower = 0;

        private int level;

        public CrimsonBow(Orb orb) : base(orb.X, orb.Y)
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

            if (orb.State != OrbState.ATTACK || GameManager.Current.Player.MP < orb.MpCost[SpellType.CRIMSON_ARC][orb.Level])
            {
                if (power > 5)
                {
                    var degAngle = MathUtil.VectorToAngle(new Vector2(X - orb.Parent.X, Y - orb.Parent.Y));
                    SpawnProjectile(degAngle);

                    //switch (level)
                    //{
                    //    case 0:
                    //        SpawnProjectile(degAngle);
                    //        break;

                    //    case 1:
                    //        SpawnProjectile(degAngle);
                    //        SpawnProjectile(degAngle, 4.5f);
                    //        break;

                    //    case 2:
                    //        SpawnProjectile(degAngle);
                    //        SpawnProjectile(degAngle, 4.5f);
                    //        SpawnProjectile(degAngle, 5f);
                    //        break;
                    //}
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
            var crimsonProj = new CrimsonProjectile(orb.Parent.X, orb.Parent.Y, (SpellLevel)(level + 1));
            crimsonProj.Texture = Texture = AssetManager.Projectiles[4 + level];
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
                // bow

                sb.Draw(Texture, Position, null, Color, Angle + (float)MathUtil.DegToRad(45), DrawOffset, Scale, SpriteEffects.None, Depth);
                
                // bow-string
                var f = 8 - power / maxPower * 8;
                var recoil = new Vector2(f * (float)MathUtil.LengthDirX(MathUtil.RadToDeg(Angle)), f * (float)MathUtil.LengthDirY(MathUtil.RadToDeg(Angle)));

                var start = new Vector2((0 - 8) * (float)MathUtil.LengthDirX(MathUtil.RadToDeg(Angle)), (0 - 8) * (float)MathUtil.LengthDirY(MathUtil.RadToDeg(Angle)));
                var left = new Vector2(9 * (float)MathUtil.LengthDirX(MathUtil.RadToDeg(Angle) - 90), 9 * (float)MathUtil.LengthDirY(MathUtil.RadToDeg(Angle) - 90));
                var right = new Vector2(9 * (float)MathUtil.LengthDirX(MathUtil.RadToDeg(Angle) + 90), 9 * (float)MathUtil.LengthDirY(MathUtil.RadToDeg(Angle) + 90));
                
                float rel = power / maxPower;                
                var col = new Color(rel * 255, 0, 0);

                if (rel * RND.Next * 10 > 8)
                {
                    var emitter = new SingleParticleEmitter(X + recoil.X, Y + recoil.Y);
                    var colors = CrimsonBurstEmitter.CrimsonColors;
                    var colorIndex = RND.Int(colors.Count - 1);
                    emitter.Color = colors[colorIndex];
                    emitter.XVel = rel * -(float)MathUtil.LengthDirX(MathUtil.RadToDeg(Angle));
                    emitter.YVel = rel * -(float)MathUtil.LengthDirY(MathUtil.RadToDeg(Angle));
                }
                sb.DrawLine(Position.X + left.X - .5f, Position.Y + left.Y - .5f, Position.X + recoil.X + start.X - .5f, Position.Y + recoil.Y + start.Y - .5f, col, Depth - .0002f);
                sb.DrawLine(Position.X + right.X - .5f, Position.Y + right.Y - .5f, Position.X + recoil.X + start.X - .5f, Position.Y + recoil.Y + start.Y - .5f, col, Depth - .0002f);

                // arrow

                var arrow = AssetManager.Projectiles[4 + level];
                sb.Draw(arrow, Position + recoil, null, Color, Angle + (float)MathUtil.DegToRad(45), DrawOffset, Scale, SpriteEffects.None, Depth + .0001f);                
            }
        }
    }
}
