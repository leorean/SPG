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

        private TorchEmitter torchEmitter;

        private float power;
        private float maxPower = 2 * 60;

        private float delay;
        private float maxDelay;
        private float maxMaxDelay;

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
                    maxMaxDelay = 20;
                    break;
                case SpellLevel.TWO:
                    maxMaxDelay = 15;
                    break;
                case SpellLevel.THREE:
                    maxMaxDelay = 10;
                    break;
            }


            torchEmitter = new TorchEmitter(X, Y);            
            torchEmitter.XRange = 8;
            torchEmitter.YRange = 8;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            power = Math.Min(power + 1, maxPower);

            var ratio = power / maxPower;

            Position = player.Position + new Vector2(7 * Math.Sign((int)player.Direction), 0);

            torchEmitter.Position = Position;// + new Vector2(player.XVel, player.YVel);

            torchEmitter.SpawnRate = 2 + (int)(10 * ratio);
            torchEmitter.Scale = new Vector2(.5f) + new Vector2(.75f * ratio);

            maxDelay = maxMaxDelay - .5f * maxMaxDelay * ratio;
            delay = Math.Min(delay, maxDelay);

            delay = Math.Max(delay - 1, 0);
            if (delay == 0)
            {
                var proj = new FireProjectile(X, Y, level);
                proj.XVel = (2 + 4 * ratio) * Math.Sign((int)player.Direction);
                proj.YVel = 0;// (float)(-.2f + RND.Next * .4f);

                delay = maxDelay;
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

            //sb.DrawBar(orb.Position + new Vector2(0, -8), 24, power / maxPower, Color.White, Color.Black, orb.Depth + .0001f, 2, false);
        }
        
        public override void Destroy(bool callGC = false)
        {
            torchEmitter.Kill();
            
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
