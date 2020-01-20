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
        
        enum StoryState { FadeIn, Showing, FadeOut, Hiding }

        StoryState state = StoryState.FadeIn;
        
        private int startFrame;
        private int endFrame;

        public Action OnCompleted;

        public StoryScene(int startFrame, int endFrame) : base(0, 0, null)
        {
            position = new Vector2(camera.ViewX, camera.ViewY);
            
            // ---- INTRO ----

            // 0:
            texts.Add("In the beginning, ~Leore~ was a lifeless place, drifting through the [973bba]~Void~.");
            // 1:
            texts.Add("Then, the [f6c048]~sun, source of all energy~, came to be and created life.|" +
                "Every living being is born as a child of the sun, holding part of it's energy within." +
                "|This energy is called [5fcde4]~magic~.");
            // 2:
            texts.Add("Eons passed and life was flourishing on ~Leore~. And so, in time, civilizations formed and thrived. " +
                "|But eventually, it's peak was reached.");
            // 3:
            texts.Add("The sun became weaker, drifting slowly away and ~Leore~ would eventually face a cold dark death.");
            // 4:
            texts.Add("There were the protectors of the world called ~Ancients~, who defied the apocalypse." +
                "|They found a solution that should prevent the inevitable destiny of ~Leore~.");
            // 5:
            texts.Add("The ~Ancients~ built ~towers~ across the world that would create a vast magic barrier.");

            // 6:
            texts.Add("For centuries, this barrier would hold together the fabric of the world, protecting it from the [973bba]~Void~.");
            // 7:
            texts.Add("Eventually, the [973bba]~Void~ found a way to break through..");
            
            // 8:
            texts.Add("Corrupted by the [973bba]~Void~, the ~towers~ turned from sacret sites into pinnacles of darkness.");

            // 9
            //texts.Add("If the corruption is not stopped, the barrier will give in and ~Leore~ will live to see the era of eternal darkness..");
            texts.Add("...|" +
                ".....|" +
                "...wake up.");
            
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

            // so window resizing happens properly
            camera.SetTarget(this);

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
