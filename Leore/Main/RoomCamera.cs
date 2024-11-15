﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Objects;
using Leore.Objects.Effects;
using SPG.Map;
using SPG.Objects;
using SPG.Util;
using SPG.View;
using System;
using System.Linq;
using Leore.Objects.Effects.Weather;
using System.Diagnostics;
using Leore.Objects.Effects.Ambience;
using Leore.Objects.Effects.Emitters;
using static Leore.Objects.Effects.Transition;

namespace Leore.Main
{
    public class RoomCamera : Camera
    {
        public enum State
        {
            Default,
            RoomTransition
        }
        
        private State state = State.Default;
        
        private GameObject target;
        
        // room <-> view calculation vars
        
        private Room lastRoom;
        public Room CurrentRoom { get; private set; }
        public int CurrentBG { get; set; } = 0;

        private float curX, curY;
        private float tx, ty;
        private float dyVel;
        
        // player-related vars

        private float offsetX = 0;
        private float offsetY = -Globals.T;

        // background vars

        private TextureSet _backgrounds;
        private float backgroundAlpha = 1f;        
        private int lastBG = 0;
        
        // transition
        
        private Vector2 newPosition;
        private bool invokeRoomChange;

        private bool lookLocked;
        float px, py;

        // target

        private Player player { get => target as Player; }
        
        // events

        /// <summary>
        /// Is called when a room change is initiated. Provides last room and current room as arguments.
        /// </summary>
        public event EventHandler<(Room, Room)> OnRoomChange;

        private static RoomCamera instance;
        public static new RoomCamera Current { get => instance; }
        public RoomCamera(ResolutionRenderer resolutionRenderer) : base(resolutionRenderer)
        {
            instance = this;
        }
        
        public void SetTarget(GameObject target)
        {
            this.target = target;
            Position = target != null ? target.Position : Vector2.Zero;            
        }

        // if type = 0, black transition. if type = 1, white transition, if type = 2, white transition + lie down
        private void Transition_1(TransitionType transitionType, Direction direction, string levelName, string textAfterTransition)
        {
            // reset possible teleporter stuff
            player.Teleporter?.Reset();

            if (player.Teleporter != null && player.Stats.Teleporters.Count == 1)
            {
                new MessageBox("...");
            }

            player.Teleporter = null;
            if (player.Orb != null) player.Orb.Visible = true;

            switch (transitionType)
            {
                case TransitionType.LONG_LIGHT:
                    player.LieDown(2 * 60);
                    break;
                case TransitionType.LIGHT_FLASH_LONG_FADE:
                    player.LieDown(4 * 60);
                    break;
                default:
                    player.State = Player.PlayerState.IDLE;
                    break;
            }
            
            if (direction != Direction.NONE)
                player.Direction = direction;

            player.OutOfScreen = false;
            player.Position = newPosition;

            if (player.Orb != null)
                player.Orb.Position = player.Position;

            if (levelName != null && levelName != GameManager.Current.Map.Name)
            {
                GameManager.Current.UnloadLevel(true);
                GameManager.Current.SetCurrentGameMap(levelName);
                GameManager.Current.LoadLevelObjects();

                DisableBounds();
                SetTarget(GameManager.Current.Player);                
            }

            tx = newPosition.X;
            ty = newPosition.Y;
            curX = newPosition.X;
            curY = newPosition.Y;
            invokeRoomChange = true;
            DisableBounds();
            
            GameManager.Current.Transition.FadeOut();
            
            GameManager.Current.Transition.OnTransitionEnd = Transition_2;
            player.Visible = true;            
        }

        private void Transition_2(TransitionType type, Direction direction, string levelName, string textAfterTransition)
        {
            GameManager.Current.Transition.OnTransitionEnd = null;
            GameManager.Current.Transition = null;
            //player.Visible = true; <- done already at end of transition_1            
            MainGame.Current.HUD.SetVisible(true);

            if (textAfterTransition != null)
            {
                new LevelTextDisplay(player.X, player.Y, textAfterTransition);
            }
        }

