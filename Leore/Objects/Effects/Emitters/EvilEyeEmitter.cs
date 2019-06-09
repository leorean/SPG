using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Objects.Enemies;
using Leore.Resources;
using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Util;

namespace Leore.Objects.Effects.Emitters
{
    public class EvilEyeParticle : Particle
    {
        public EvilEyeParticle(float x, float y, ParticleEmitter emitter) : base(emitter)
        {
            Position = new Vector2(x, y);
            Color = GameResources.VoidColor;

            LifeTime = 30;

            YVel = (float) (-.1 - RND.Next * .1);
            Scale = new Vector2(RND.Choose(1.5f, 2));
            DrawOffset = new Vector2(.5f);

            Alpha = 0;
            Angle = (float)(RND.Next * 360);
        }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        var s = Math.Max(Scale.X - .025f, 1);

        Scale = new Vector2(s);

        var relativeLifeTime = LifeTime / 30f;

        if (relativeLifeTime > .5f)
            Alpha = Math.Min(Alpha + .1f, 1);
        else
        {
            Alpha = Math.Max(Alpha - .05f, 0);
        }
    }
}

    public class EvilEyeEmitter : ParticleEmitter
    {
        private BossMirrorSelf boss => Parent as BossMirrorSelf;
        public EvilEyeEmitter(float x, float y, BossMirrorSelf boss) : base(x, y)
        {
            Parent = boss;
            SpawnRate = 1;
            SpawnTimeout = 2;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Position = Parent.Position;
        }

        public override void CreateParticle()
        {
            var offx = Math.Sign((int)boss.Direction) * 2f;

            new EvilEyeParticle(X + offx - 3f, Y - 1, this);
            new EvilEyeParticle(X + offx + 3f, Y - 1, this);
        }
    }
}
