using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Util;
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

        public SaveStatue(float x, float y, Room room, string name = null) : base(x, y, room)
        {            
            BoundingBox = new RectF(4, 6, Globals.TILE - 8, Globals.TILE - 7);
            Visible = true;
            AnimationSpeed = .1f;
            AnimationTexture = MainGame.Current.SaveStatueSprites;

            Depth = Globals.LAYER_BG + .001f;

            emitter = new ParticleEmitter(x + 8, y + 8);

            emitter.ParticleInit = (particle) =>
            {
                var posX = x + RND.Next * 16;
                var posY = y + RND.Next * 16;

                particle.LifeTime = 60;

                particle.Position = new Vector2((float) posX, (float) posY);
                
                particle.YVel = (float) (-.2 - RND.Next * .5);
                particle.Scale = new Vector2(3, 3);
                particle.Alpha = 0;

                int r = (int)(120 + RND.Next * 60);
                int g = (int)(150 + RND.Next * 100);
                int b = 255;

                particle.Angle = (float)(RND.Next * 360);

                particle.Color = new Color(r, g, b);

            };

            emitter.ParticleUpdate = (particle) => {

                var s = Math.Max(particle.Scale.X - .025f, 1);

                particle.Scale = new Vector2(s);

                var p = Math.Sin((particle.LifeTime / 60f) * 2 * Math.PI);
                
                particle.Alpha = (float) p * 1;
            };
        }
        
        // methods

        public void Save()
        {
            if (alreadyActivated)
                return;

            var posX = MathUtil.Div(X, Globals.TILE) * Globals.TILE + 8;
            var posY = MathUtil.Div(Y, Globals.TILE) * Globals.TILE + 7;

            alreadyActivated = true;
            Debug.WriteLine("Saved.");
            MainGame.Current.Save(posX, posY);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // ++++ draw <-> state logic ++++

            var cols = 4; // how many columns there are in the sheet
            var row = 0; // which row in the sheet
            var fSpd = 0f; // frame speed
            var fAmount = 4; // how many frames
            var offset = 0; // offset within same row

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

            if (Active)
            {
                row = 1;
                fSpd = .1f;
                emitter.Active = true;
            }
            else
            {
                row = 0;
                fSpd = .1f;
                emitter.Active = false;
            }

            SetAnimation(cols * row + offset, cols * row + offset + fAmount - 1, fSpd, true);
        }
    }
}
