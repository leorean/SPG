using System.Linq;
using Microsoft.Xna.Framework;
using Leore.Main;
using System;
using Leore.Objects.Effects.Emitters;

namespace Leore.Objects.Level.Blocks
{
    public class KeyBlock : Solid
    {
        public bool Unlocked { get; set; }

        public KeyBlock(float x, float y, Room room) : base(x, y, room)
        {
            Texture = GameManager.Current.Map.TileSet[582];            
        }
        
        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);

            if (GameManager.Current.Player.Stats.KeysAndKeyblocks.Contains(ID))
            {
                Destroy();
                return;
            }

            Visible = true;
        }

        public void Unlock(float x, float y)
        {
            if (Unlocked)
                return;

            Unlocked = true;

            var emitter = new KeyBurstEmitter(x, y, this);
            emitter.OnFinishedAction = () =>
            {
                GameManager.Current.Player.Stats.KeysAndKeyblocks.Add(ID);

                new Effects.SingularEffect(X + 8, Y + 8, 2);
                new StarEmitter(X + 8, Y + 8);
            };
        }
    }
}
