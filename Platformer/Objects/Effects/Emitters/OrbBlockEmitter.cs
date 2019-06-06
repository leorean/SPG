using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Platformer.Objects.Effects.Emitters
{
    public class OrbBlockEmitter : SaveStatueEmitter
    {
        public OrbBlockEmitter(float x, float y) : base(x, y)
        {
            SpawnTimeout = 10;
            Active = false;

            particleColors = new List<Color>
            {
                new Color(255, 255, 255)
            };
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Active && Particles.Count == 0)
                Destroy();
        }        
    }
}
