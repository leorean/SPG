using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using SPG.Draw;
using Leore.Objects.Effects.Emitters;
using SPG;
using Leore.Resources;
using Leore.Main;

namespace Leore.Objects.Projectiles
{
    public class CrimsonBurstEmitter :SaveBurstEmitter
    {
        public CrimsonBurstEmitter(float x, float y) : base(x, y)
        {
            ParticleColors = GameResources.CrimsonColors;
        }
    }

    public class CrimsonSpell : SpellObject
    {
        private Orb orb;

        float power = 0;
        float maxPower = 0;

        private int intLevel;

        public CrimsonSpell(Orb orb) : base(orb.X, orb.Y, orb.Level)
        {
            Parent = orb;
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

            //level = Math.Min(MathUtil.Div(power, 30), (int)orb.Level - 1);
            intLevel = (int)orb.Level - 1;

            power = Math.Min(power + 1, maxPower);

            Angle = (float)MathUtil.VectorToAngle(new Vector2(X - orb.Parent.X, Y - orb.Parent.Y), true);

            if (orb.State != OrbState.ATTACK)
            {
                //if (power > 5)
                {
                    var degAngle = MathUtil.VectorToAngle(new Vector2(orb.TargetPosition.X  - orb.Parent.X, orb.TargetPosition.Y - orb.Parent.Y));

                    //SpawnProjectile(degAngle);

                    var x = orb.Parent.X;
                    var y = orb.Parent.Y;

                    switch (intLevel)
                    {
                        case 0:
                            SpawnProjectile(x, y, degAngle);
                            break;

                        case 1:
                            SpawnProjectile(x + 2 * (float)MathUtil.LengthDirX(degAngle), y + 2 * (float)MathUtil.LengthDirY(degAngle), degAngle - 5);
                            SpawnProjectile(x - 2 * (float)MathUtil.LengthDirX(degAngle), y - 2 * (float)MathUtil.LengthDirY(degAngle), degAngle + 5);                            
                            break;
                        case 2:
                            SpawnProjectile(x + 2 * (float)MathUtil.LengthDirX(degAngle), y + 2 * (float)MathUtil.LengthDirY(degAngle), degAngle - 5);
                            SpawnProjectile(x + 0 * (float)MathUtil.LengthDirX(degAngle), y + 0 * (float)MathUtil.LengthDirY(degAngle), degAngle);
                            SpawnProjectile(x - 2 * (float)MathUtil.LengthDirX(degAngle), y - 2 * (float)MathUtil.LengthDirY(degAngle), degAngle + 5);                            
                            break;
                    }
                }

                new CrimsonBurstEmitter(X, Y);

                Parent = null;
                Destroy();
            }

            //if (this.IsOutsideCurrentRoom(2 * Globals.T))
            //    Destroy();
        }

        public override void Destroy(bool callGC = false)
        {
            //Parent = null;
            orb.Visible = true;
            base.Destroy(callGC);            
        }

        private void SpawnProjectile(float x, float y, double degAngle, float vel = 4)
        {
            var crimsonProj = new CrimsonProjectile(x, y, orb.Level);
            crimsonProj.Texture = Texture = AssetManager.Projectiles[4];
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
                    var colors = GameResources.CrimsonColors;
                    var colorIndex = RND.Int(colors.Count - 1);
                    emitter.Color = colors[colorIndex];
                    emitter.XVel = rel * -(float)MathUtil.LengthDirX(MathUtil.RadToDeg(Angle));
                    emitter.YVel = rel * -(float)MathUtil.LengthDirY(MathUtil.RadToDeg(Angle));
                }
                sb.DrawLine(Position.X + left.X - .5f, Position.Y + left.Y - .5f, Position.X + recoil.X + start.X - .5f, Position.Y + recoil.Y + start.Y - .5f, col, Depth - .0002f);
                sb.DrawLine(Position.X + right.X - .5f, Position.Y + right.Y - .5f, Position.X + recoil.X + start.X - .5f, Position.Y + recoil.Y + start.Y - .5f, col, Depth - .0002f);

                // arrow

                var arrow = AssetManager.Projectiles[4 + intLevel];
                sb.Draw(arrow, Position + recoil, null, Color, Angle + (float)MathUtil.DegToRad(45), DrawOffset, Scale, SpriteEffects.None, Depth + .0001f);                
            }
        }
    }
}
