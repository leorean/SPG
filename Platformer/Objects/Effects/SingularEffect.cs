using Microsoft.Xna.Framework;
using Platformer.Objects.Main;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Effects
{
    public class SingularEffect : GameObject
    {
        public int Type { get; set; }

        public SingularEffect(float x, float y, int type = 0) : base(x, y)
        {
            Depth = .8f;
            Type = type;
            AnimationTexture = AssetManager.Effects;
            DrawOffset = new Vector2(16, 16);

            Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            var cols = 7; // how many columns there are in the sheet
            var fSpd = .3f; // frame speed
            var fAmount = 7; // how many frames

            switch (Type)
            {
                case 0:                    
                    fAmount = 5;
                    break;
                case 1:
                    fAmount = 6;                    
                    break;
            }

            SetAnimation(cols * Type, cols * Type + fAmount, fSpd, false);
            Visible = true;

            AnimationComplete += Effect_AnimationComplete;
        }

        private void Effect_AnimationComplete(object sender, EventArgs e)
        {
            AnimationComplete -= Effect_AnimationComplete;

            Destroy();
        }
    }
}