        public void ChangeRoomsToPosition(Vector2 position, TransitionType type, Direction direction, string levelName, string textAfterTransition)
        {
            //player.State = Player.PlayerState.BACKFACING;
            
            GameManager.Current.Transition = new Transition(type, direction, levelName, textAfterTransition);
            GameManager.Current.Transition.FadeIn();
            
            newPosition = position;
            GameManager.Current.Transition.OnTransitionEnd = new Transition.TransitionEnd(Transition_1);

        }

        private Vector2 vel;

        private enum TransitionDirection { Horizontal, Vertical }

        private TransitionDirection moveDirection;

        private int shakeTimer;
        private Action onShakeCompleteAction;
        public void Shake(int time, Action onComplete)
        {
            if (time == 0)
            {
                onComplete?.Invoke();
                return;
            }

            if (shakeTimer > 0 || onShakeCompleteAction != null)
                return;

            if (CurrentRoom != null && (CurrentRoom.BoundingBox.Width == ViewWidth || CurrentRoom.BoundingBox.Height == ViewHeight))
                DisableBounds();
            
            shakeTimer = time;
            onShakeCompleteAction = onComplete;
        }

        public override void Update(GameTime gt)
        {
            base.Update(gt);

            if (ObjectManager.ElapsedTime < ObjectManager.GameDelay)
                return;
            
            if (target == null)
                return;
            
            if (invokeRoomChange)
            {
                var targetRoom = target.CollisionPointFirstOrDefault<Room>(target.X, target.Y);

                if (CurrentRoom != targetRoom)
                    GameManager.Current.ChangeRoom(CurrentRoom, targetRoom);
                
                Position = newPosition;
                CurrentRoom = null;
                state = State.Default;
                invokeRoomChange = false;
                
                return;
            }

            // if no room is available, always resort to this state
            if (state == State.Default)
            {
                // if no room is yet found, try to find first room
                if (CurrentRoom == null)
                {                    
                    CurrentRoom = target.CollisionPointFirstOrDefault<Room>(target.X, target.Y);
                    if (CurrentRoom != null)
                    {
                        var tx = Math.Min(Math.Max(target.X + offsetX, CurrentRoom.X + .5f * ViewWidth), CurrentRoom.X + CurrentRoom.BoundingBox.Width - .5f * ViewWidth);
                        var ty = Math.Min(Math.Max(target.Y + offsetY, CurrentRoom.Y + .5f * ViewHeight), CurrentRoom.Y + CurrentRoom.BoundingBox.Height - .5f * ViewHeight);

                        Position = new Vector2(tx, ty);
                        
                        lastRoom = CurrentRoom;
                        lastBG = CurrentBG;
                        if (CurrentRoom.Background != -1)
                            CurrentBG = CurrentRoom.Background;
                        EnableBounds(new Rectangle((int)CurrentRoom.X, (int)CurrentRoom.Y, (int)CurrentRoom.BoundingBox.Width, (int)CurrentRoom.BoundingBox.Height));                        
                    }
                    else
                        return;                    
                }
                
                if (target is Player player)
                {
                    var dir = (target as Player).Direction;

                    if (MainGame.Current.HUD.Boss == null)
                    {
                        switch (player.State)
                        {

                            case Player.PlayerState.OBTAIN:
                            case Player.PlayerState.DEAD:
                            case Player.PlayerState.BACKFACING:
                                offsetX *= .9f;
                                break;
                            default:
                                offsetX += Math.Sign((int)dir) * 1f;
                                offsetX = offsetX.Clamp(-4 * Globals.T, 4 * Globals.T);
                                break;
                        }
                    }
                    else
                    {
                        offsetX *= .9f;
                    }
                    
                    if (!lookLocked)
                    {
                        switch (player.LookDirection)
                        {
                            case Direction.UP:
                                offsetY = Math.Max(offsetY - .5f, -3 * Globals.T);
                                break;
                            case Direction.DOWN:
                                offsetY = Math.Min(offsetY + .5f, 2 * Globals.T);
                                break;
                            default:
                                offsetY = -Globals.T;
                                break;
                        }
                    } else
                        offsetY = -Globals.T;                    
                }

                var tarX = Math.Min(Math.Max(target.X + offsetX, CurrentRoom.X + .5f * ViewWidth), CurrentRoom.X + CurrentRoom.BoundingBox.Width - .5f * ViewWidth);
                var tarY = Math.Min(Math.Max(target.Y + offsetY, CurrentRoom.Y + .5f * ViewHeight), CurrentRoom.Y + CurrentRoom.BoundingBox.Height - .5f * ViewHeight);

                if (Math.Abs(tarY - Position.Y) > 2 * Globals.T || Math.Abs(target.YVel) > 3)
                    dyVel = Math.Max(dyVel - .1f, 1);
                else
                    dyVel = Math.Min(dyVel + .1f, 12f);

                vel = new Vector2((tarX - Position.X) / 12f, (tarY - Position.Y) / dyVel);

                if (lookLocked)
                {
                    // move faster during lock-mode
                    vel = new Vector2((tarX - Position.X) / 6f, (tarY - Position.Y) / 6f);

                    if (Math.Abs(Position.X - tarX) < 8)
                        vel = new Vector2((tarX - Position.X) / 2f, vel.Y);
                    if (Math.Abs(Position.Y - tarY) < 8)
                        vel = new Vector2(vel.X, (tarY - Position.Y) / 2f);
                }

                Position = new Vector2(Position.X + vel.X, Position.Y + vel.Y);
                
                if (shakeTimer > 0)
                {
                    shakeTimer = Math.Max(shakeTimer - 1, 0);

                    var shakeX = RND.Choose(0, 0.5, 1);
                    var shakeY = RND.Choose(0, 0.5, 1);

                    var shakeSign = (shakeTimer % 2 == 0) ? 1 : -1;
                    Position += new Vector2((float)shakeX * shakeSign, (float)shakeY * shakeSign);

                    if (shakeTimer == 0)
                    {
                        EnableBounds(new Rectangle((int)CurrentRoom.X, (int)CurrentRoom.Y, (int)CurrentRoom.BoundingBox.Width, (int)CurrentRoom.BoundingBox.Height));
                        onShakeCompleteAction?.Invoke();
                        onShakeCompleteAction = null;
                    }
                }
                
                if (lookLocked)
                {
                    if (moveDirection == TransitionDirection.Horizontal)
                    {
                        py = Math.Max(Position.Y, Math.Max(CurrentRoom.Y, lastRoom.Y) + .5f * ViewHeight);
                        py = Math.Min(py, Math.Min(CurrentRoom.Y + CurrentRoom.BoundingBox.Height, lastRoom.Y + lastRoom.BoundingBox.Height) - .5f * ViewHeight);
                        
                        Position = new Vector2(Position.X, py);
                        
                        if (Math.Abs(Position.X - tarX) < .5f)
                        {
                            Position = new Vector2(tarX, Position.Y);
                            lookLocked = false;
                        }
                    }

                    if (moveDirection == TransitionDirection.Vertical)
                    {

                        px = Math.Max(Position.X, Math.Max(CurrentRoom.X, lastRoom.X) + .5f * ViewWidth);
                        px = Math.Min(px, Math.Min(CurrentRoom.X + CurrentRoom.BoundingBox.Width, lastRoom.X + lastRoom.BoundingBox.Width) - .5f * ViewWidth);
                        
                        //px = MathUtil.Div(Position.X, ViewWidth) * ViewWidth + .5f * ViewWidth;
                        
                        Position = new Vector2(px, Position.Y);

                        if (Math.Abs(Position.Y - tarY) < .5f)
                        {
                            Position = new Vector2(Position.X, tarY);
                            lookLocked = false;
                        }
                    }

                }
                
                // if outside view, try to find new room                
                if ((!MathUtil.In(target.X, CurrentRoom.X, CurrentRoom.X + CurrentRoom.BoundingBox.Width)
                        || !MathUtil.In(target.Y, CurrentRoom.Y, CurrentRoom.Y + CurrentRoom.BoundingBox.Height)))
                {
                    if (!target.X.In(CurrentRoom.X, CurrentRoom.X + CurrentRoom.BoundingBox.Width))
                        moveDirection = TransitionDirection.Horizontal;
                    if (!target.Y.In(CurrentRoom.Y, CurrentRoom.Y + CurrentRoom.BoundingBox.Height))
                        moveDirection = TransitionDirection.Vertical;
                    
                    lastRoom = CurrentRoom;
                    CurrentRoom = ObjectManager.CollisionPoints<Room>(target, target.X, target.Y).FirstOrDefault();
                    
                    if (CurrentRoom != null)
                    {
                        lookLocked = true;
                        
                        offsetX = 0;
                        state = State.RoomTransition;

                        lastBG = CurrentBG;
                        if (CurrentRoom.Background != -1)
                        {
                            // switch backgrounds                                                        
                            CurrentBG = CurrentRoom.Background;
                        }

                        if (lastBG != CurrentBG && lastBG != -1)
                            backgroundAlpha = 0;
                    }
                }
            }

            // prepare for transition
            if (state == State.RoomTransition)
            {
                DisableBounds();
                
                tx = Position.X;
                ty = Position.Y;

                curX = tx;
                curY = ty;
                
                OnRoomChange?.Invoke(this, (lastRoom, CurrentRoom));
                
                state = State.Default;
            }

            // background interpolation
            backgroundAlpha = Math.Min(backgroundAlpha + .02f, 1);
        }

