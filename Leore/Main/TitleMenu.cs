using Leore.Objects.Effects;
using Leore.Objects.Effects.Emitters;
using Leore.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Draw;
using SPG.Objects;
using SPG.Save;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Main
{
    public class TitleMenu : GameObject
    {
        private RoomCamera camera => RoomCamera.Current;
        Vector2 pos;

        private Font font = AssetManager.DefaultFont;

        double t;
        float z;
        float y;
        float yMax;
        float a;
        float spd;
        int cursor;
        float py;

        bool saveExists;
        private SaveGame saveGame;
        bool isShowingDialog;

        bool hasFlashed = false;
        float flash = 0;

        public TitleMenu(float x, float y, string name = null) : base(x, y, name)
        {
            saveExists = checkSaveFileExists();
            Reset();
        }

        public bool checkSaveFileExists()
        {
            saveGame = new SaveGame("save.dat");
            return SaveManager.Load(ref saveGame);
        }

        private void Reset()
        {
            hasFlashed = false;
            y = 0;
            t = 0;
            z = 0;
            yMax = -AssetManager.TitleMenu.Height + 144;
            a = -2;
            cursor = 0;
            spd = .2f;
            py = 144;
        }

        private void Flash()
        {
            if (hasFlashed)
                return;

            flash = 1.5f;
            hasFlashed = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // TODO: remove
            //new StoryScene(0, 9).OnCompleted = () =>
            //{
            //    GameManager.Current.ReloadLevel();
            //};

            //Destroy();
            //return;

            if (InputMapping.KeyPressed(InputMapping.ResetMenu))
            {
                Reset();
            }
            
            pos = new Vector2(camera.ViewX, camera.ViewY);

            t = (t + .025f);
            z = (float)(2 * Math.Sin(t));
            
            y = Math.Max(y - spd, yMax);

            //if (Math.Abs(y - yMax) < 2)
            if (y == yMax && py == 0)
            {
                //Flash();
            }

            flash = Math.Max(flash - .02f, 0);

            // TODO: doesn't work with gamepad

            if (Math.Abs(y - yMax) < 144)
                py = Math.Max(0, py - 1);

            if (y != yMax)
            {
                if (Math.Abs(y - yMax) > 288)
                {
                    spd = Math.Min(spd * 1.01f, 7);
                }
                else
                {
                    spd = Math.Max(spd * .977f, .2f);                    
                }
                if (InputMapping.KeyPressed(InputMapping.MessageNext))
                {
                    y = yMax;
                    Flash();
                }
            }
            else
            {
                //a = Math.Min(a + .0035f, 1);
                a = Math.Min(a + .015f, 1);
                spd = 0;

                if (MainGame.Current.Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Down, SPG.Input.State.Pressed))
                {
                    cursor = (cursor + 1) % 2;
                }
                if (MainGame.Current.Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Up, SPG.Input.State.Pressed))
                {
                    cursor = (cursor + 3) % 2;
                }

                if (!isShowingDialog)
                {
                    if (InputMapping.KeyPressed(InputMapping.MessageNext))
                    {
                        if (cursor == 0)
                        {
                            isShowingDialog = true;
                            if (saveExists)
                            {
                                isShowingDialog = true;
                                var dialog = new MessageDialog("Load game?");
                                dialog.NoAction = () =>
                                {
                                    isShowingDialog = false;
                                };
                                dialog.YesAction = () =>
                                {
                                    isShowingDialog = false;
                                    GameManager.Current.ReloadLevel();
                                    MainGame.Current.State = MainGame.GameState.Running;
                                };
                            }
                            else
                            {
                                isShowingDialog = true;
                                var dialog = new MessageDialog("Start new game?");
                                dialog.NoAction = () =>
                                {
                                    isShowingDialog = false;
                                };
                                dialog.YesAction = () =>
                                {
                                    new StoryScene(0, 10).OnCompleted = () =>
                                    {
                                        GameManager.Current.ReloadLevel();
                                    };
                                    Destroy();
                                };
                            }
                        }
                        if (cursor == 1)
                        {
                            isShowingDialog = true;
                            var dialog = new MessageDialog("Delete save game?");
                            dialog.NoAction = () =>
                            {
                                isShowingDialog = false;
                            };
                            dialog.YesAction = () =>
                            {
                                GameManager.Current.SaveGame.Delete();
                                Debug.WriteLine("Deleted save game.");

                                var message = new MessageBox("Save game deleted.");
                                message.OnCompleted = () =>
                                {
                                    isShowingDialog = false;

                                };
                                saveExists = checkSaveFileExists();
                            };
                        }
                    }
                }
            }

            // so window resizing happens properly
            camera.SetTarget(this);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            // presents..
            font.Halign = Font.HorizontalAlignment.Center;
            font.Valign = Font.VerticalAlignment.Top;
            font.Draw(sb, pos.X + camera.ViewWidth * .5f, pos.Y + camera.ViewHeight * .5f + y * .5f, "Shinypixelgames presents...", depth: .00003f);

            // BG
            sb.Draw(AssetManager.TitleMenu, pos + new Vector2(0, y), new Rectangle(256, 0, 256, AssetManager.TitleMenu.Height), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0000f);

            // title
            if (py == 0)
            {
                sb.Draw(AssetManager.TitleMenu, pos + new Vector2(0, z), new Rectangle(0, 0, 256, 144), new Color(Color.White, a), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0001f);
            }

            // cliff
            sb.Draw(AssetManager.TitleMenu, pos + new Vector2(0, py), new Rectangle(0, 288, 256, 144), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0002f);

            // title orb
            sb.Draw(AssetManager.TitleMenu, pos + new Vector2(0, z + py), new Rectangle(0, 144, 256, 144), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0001f);

            if (y == yMax)
            {
                if (py == 0)
                {
                    // font
                    font.Halign = Font.HorizontalAlignment.Left;
                    font.Valign = Font.VerticalAlignment.Top;
                    font.Draw(sb, pos.X + 32, pos.Y + camera.ViewHeight - 32, saveExists ? $"Load Game" : "New Game", depth: .0003f);
                    font.Draw(sb, pos.X + 32, pos.Y + camera.ViewHeight - 16, "Delete Game", depth: .0003f);

                    // cursor
                    font.Draw(sb, pos.X + 24, pos.Y + camera.ViewHeight - 32 + cursor * 16, ((char)129).ToString(), depth: .0003f);

                    font.Halign = Font.HorizontalAlignment.Right;
                    if (saveExists)
                    {
                        font.Draw(sb, pos.X + camera.ViewWidth - 16, pos.Y + camera.ViewHeight - 2 * Globals.T, $"Playtime: " + TimeUtil.TimeStringFromMilliseconds(saveGame.playTime), depth: .0003f);
                    }
                }
            }

            // flash
            sb.Draw(AssetManager.Flash, pos, null, new Color(Color.White, flash), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0005f);
        }
    }
}
