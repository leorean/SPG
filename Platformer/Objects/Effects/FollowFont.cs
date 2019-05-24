using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Main;
using Platformer.Objects.Main;
using SPG;
using SPG.Draw;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Effects
{
    class FollowFont : GameObject
    {
        protected string text;
        protected float alpha;

        protected Font font;

        private float scale = .25f;

        private float offX, offY;
        private GameObject target;
        public GameObject Target { get => target; set { target = value; offX = (X - target.X); offY = (Y - target.Y); } }

        public FollowFont(float x, float y, string text) : base(x, y)
        {
            Position = new Vector2(x, y);
            this.text = text;

            alpha = 2f;

            font = AssetManager.DamageFont.Copy();
            Color = Color.White;
            
            font.Halign = Font.HorizontalAlignment.Center;
            font.Valign = Font.VerticalAlignment.Center;

            Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (ObjectManager.Exists(Target))
            {
                YVel = 0;
                Position = new Vector2(Target.X + offX, Target.Y + offY);
            }
            
            alpha = Math.Max(alpha - .03f, 0);

            if (alpha < 1) offY -= .5f;
            
            Visible = true;

            scale = Math.Min(scale + .04f, 1);

            var c = Color;
            Color = new Color(c, alpha);
            font.Color = Color;

            if (alpha == 0 || Y > RoomCamera.Current.ViewY + RoomCamera.Current.ViewHeight)
                Destroy();
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);
            
            font.Draw(sb, X, Y, text, scale:scale);
        }        
    }
}
