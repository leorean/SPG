using Microsoft.Xna.Framework;
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

        //public List<Room> Rooms { get; private set; } = new List<Room>();

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

        private Weather weather;

        // transition
        
        private Vector2 newPosition;
        private bool invokeRoomChange;

        private int lookLocked;

        // target

        private Player player { get => target as Player; }

        // events

        /// <summary>
        /// Is called when a room change is initiated. Provides last room and current room as arguments.
        /// </summary>
        public event EventHandler<Tuple<Room, Room>> OnRoomChange;

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

        private void Transition_1()
        {
            // reset possible teleporter stuff
            player.Teleporter?.Reset();

            if (player.Teleporter != null && player.Stats.Teleporters.Count == 1)
            {
                new MessageBox("...");
            }

            player.Teleporter = null;
            if (player.Orb != null) player.Orb.Visible = true;

            player.State = Player.PlayerState.IDLE;
            player.OutOfScreen = false;
            player.Position = newPosition;

            if (player.Orb != null)
                player.Orb.Position = player.Position;

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

        private void Transition_2()
        {
            GameManager.Current.Transition.OnTransitionEnd = null;
            GameManager.Current.Transition = null;
            //player.Visible = true; <- done already at end of transition_1
        }

        public void ChangeRoomsToPosition(Vector2 position, int type)
        {
            //player.State = Player.PlayerState.BACKFACING;            
            GameManager.Current.Transition = new Transition(type);
            GameManager.Current.Transition.FadeIn();
            newPosition = position;
            GameManager.Current.Transition.OnTransitionEnd = Transition_1;
        }

        private Vector2 vel;

        private enum TransitionDirection { Horizontal, Vertical }

        private TransitionDirection moveDirection;
                
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
                //lookLocked = Math.Max(lookLocked - 1, 0);

                // if no room is yet found, try to find first room
                if (CurrentRoom == null)
                {                    
                    CurrentRoom = target.CollisionPointFirstOrDefault<Room>(target.X, target.Y);
                    if (CurrentRoom != null)
                    {
                        //lookLocked = 1;

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
                
                if (target is Player)
                {
                    var dir = (target as Player).Direction;
                    
                    switch ((target as Player).State)
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

                    
                    if (lookLocked == 0)
                    {
                        switch ((target as Player).LookDirection)
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

                if (lookLocked > 0)
                {
                    // move faster during lock-mode
                    vel = new Vector2((tarX - Position.X) / 6f, (tarY - Position.Y) / 6f);

                    if (MathUtil.Euclidean(Position, new Vector2(tarX, tarY)) < 2)
                        vel = new Vector2((tarX - Position.X) / 2f, (tarY - Position.Y) / 2f);
                }
                
                Position = new Vector2(Position.X + vel.X, Position.Y + vel.Y);
                
                if (lookLocked> 0)
                {
                    if (moveDirection == TransitionDirection.Horizontal)
                    {
                        var py = MathUtil.Div(Position.Y, ViewHeight) * ViewHeight + .5f * ViewHeight;
                        Position = new Vector2(Position.X, py);
                        
                        if (Math.Abs(Position.X - tarX) < .5f)
                        {
                            Position = new Vector2(tarX, Position.Y);
                            lookLocked = 0;
                        }
                    }

                    if (moveDirection == TransitionDirection.Vertical)
                    {
                        var px = MathUtil.Div(Position.X, ViewWidth) * ViewWidth + .5f * ViewWidth;
                        Position = new Vector2(px, Position.Y);

                        if (Math.Abs(Position.Y - tarY) < .5f)
                        {
                            Position = new Vector2(Position.X, tarY);
                            lookLocked = 0;
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
                        lookLocked = 1;

                        //if (lockDirection == LD.Horizontal)
                        //    Position = new Vector2(Position.X, Math.Min(Math.Max(target.Y, CurrentRoom.Y + .5f * ViewHeight), CurrentRoom.Y + CurrentRoom.BoundingBox.Height - .5f * ViewHeight));
                        
                        offsetX = 0;
                        state = State.RoomTransition;

                        if (CurrentRoom.Background != -1)
                        {
                            // switch backgrounds
                            backgroundAlpha = 0;
                            lastBG = CurrentBG;
                            CurrentBG = CurrentRoom.Background;
                        }                            
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
                
                OnRoomChange?.Invoke(this, new Tuple<Room,Room>(lastRoom, CurrentRoom));
                
                // change weather here
                switch (CurrentRoom?.Weather)
                {
                    case 0: // no weather
                            /*if (weather != null)
                            {
                                weather.Destroy();
                                weather = null;
                            }*/
                        break;
                    case 1: // 
                        //new EmitterSpawner<FireFlyEmitter>(CurrentRoom.X, CurrentRoom.Y, CurrentRoom);
                        break;
                }

                state = State.Default;
            }

            // background interpolation
            backgroundAlpha = Math.Min(backgroundAlpha + .02f, 1);
        }

        public void Reset()
        {
            SetTarget(null);

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
            var b = (Position.X * .4f) % 256;
            var c = (Position.X * .6f) % 256;
            var d = (Position.X * .8f) % 256;

            var bg = CurrentBG * 5;

            if (_backgrounds != null)
            {
                // TODO.
                //if (lastRoom != null)
                //{
                //    if (backgroundAlpha < 1)
                //    {
                //        if (lastBG != -1)
                //            sb.Draw(_backgrounds[lastBG], Position - new Vector2(ViewWidth * .5f, ViewHeight * .5f), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.0001f);
                //    }
                //}
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
                        }

                        //sb.Draw(_backgrounds[CurrentBG], Position - new Vector2(ViewWidth * .5f, ViewHeight * .5f), null, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0.0002f);                        
                    }
                }                
            }
        }        
    }
}
