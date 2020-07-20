﻿using Leore.Objects.Effects.Emitters;
using Leore.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG;
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
    public class StoryScene : GameObject, IKeepEnabledAcrossRooms
    {
        private TextureSet image => AssetManager.Scenes;
        private RoomCamera camera => RoomCamera.Current;
        private Vector2 position;
        private Font font = AssetManager.DefaultFont;
        
        private int cursor = 0;
        private List<(string, bool, bool)> texts = new List<(string, bool, bool)>();

        float alpha = 0;
        
        enum StoryState { FadeIn, Showing, FadeOut, Hiding }

        StoryState state = StoryState.FadeIn;
        
        private int startFrame;
        private int endFrame;

        private int timer = 0;
        private MessageBox messageBox;

        public Action OnCompleted;

        string sunColor => GameResources.SunColor.ToHex();
        string voidColor => GameResources.VoidColor.ToHex();
        string magicColor => GameResources.MagicColor.ToHex();

        void AddText(string text, bool showBorder = false, bool center = false)
        {
            texts.Add((text, showBorder, center));
        }

        public StoryScene(int startFrame, int endFrame) : base(0, 0, null)
        {
            position = new Vector2(camera.ViewX, camera.ViewY);

            // ---- INTRO ----

            // 0:
            AddText($"This is one of many tales of which the people speak...", center: true);
            // 1:
            AddText($"Long ago, there existed a land, vast and bright.|" +
                $"Golden light touched the earth and the skies.|" +
                $"With the sun, the gift of life was given.|" +
                $"Eons have passed and kingdoms thrived.|" +
                $"But eventually...");
            //AddText($"Long ago, a land existed, vast and bright.|" +
            //    $"The skies were whelmed by golden light.|" +
            //    $"The sun endowed the gift of life,\n" +
            //    $"so, eons passed and kingdoms thrived.");
            // 2:
            AddText($"One fateful day, the sun went dark.|" +
                $"When the sky turned black, an evil force emerged.|" +
                $"The void was born and threatened to swallow the world.");
            //AddText($"Until one day the sun went cold,\n" +
            //    $"the land would wither, skies would fold.|" +
            //    $"For, once the source of light had gone,\n" +
            //    $"the darkest era was to come.");
            // 3:
            AddText("There were the protectors of the world - called ~Ancients~ - that defied the apocalypse.|" +
                "With their magic powers, they fended off the void and kindled the light once more.|" +
                "When the deed was done, they vanished, never to be seen again...");
            // 4:
            AddText($"In their final act, they emplaced their power into an ~Orb~ that would awaken the rightful heir.");

            // 5
            AddText($"...|" +
                $".....|" +
                $"...wake up.", true);

            // ---- INTRO (old) ----

            //// 0:
            //AddText($"In the [{voidColor}]~Void~, there was a world floating through time and space.|This world was called ~Leore~.");
            //// 1:
            //AddText($"Then came the [{sunColor}]~Sun~." +
            //    $"|It created life." +
            //    $"|Years, decades, centuries, even millenia passed and life evolved.");
            //// 2:
            //AddText($"And so, in time, civilizations formed and thrived. " +
            //    $"|But eventually, the apex was reached.");
            //// 3:
            //AddText($"Eventually, the [{sunColor}]~Sun~ became weaker and would drift away, yielding a cold death to the creatures of  ~Leore~.");
            //// 4:
            //AddText($"There were the protectors of the world - called ~Ancients~ - who defied this apocalypse." +
            //    $"|They discovered a way that would prevent the inevitable doom of ~Leore~.");
            //// 5:
            //AddText($"The ~Ancients~ built [{sunColor}]~holy structures~ across the world. \nThese would create a vast [{magicColor}]~magic barrier~.");

            //// 6:
            //AddText($"This barrier would maintain equilibrium between [{sunColor}]~light~ and [{voidColor}]~darkness~." +
            //    $"|For centuries, this has been proven true and life was protected.|Only a few would even know of this actuality.");
            //// 7:
            //AddText($"But light cannot exist without darkness.\nThe [{sunColor}]~Sun~ could not exist without the [{voidColor}]~Void~.");
            
            //// 8:
            //AddText($"And eventually, the [{voidColor}]~Void~ found a way to break through the barrier.");

            //// 9:
            //AddText($"The [{sunColor}]~sacred sites~ were corrupted and turned into [{voidColor}]~structures of darkness~." +
            //    $"|The ~Ancients~ were able to ~seal them~ just in time before the barrier would give in completely." +
            //    $"|But they had to pay an ultimate price..");

            //// 10:
            //AddText($"In their final act, they emplaced their power into an ~Orb~ that would awaken the rightful heir.");

            //// 11
            //AddText($"...|" +
            //    $".....|" +
            //    $"...wake up.");
            
            // ---- ENDING 1 ----

            // 8:
            AddText("");
            
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

            timer = Math.Max(timer - 1, 0);

            switch (state)
            {
                case StoryState.FadeIn:
                    alpha = Math.Min(alpha + .02f, 1);

                    if (alpha == 1)
                    {
                        if (messageBox == null)
                        {
                            messageBox = new MessageBox(texts[cursor].Item1, showBorder: texts[cursor].Item2, centerText: texts[cursor].Item3);
                            messageBox.OnCompleted = () =>
                            {
                                state = StoryState.Showing;
                                messageBox = null;
                            };
                        }
                        timer = 30;
                        //state = StoryState.Showing;
                    }
                    
                    break;
                case StoryState.Showing:                    
                    if (timer == 0)
                    {
                        state = StoryState.FadeOut;
                    }
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
                        timer = 60;
                        state = StoryState.Hiding;
                    }
                    break;
                case StoryState.Hiding:                    
                    if (timer == 0)
                    {
                        state = StoryState.FadeIn;
                    }
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
