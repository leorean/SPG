using Leore.Main;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Level;
using Leore.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Objects.Items
{
    public class Collectable : Item
    {
        LightSource light;

        float sin;
        double t;
        Vector2 origPosition;

        private float lightAngle1;
        private float lightAngle2;

        CollectableAmbientEmitter emitter;

        Player player => GameManager.Current.Player;
        
        public Collectable(float x, float y, Room room) : base(x, y, room, "Collectable")
        {
            AnimationTexture = AssetManager.Collectable;
            //Texture = AssetManager.Collectable[0];

            Depth = Globals.LAYER_PLAYER - .0001f;

            DrawOffset = new Vector2(8);
            light = new LightSource(this);
            light.Scale = new Vector2(.5f);
            light.Active = true;

            origPosition = Position;
            emitter = new CollectableAmbientEmitter(X, Y);
            emitter.Parent = this;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            t = (t + .05) % (2 * Math.PI);
            var z = (float)Math.Sin(t);

            Position = new Vector2(origPosition.X, origPosition.Y + z);

            light.Scale = new Vector2(.5f - .1f * z);

            SetAnimation(0, 23, .3f, true);

            lightAngle1 = (lightAngle1 + 2) % 360;
            lightAngle2 = (lightAngle2 + 360 - 3) % 360;
            
            Taken = player.Stats.Collectables.Contains(ID);

            emitter.Position = Position + new Vector2(0, z - 2);

            //var alpha = .35f + Math.Abs(.25f * z);
            //Color = new Color(Color.White, alpha);
            
            Angle = 0 + .2f * z;
            
            if (Taken)
                Destroy();            
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            var lightAlpha1 = .5f * Math.Abs(Math.Sin((lightAngle1 / 360) * 2 * Math.PI));
            var lightAlpha2 = .5f * Math.Abs(Math.Sin((((lightAngle2 + 60) / 360) % 360) * 2 * Math.PI));

            sb.Draw(AssetManager.WhiteCircle[1], Position, null, new Color(Color.White, (float)lightAlpha1), (float)MathUtil.DegToRad(lightAngle1), new Vector2(32), .5f, SpriteEffects.None, Depth - .0001f);
            sb.Draw(AssetManager.WhiteCircle[1], Position, null, new Color(Color.White, (float)lightAlpha2), (float)MathUtil.DegToRad(lightAngle2), new Vector2(32), .5f, SpriteEffects.None, Depth - .0002f);
        }

        public override void Take(Player player)
        {
            if (Taken)
                return;

            // FLASH
            //new FlashEmitter(X, Y);

            var burst = new KeyBurstEmitter(X, Y, player);

            var colors = new List<Color>();
            for(var i = 0; i < 10; i ++)
            {
                colors.Add(CollectableAmbientEmitter.GetRandomColor());
            }

            burst.Colors = colors;

            player.Stats.Collectables.Add(ID);
            MainGame.Current.HUD.ShowCollectables(4 * 60, true);
        }
    }
}