        public void Reset()
        {
            SetTarget(null);
            shakeTimer = 0;
            onShakeCompleteAction = null;
            
            state = State.Default;
            CurrentRoom = null;
            lastRoom = null;
        }
        
        /// <summary>
        /// Loads the backgrounds to the camera so it can display them depending on the room background number.
        /// </summary>
        /// <param name="backgrounds"></param>
        public void SetBackgrounds(TextureSet backgrounds)
        {
            _backgrounds = backgrounds;
        }

        public void Draw(SpriteBatch sb, GameTime gameTime)
        {
            var a = (Position.X * .0f) % 256;
            var b = (Position.X * .15f) % 256;
            var c = (Position.X * .25f) % 256;
            var d = (Position.X * .3f) % 256;
            var e = (Position.X * .5f) % 256;

            var bg = CurrentBG * 6;
            var bgOld = lastBG * 6;

            if (_backgrounds != null)
            {
                // TODO.
                if (lastRoom != null)
                {
                    if (backgroundAlpha < 1)
                    {
                        if (lastBG != -1)
                        {
                            //sb.Draw(_backgrounds[lastBG], Position - new Vector2(ViewWidth * .5f, ViewHeight * .5f), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.0001f);
                            for (var i = -1; i < 2; i++)
                            {
                                var posX = Position.X - ViewWidth * .5f + i * ViewWidth;
                                var posY = Position.Y - ViewHeight * .5f;

                                sb.Draw(_backgrounds[bgOld + 0], new Vector2(posX - a, posY), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.00010f);
                                sb.Draw(_backgrounds[bgOld + 1], new Vector2(posX - b, posY), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.00011f);
                                sb.Draw(_backgrounds[bgOld + 2], new Vector2(posX - c, posY), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.00012f);
                                sb.Draw(_backgrounds[bgOld + 3], new Vector2(posX - d, posY), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.00013f);
                                sb.Draw(_backgrounds[bgOld + 4], new Vector2(posX - e, posY), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.00014f);
                            }
                        }
                    }
                }
                if (CurrentRoom != null)
                {
                    var color = new Color(Color.White, backgroundAlpha);
                    if (CurrentBG != -1)
                    {
                        for(var i = -1; i < 2; i++)
                        {
                            var posX = Position.X - ViewWidth * .5f + i * ViewWidth;
                            var posY = Position.Y - ViewHeight * .5f;

                            sb.Draw(_backgrounds[bg + 0], new Vector2(posX - a, posY), null, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0.00020f);
                            sb.Draw(_backgrounds[bg + 1], new Vector2(posX - b, posY), null, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0.00021f);
                            sb.Draw(_backgrounds[bg + 2], new Vector2(posX - c, posY), null, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0.00022f);
                            sb.Draw(_backgrounds[bg + 3], new Vector2(posX - d, posY), null, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0.00023f);
                            sb.Draw(_backgrounds[bg + 4], new Vector2(posX - e, posY), null, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0.00024f);
                        }

                        //sb.Draw(_backgrounds[CurrentBG], Position - new Vector2(ViewWidth * .5f, ViewHeight * .5f), null, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0.0002f);                        
                    }
                }                
            }
        }        
    }
}
