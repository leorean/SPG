using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Leore.Objects.Effects.Emitters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using SPG.Util;
using static Leore.Objects.Effects.Transition;

namespace Leore.Objects.Level
{
    public class StoryWarp : RoomObject
    {
        Player player => GameManager.Current.Player;
        string setCondition;        
        string text;
        string levelName;
        string textAfterTransition;
        Direction direction;

        Vector2 targetPos;

        float alpha = 0;
        float maxAlpha = 2;
        bool touched;

        float xScale = 0;
        float warpY;

        bool init;
        bool firstTimeUse;

        private StoryWarpEmitter emitter;

        public StoryWarp(float x, float y, Room room, string setCondition, int tx, int ty, string text, Direction direction = Direction.NONE, string levelName = null, string textAfterTransition = null) : base(x, y, room)
        {
            BoundingBox = new RectF(-8, 0, 16, 8);
            
            Texture = AssetManager.StoryWarp[0];
            DrawOffset = new Vector2(16, 136);
            
            this.setCondition = setCondition;            
            this.text = text;
            this.direction = direction;
            this.levelName = levelName;
            this.textAfterTransition = textAfterTransition;

            if (!string.IsNullOrEmpty(levelName))
            {
                var px = tx * Globals.T + 8;
                var py = ty * Globals.T + 8;
                targetPos = new Vector2(px, py);
            }
            else
            {
                var px = MathUtil.Div(X, Globals.T) * Globals.T + 8;
                var py = MathUtil.Div(Y, Globals.T) * Globals.T + 8;
                targetPos = new Vector2(px + tx * Globals.T, py + ty * Globals.T);
            }
            
            warpY = Y;

            firstTimeUse = true;

            emitter = new StoryWarpEmitter(X, Y + 8);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!init)
            {
                init = true;
                if (GameManager.Current.HasStoryFlag(setCondition))
                {
                    xScale = 1;
                    alpha = .25f;
                    firstTimeUse = false;
                }
            }

            if (!touched)
            {
                if (this.CollisionBounds(player, X, Y))
                {
                    player.YVel = 0;
                    warpY = player.Y;
                    touched = true;
                    if (player.Orb != null)
                        player.Orb.Visible = false;
                    player.SetControlsEnabled(false);
                    MainGame.Current.HUD.SetVisible(false);
                }
            }
            else
            {
                player.State = Player.PlayerState.JUMP_UP;
                player.XVel = (X - player.X) / 20f;
                player.YVel = (warpY - player.Y) / 60f;

                warpY = Math.Max(warpY - .5f, Room.Y + 16);

                xScale = Math.Min(xScale + .01f, 1);

                alpha = Math.Min(alpha + .0045f, maxAlpha);
                if (alpha == maxAlpha)
                {
                    player.SetControlsEnabled(true);
                    GameManager.Current.AddStoryFlag(setCondition);

                    if (firstTimeUse)
                    {
                        var msg = new MessageBox(text, textSpeed: MessageBox.TextSpeed.SLOW);
                        msg.OnCompleted = Complete;
                    }
                    else
                    {
                        Complete();
                    }
                }
            }
        }
        
        void Complete()
        {
            RoomCamera.Current.ChangeRoomsToPosition(targetPos, TransitionType.LIGHT_FLASH_LONG_FADE, direction, levelName, textAfterTransition);
            Destroy();
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);
            
            // everything
            sb.Draw(AssetManager.Transition[1], new Vector2(RoomCamera.Current.ViewX, RoomCamera.Current.ViewY), null, new Color(Color.White, alpha - .25f), 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_FONT - .005f);

            // warp ray
            sb.Draw(AssetManager.StoryWarp[1], Position, null, new Color(Color.White, alpha), 0f, DrawOffset, new Vector2(xScale, 1), SpriteEffects.None, player.Depth + .001f);
            sb.Draw(AssetManager.StoryWarp[1], Position, null, new Color(Color.White, alpha), 0f, DrawOffset, new Vector2(.75f * xScale, 1), SpriteEffects.None, player.Depth + .001f);
        }
    }
}
