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
        
        public SaveStatue(float x, float y, Room room, string name = null) : base(x, y, room)
        {            
            BoundingBox = new RectF(0, 0, Globals.TILE, Globals.TILE);
            Visible = true;
            AnimationSpeed = .1f;
            AnimationTexture = MainGame.Current.SaveStatueSprites;

            Depth = Globals.LAYER_BG + .001f;
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
            }
            else
            {
                row = 0;
                fSpd = .1f;
            }

            SetAnimation(cols * row + offset, cols * row + offset + fAmount - 1, fSpd, true);
        }
    }
}
