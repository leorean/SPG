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
        Vector2 position;

        private Font font = AssetManager.DefaultFont;

        double t = 0;
        float z = 0;

        int cursor = 0;

        bool saveExists;
        private SaveGame saveGame;
        bool isShowingDialog;
        
        public TitleMenu(float x, float y, string name = null) : base(x, y, name)
        {
            saveExists = checkSaveFileExists();
        }

        public bool checkSaveFileExists()
        {
            saveGame = new SaveGame("save.dat");
            return SaveManager.Load(ref saveGame);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            position = new Vector2(camera.ViewX, camera.ViewY);
            t = (t + .025f);
            z = (float)(2 * Math.Sin(t));

            // TODO: doesn't work with gamepad

            if (MainGame.Current.Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Down, SPG.Input.State.Pressed))
            {
                cursor = (cursor + 1) % 2;
            }
            if (MainGame.Current.Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Up, SPG.Input.State.Pressed))
            {
                cursor = (cursor + 3) % 2;
            }

            if (!isShowingDialog) {
                if (MainGame.Current.Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Enter, SPG.Input.State.Pressed)
                    || MainGame.Current.Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.A, SPG.Input.State.Pressed)
                    || MainGame.Current.Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.S, SPG.Input.State.Pressed))
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
                                isShowingDialog = false;
                                GameManager.Current.ReloadLevel();
                                MainGame.Current.State = MainGame.GameState.Running;
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

            camera.SetTarget(this);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            // BG
            sb.Draw(AssetManager.TitleMenu, position, new Rectangle(256, 0, 256, 144), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0000f);

            // floaty thing
            sb.Draw(AssetManager.TitleMenu, position + new Vector2(0, z), new Rectangle(0, 0, 256, 144), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0001f);

            // font
            font.Halign = Font.HorizontalAlignment.Left;
            font.Valign = Font.VerticalAlignment.Top;
            font.Draw(sb, position.X + 32, position.Y + camera.ViewHeight - 32, saveExists ? $"Load Game" : "New Game", depth: .00003f);
            font.Draw(sb, position.X + 32, position.Y + camera.ViewHeight - 16, "Delete Game", depth: .00003f);

            // cursor
            font.Draw(sb, position.X + 24, position.Y + camera.ViewHeight - 32 + cursor * 16, ((char)129).ToString(), depth: .00003f);

            font.Halign = Font.HorizontalAlignment.Right;
            if (saveExists)
            {
                font.Draw(sb, position.X + camera.ViewWidth - 16, position.Y + camera.ViewHeight - 2 * Globals.T, $"Playtime: " + TimeUtil.TimeStringFromMilliseconds(saveGame.playTime), depth: .00003f);
            }            
        }
    }
}
