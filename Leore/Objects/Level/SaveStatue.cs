using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Main;
using Leore.Objects.Effects.Emitters;
using SPG.Objects;
using SPG.Util;
using System;
using System.Diagnostics;
using System.Linq;

namespace Leore.Objects.Level
{
    public class SaveStatue : RoomObject
    {
        private bool alreadyActivated;
        public bool Active { get; private set; }

        private SaveStatueEmitter emitter;
        
        private Vector2 floatPosition;

        private float sin;
        
        public SaveStatue(float x, float y, Room room, string name = null) : base(x, y, room)
        {            
            BoundingBox = new RectF(4, 6, Globals.T - 8, Globals.T - 7);
            Visible = true;
            AnimationSpeed = .1f;
            AnimationTexture = AssetManager.SaveStatue;
            
            floatPosition = Vector2.Zero;

            alreadyActivated = true;

            emitter = new SaveStatueEmitter(X + 8, Y + 8);
        }
        
        // methods

        public void Save()
        {
            if (alreadyActivated)
                return;

            var posX = MathUtil.Div(X, Globals.T) * Globals.T + 8f;
            var posY = MathUtil.Div(Y, Globals.T) * Globals.T + 8f;

            var burst = new SaveBurstEmitter(emitter.Position.X, emitter.Position.Y);
            
            alreadyActivated = true;
            GameManager.Current.Save(posX, posY);
            Debug.WriteLine("Saved.");            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // ++++ draw <-> state logic ++++
            
            if (GameManager.Current.SaveGame != null)
            {
                var saveStatue = ObjectManager.CollisionPointFirstOrDefault<SaveStatue>(GameManager.Current.SaveGame.playerPosition.X, GameManager.Current.SaveGame.playerPosition.Y);

                if (saveStatue == this)
                    Active = true;
                else
                    Active = false;
            }

            if (alreadyActivated)
            {
                var player = this.CollisionBounds<Player>(X, Y).FirstOrDefault();
                if (player == null)
                    alreadyActivated = false;
            }

            emitter.Active = Active;
            //emitter.Enabled = Enabled;

            if (Active)
            {                
                sin = (sin + .05f) % (2f * (float)Math.PI);
            }
            else
            {
                sin = Math.Max(sin - .1f, 0);
            }
            
            emitter.Position = Position + floatPosition + new Vector2(8 , 8);

            floatPosition = new Vector2(0, -5 - (float)Math.Sin(sin) * 2);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {            
            sb.Draw(AnimationTexture[0], Position, null, Color.White, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);

            if (Active)
                sb.Draw(AnimationTexture[1], Position + floatPosition, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth - .0003f);
            else
                sb.Draw(AnimationTexture[3], Position + floatPosition, null, new Color(Color.White, 0.5f), Angle, DrawOffset, Scale, SpriteEffects.None, Depth - .0003f);

            if (Active)
            {
                var shineColor = new Color(Color.White, (float)Math.Max(Math.Sin(sin), 0) * .7f);
                sb.Draw(AnimationTexture[2], Position + floatPosition, null, shineColor, Angle, DrawOffset, Scale, SpriteEffects.None, Depth - .0002f);
            }
        }
    }
}
