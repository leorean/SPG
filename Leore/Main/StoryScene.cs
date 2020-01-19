using Leore.Objects.Effects.Emitters;
using Leore.Resources;
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
        float minAlpha = -.5f;
        float maxAlpha = 2;

        enum StoryState { FadeIn, Showing, FadeOut, Hiding }

        StoryState state = StoryState.FadeIn;

        bool messageShown;

        private int startFrame;
        private int endFrame;

        public Action OnCompleted;

        public StoryScene(int startFrame, int endFrame) : base(0, 0, null)
        {
            position = new Vector2(camera.ViewX, camera.ViewY);
            camera.SetTarget(this);

            alpha = minAlpha;
            
            // ---- INTRO ----

            // 0:
            texts.Add("In the beginning, ~Leore~ was a dark and cold place, drifting through the void.");
            // 1:
            texts.Add("Then, the [f6c048]~sun, source of all energy~, came to be and created life.|" +
                "Every living being is born as a child of the sun, holding part of it's energy within." +
                "|This energy is called [5fcde4]~magic~.");
            // 2:
            texts.Add("Eons passed and life was flourishing on ~Leore~. And so, in time, civilizations formed and thrived. " +
                "|But eventually, it's peak was reached.");
            // 3:
            texts.Add("The sun became weaker, drifting slowly away and the world would eventually face a cold dark death.");
            // 4:
            texts.Add("There were the protectors of the world called ~Ancients~, who defied the apocalypse." +
                "|They found a solution that should prevent the inevitable destiny of ~Leore~.");
            // 5:
            texts.Add("The ~Ancients~ created ~towers~ placed across the world that would form a powerful magic barrier." +
                "|This would hold together the fabric of the world and maintain [5fcde4]~balance~." +
                "|Exhausting their energy, the ~Ancients~ had to retreat from the face of the world.");
            // 6:
            texts.Add("Eventually, the [973bba]~Void~ could breach the barrier, corrupting the towers and letting darkness enter the world.");

            // 7:
            texts.Add("~Leore~ is once more threatened to fall into an era of eternal darkness..");

            // ---- ENDING 1 ----

            // 8:
            texts.Add("");
            
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
                    alpha = Math.Min(alpha + .02f, maxAlpha);
                    if (!messageShown)
                    {
                        if (alpha >= 0)
                        {
                            new MessageBox(texts[cursor]).OnCompleted = () =>
                            {
                                state = StoryState.FadeOut;
                            };
                            messageShown = true;
                        }                        
                    }
                    else
                    {
                        if (alpha == maxAlpha)
                        {
                            state = StoryState.Showing;
                            messageShown = false;
                        }
                    }
                    
                    break;
                case StoryState.Showing:
                    break;
                case StoryState.FadeOut:
                    alpha = Math.Max(alpha - .02f, minAlpha);
                    if (alpha == minAlpha)
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
            sb.Draw(image[cursor], position, null, new Color(Color.White, alpha *.5f), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.00001f);
            
            // text
            //font.Halign = Font.HorizontalAlignment.Center;
            //font.Valign = Font.VerticalAlignment.Top;
            //font.Draw(sb, position.X + camera.ViewWidth * .5f, position.Y + camera.ViewHeight * .5f, "Hello World.", depth: .001f);
        }
    }
}
