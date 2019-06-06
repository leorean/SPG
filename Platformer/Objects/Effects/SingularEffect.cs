using Microsoft.Xna.Framework;
using Platformer.Main;
using SPG.Objects;
using System;

namespace Platformer.Objects.Effects
{
    public class SingularEffect : GameObject
    {
        public int Type { get; set; }

        public bool Loop { get; set; }

        protected int cols;
        protected float fSpd;
        protected int fAmount;

        public SingularEffect(float x, float y, int type = 0) : base(x, y)
        {
            Depth = Globals.LAYER_EFFECT;
            Type = type;
            AnimationTexture = AssetManager.Effects;
            DrawOffset = new Vector2(16, 16);

            Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            cols = 10; // how many columns there are in the sheet
            fSpd = .3f; // frame speed
            fAmount = 7; // how many frames
            
            switch (Type)
            {
                case 0:                    
                    fAmount = 5;
                    break;
                case 1:                
                    fAmount = 6;
                    break;
                case 2:
                case 4:
                    fAmount = 8;
                    fSpd = .4f;
                    break;
                case 8:
                    fAmount = 9;
                    fSpd = .4f;
                    break;
            }

            SetAnimation(cols * Type, cols * Type + fAmount, fSpd, Loop);
            Visible = true;

            if (!Loop)
                AnimationComplete += Effect_AnimationComplete;
        }

        protected void Effect_AnimationComplete(object sender, EventArgs e)
        {
            AnimationComplete -= Effect_AnimationComplete;

            Destroy();
        }
    }
}
