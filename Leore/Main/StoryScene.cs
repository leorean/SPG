using Leore.Objects.Effects.Emitters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Draw;
using SPG.Map;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Main
{
    public class StoryScene : GameObject
    {
        private TextureSet image => AssetManager.Scenes;
        private RoomCamera camera => RoomCamera.Current;
        private Vector2 position;
        private Font font = AssetManager.DefaultFont;
        
        private int cursor = 0;
        private List<string> texts = new List<string>();

        float alpha = 0;
        
        enum StoryState { FadeIn, Showing, FadeOut, Hiding }

        StoryState state = StoryState.FadeIn;

        public StoryScene(float x, float y, string name = null) : base(x, y, name)
        {
            position = new Vector2(camera.ViewX, camera.ViewY);
            camera.SetTarget(this);

            texts.Add("Once upon a time, there was a world called \n~Leore~.");
            texts.Add("1.");
            texts.Add("2.");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            switch (state)
            {
                case StoryState.FadeIn:
                    alpha = Math.Min(alpha + .02f, 1);
                    if (alpha == 1)
                    {
                        new MessageBox(texts[cursor]).OnCompleted = () =>
                        {
                            state = StoryState.FadeOut;
                        };
                        state = StoryState.Showing;
                    }
                    break;
                case StoryState.Showing:
                    break;
                case StoryState.FadeOut:
                    alpha = Math.Max(alpha - .02f, 0);
                    if (alpha == 0)
                    {
                        cursor += 1;
                        state = StoryState.Hiding;
                    }
                    break;
                case StoryState.Hiding:
                    state = StoryState.FadeIn;
                    break;
            }
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            // draw darkness
            sb.Draw(AssetManager.Transition[0], position, null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);

            // draw scene background            
            sb.Draw(image[cursor], position, null, new Color(Color.White, alpha), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.00001f);
            
            // text
            //font.Halign = Font.HorizontalAlignment.Center;
            //font.Valign = Font.VerticalAlignment.Top;
            //font.Draw(sb, position.X + camera.ViewWidth * .5f, position.Y + camera.ViewHeight * .5f, "Hello World.", depth: .001f);
        }
    }
}
