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

        private int startFrame;
        private int endFrame;

        public Action OnCompleted;

        public StoryScene(int startFrame, int endFrame) : base(0, 0, null)
        {
            position = new Vector2(camera.ViewX, camera.ViewY);
            camera.SetTarget(this);

            //texts.Add("Once upon a time, there was a world called \n~Leore~.");
            //texts.Add("In the beginning, ~Leore~ was a dark and cold place, drifting through the void.");
            //texts.Add("Then, the ~sun, source of all energy~, came to be and created life.\n" +
            //    "Every creature is born from the sun, holding part of it's energy within.");
            //texts.Add("Nonetheless, the world was an unforgiving place. As the sun gave more and more of it's source, it became colder and weaker, until eventually, the void would consume the world once again.");
            texts.Add("In time, some creatures learned to harness this energy.\nThey called it ~magic~ and developed outstanding powers." +
                "|There were those, who wanted to make these powerse accessible for everyone." +
                "\nThe most powerful spell was cast and it created the most remarkable event in history: ~The proclamation of magic~.");

            //texts.Add("Civilizations flourished and life was at the peak of prosperity." +
            //    "|But this came with an enormous price...");

            texts.Add("There were the protectors of the world, called ~Ancients~, who defied the apocalypse and created a solution that should prevent the inevitable destiny of ~Leore~.");
            texts.Add("The ~Ancients~ created ~six towers~ placed across the world that would form a powerful magic barrier, holding together the fabric of the world.");
            texts.Add("asdf");
            texts.Add("asdf");
            texts.Add("asdf");
            texts.Add("asdf");
            texts.Add("asdf");
            texts.Add("asdf");


            if (startFrame > endFrame) throw new ArgumentOutOfRangeException("startFrame");
            if (endFrame > texts.Count) throw new ArgumentOutOfRangeException("endFrame");

            this.startFrame = startFrame;
            this.endFrame = endFrame;

            cursor = startFrame;
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

                        if (cursor > endFrame)
                        {
                            OnCompleted?.Invoke();
                            Destroy();
                        }
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
