using Microsoft.Xna.Framework;
using Leore.Main;
using SPG.Objects;
using SPG.Util;
using System;

namespace Leore.Objects.Effects.Emitters
{
    public class SpellParticle : Particle
    {
        private GameObject target;

        public SpellParticle(ParticleEmitter emitter, GameObject target) : base(emitter)
        {
            LifeTime = 60;
            this.target = target;

            Position = new Vector2(emitter.Position.X - 4 + (float)RND.Next * 8, emitter.Position.Y - 4 + (float)RND.Next * 8);

            Depth = Globals.LAYER_PLAYER + .00005f;
            Scale = new Vector2(1.5f + (float)RND.Next * .5f);
            Alpha = 0f;
            Angle = (float)(RND.Next * 2 * Math.PI);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Alpha = Math.Min(Alpha + .3f, 1f);

            var player = Emitter.Parent?.Parent as Player;

            var tx = target.X - 4 + (float)RND.Next * 8;
            var ty = target.Y - 4 + (float)RND.Next * 8;

            XVel = (tx - Position.X) / 6f + ((player != null) ? player.XVel : 0);
            YVel = (ty - Position.Y) / 6f + ((player != null) ? player.YVel : 0);

            if (MathUtil.Euclidean(Position, target.Position) < 2)
            {
                //var part = new StarParticle(Emitter);
                //part.Position = Position;
                //part.Scale = Vector2.One;
                //part.DrawOffset = Vector2.Zero;
                //part.Depth = Depth;
                //part.XVel *= .5f;
                //part.YVel *= .5f;

                //if (RND.Next * 100 < 20)
                //{
                //    var part = new PlayerLevitationParticle(Emitter);
                //    part.XVel = -.1f + (float)RND.Next * .2f;
                //    part.YVel = -.1f + (float)RND.Next * .2f;
                //    part.Position = Position + new Vector2((float)MathUtil.LengthDirX(part.Angle) * 4, (float)MathUtil.LengthDirY(part.Angle) * 4);
                //    part.Depth = Depth;
                //}
                LifeTime = 0;
            }
        }
    }

    public class SpellEmitter : ParticleEmitter
    {
        private GameObject target;
        public SpellEmitter(float x, float y, GameObject target) : base(x, y)
        {
            SpawnRate = 1;
            this.target = target;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var player = (Parent as Orb).Parent as Player;

            if (player != null)
            {
                Position = new Vector2(player.X + Math.Sign((int)player.Direction) * 7, player.Y);
            }            
        }

        public override void CreateParticle()
        {
            new SpellParticle(this, target);
        }
    }
}
