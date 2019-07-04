using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Leore.Objects.Level.Blocks;
using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Map;
using Leore.Objects.Effects.Emitters;
using Microsoft.Xna.Framework.Graphics;
using Leore.Util;
using Leore.Objects.Effects;
using SPG.Util;
using Leore.Resources;

namespace Leore.Objects.Projectiles
{
    public class FireSpell : GameObject
    {
        private Player player => GameManager.Current.Player;
        private Orb orb => GameManager.Current.Player.Orb;

        private static FireSpell instance;
        
        private float power;
        private float maxPower = 2 * 60;

        private int delay;
        private int maxDelay = 5;

        private float spellVel;

        private double t;

        private SpellLevel level;

        public FireSpell(float x, float y, SpellLevel level) : base(x, y)
        {

            this.level = level;

            new CrimsonBurstEmitter(orb.X, orb.Y) { ParticleColors = GameResources.FireColors };

            orb.Visible = false;
            Depth = player.Depth + .0002f;

            switch (level)
            {
                case SpellLevel.ONE:
                    spellVel = 1.7f;
                    break;
                case SpellLevel.TWO:
                    spellVel = 2.3f;
                    break;
                case SpellLevel.THREE:
                    spellVel = 3f;
                    break;
            }
            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            power = Math.Min(power + 1, maxPower);

            t = (t + .5f) % (Math.PI * 2);

            Angle = (Angle + .2f) % (int)(2 * Math.PI);

            var p = power / maxPower;

            Position = player.Position + new Vector2(7 * Math.Sign((int)player.Direction), 0);
            
            delay = Math.Max(delay - 1, 0);

            if (delay == 0)
            {
                var proj = new FireProjectile(X, Y, level);
                proj.XVel = Math.Sign((int)player.Direction) * spellVel * Math.Max(.75f, p) + player.XVel;
                proj.YVel = -.3f * (float)Math.Sin(t) + player.YVel + .5f * (int)player.LookDirection;

                delay = (int)(maxDelay * (1 - .75f * p) * 3);
            }

            if (orb.State != OrbState.ATTACK)
            {
                // TODO

                new CrimsonBurstEmitter(orb.X, orb.Y) { ParticleColors = GameResources.FireColors };

                Destroy();
            }            
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            //sb.Draw(AssetManager.FireBall, Position, null, Color, Angle, new Vector2(16), new Vector2(.5f + .5f * power / maxPower), SpriteEffects.None, Depth + .0001f);

            sb.DrawBar(player.Position + new Vector2(0, -16), 24, power / maxPower, Color.White, Color.Black, orb.Depth + .0001f, 2, false);
        }
        
        public override void Destroy(bool callGC = false)
        {
            orb.Visible = true;
            instance = null;
            base.Destroy(callGC);
        }
        
        public static void Create(float x, float y, SpellLevel level)
        {
            if (instance == null)
                instance = new FireSpell(x, y, level);
        }
    }
}
