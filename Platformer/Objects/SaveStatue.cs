using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Objects.Effects;
using Platformer.Objects.Main;
using SPG;
using SPG.Objects;
using SPG.Util;
using SPG.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects
{
    public class SaveStatue : RoomDependentdObject
    {
        private bool alreadyActivated;
        public bool Active { get; private set; }

        private ParticleEmitter emitter;

        private List<Color> particleColors;

        private Vector2 floatPosition;

        private float sin;
        
        public SaveStatue(float x, float y, Room room, string name = null) : base(x, y, room)
        {            
            BoundingBox = new RectF(4, 6, Globals.TILE - 8, Globals.TILE - 7);
            Visible = true;
            AnimationSpeed = .1f;
            AnimationTexture = MainGame.Current.SaveStatueSprites;

            Depth = Globals.LAYER_BG + .001f;

            particleColors = new List<Color>();

            floatPosition = Vector2.Zero;

            alreadyActivated = true;

            particleColors.Add(new Color(255, 255, 255));
            particleColors.Add(new Color(206, 255, 255));
            particleColors.Add(new Color(168, 248, 248));
            particleColors.Add(new Color(104, 216, 248));
            
            emitter = new ParticleEmitter(x + 8, y + 8);
            
            emitter.ParticleInit = (particle) =>
            {
                var posX = emitter.X - 4 + RND.Next * 8;
                var posY = emitter.Y + 3;

                particle.LifeTime = 60;

                // reset spawn rate to low rate
                emitter.SpawnRate = .1f;

                particle.Position = new Vector2((float) posX, (float) posY);
                
                particle.YVel = (float) (-.2 - RND.Next * .2);
                particle.Scale = new Vector2(3, 3);
                particle.Alpha = 0;
                
                particle.Angle = (float)(RND.Next * 360);

                var colorIndex = RND.Int(particleColors.Count - 1);
                particle.Color = particleColors[colorIndex];

            };

            emitter.ParticleUpdate = (particle) => {

                var s = Math.Max(particle.Scale.X - .025f, 1);

                particle.Scale = new Vector2(s);

                var relativeLifeTime = particle.LifeTime / 60f;

                if (relativeLifeTime > .5f)
                    particle.Alpha = Math.Min(particle.Alpha + .1f, 1);
                else
                {
                    particle.Alpha = Math.Max(particle.Alpha - .05f, 0);
                }                
            };
        }
        
        // methods

        public void Save()
        {
            if (alreadyActivated)
                return;

            var posX = MathUtil.Div(X, Globals.TILE) * Globals.TILE + 8f;
            var posY = MathUtil.Div(Y, Globals.TILE) * Globals.TILE + 7.9f;

            var burst = new SaveStatueEmitter(emitter.Position.X, emitter.Position.Y);

            alreadyActivated = true;
            MainGame.Current.Save(posX, posY);
            Debug.WriteLine("Saved.");            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // ++++ draw <-> state logic ++++
            
            if (MainGame.Current.SaveGame != null)
            {
                var saveStatue = ObjectManager.CollisionPoint<SaveStatue>(MainGame.Current.SaveGame.playerPosition.X, MainGame.Current.SaveGame.playerPosition.Y).FirstOrDefault();

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
            emitter.Enabled = Enabled;

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
