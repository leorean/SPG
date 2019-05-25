using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Objects;
using Platformer.Objects.Effects;
using Platformer.Objects.Effects.Emitters;
using Platformer.Objects.Enemies;
using Platformer.Objects.Main;
using SPG;
using SPG.Map;
using SPG.Objects;
using SPG.Util;
using SPG.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Main
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
        private float offsetY = -Globals.TILE;

        // background vars

        private TextureSet _backgrounds;
        private float backgroundAlpha = 1f;        
        private int lastBG = 0;

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
            player.State = Player.PlayerState.IDLE;
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
        }

        private void Transition_2()
        {
            GameManager.Current.Transition.OnTransitionEnd = null;
            GameManager.Current.Transition = null;
        }

        public void ChangeRoomsFromPosition(Vector2 position)
        {
            player.State = Player.PlayerState.DOOR;
            GameManager.Current.Transition = new Transition();
            GameManager.Current.Transition.FadeIn();
            newPosition = position;
            GameManager.Current.Transition.OnTransitionEnd = Transition_1;
        }

        private enum LD { Horizontal, Vertical }

        private LD moveDirection;

        private float minX, minY, maxX, maxY;
        
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
                        lookLocked = 1;
                        
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
                        case Player.PlayerState.DOOR:
                            offsetX *= .9f;
                            break;
                        default:
                            offsetX += Math.Sign((int)dir) * 1f;
                            offsetX = offsetX.Clamp(-4 * Globals.TILE, 4 * Globals.TILE);                            
                            break;
                    }

                    
                    if (lookLocked == 0)
                    {
                        switch ((target as Player).LookDirection)
                        {
                            case Direction.UP:
                                offsetY = Math.Max(offsetY - .5f, -3 * Globals.TILE);
                                break;
                            case Direction.DOWN:
                                offsetY = Math.Min(offsetY + .5f, 2 * Globals.TILE);
                                break;
                            default:
                                offsetY = -Globals.TILE;
                                break;
                        }
                    } else
                        offsetY = -Globals.TILE;                    
                }

                var tarX = Math.Min(Math.Max(target.X + offsetX, CurrentRoom.X + .5f * ViewWidth), CurrentRoom.X + CurrentRoom.BoundingBox.Width - .5f * ViewWidth);
                var tarY = Math.Min(Math.Max(target.Y + offsetY, CurrentRoom.Y + .5f * ViewHeight), CurrentRoom.Y + CurrentRoom.BoundingBox.Height - .5f * ViewHeight);

                if (Math.Abs(tarY - Position.Y) > 2 * Globals.TILE || Math.Abs(target.YVel) > 3)
                    dyVel = Math.Max(dyVel - .1f, 1);
                else
                    dyVel = Math.Min(dyVel + .1f, 12f);

                var vel = new Vector2((tarX - Position.X) / 12f, (tarY - Position.Y) / dyVel);

                if (lookLocked > 0)
                {
                    // move faster during lock-mode
                    vel = new Vector2((tarX - Position.X) / 8f, (tarY - Position.Y) / 8f);

                    if (MathUtil.Euclidean(Position, new Vector2(tarX,tarY)) < 2)
                        vel = new Vector2((tarX - Position.X) / 2f, (tarY - Position.Y) / 2f);
                }

                Position = new Vector2(Position.X + vel.X, Position.Y + vel.Y);
                
                if (lookLocked> 0)
                {
                    minX = Math.Max(lastRoom.X, CurrentRoom.X) + +.5f * ViewWidth;
                    maxX = Math.Min(lastRoom.X + Math.Min(lastRoom.BoundingBox.Width, CurrentRoom.BoundingBox.Width), CurrentRoom.X + Math.Min(lastRoom.BoundingBox.Width, CurrentRoom.BoundingBox.Width)) - .5f * ViewWidth;

                    minY = Math.Max(lastRoom.Y, CurrentRoom.Y) + .5f * ViewHeight;
                    maxY = Math.Min(lastRoom.Y + Math.Min(lastRoom.BoundingBox.Height, CurrentRoom.BoundingBox.Height), CurrentRoom.Y + Math.Min(lastRoom.BoundingBox.Height, CurrentRoom.BoundingBox.Height)) - .5f * ViewHeight;

                    if (moveDirection == LD.Horizontal)
                    {
                        Position = new Vector2(Position.X, Math.Min(Math.Max(target.Y, maxY), minY));

                        if (Math.Abs(Position.X - tarX) < .5f)
                        {
                            Position = new Vector2(tarX, Position.Y);
                            Debug.WriteLine("BEEP X");
                            lookLocked = 0;
                        }
                    }

                    if (moveDirection == LD.Vertical)
                    {
                        Position = new Vector2(Math.Min(Math.Max(target.X, maxX), minX), Position.Y);

                        if (Math.Abs(Position.Y - tarY) < .5f)
                        {
                            Position = new Vector2(Position.X, tarY);
                            Debug.WriteLine("BEEP Y");
                            lookLocked = 0;
                        }
                    }

                }
                //    Position = new Vector2(Position.X, Math.Min(Math.Max(target.Y + offsetY, CurrentRoom.Y + .5f * ViewHeight), CurrentRoom.Y + CurrentRoom.BoundingBox.Height - .5f * ViewHeight));

                // if outside view, try to find new room                
                if ((!MathUtil.In(target.X, CurrentRoom.X, CurrentRoom.X + CurrentRoom.BoundingBox.Width)
                        || !MathUtil.In(target.Y, CurrentRoom.Y, CurrentRoom.Y + CurrentRoom.BoundingBox.Height)))
                {

                    if (!target.X.In(CurrentRoom.X, CurrentRoom.X + CurrentRoom.BoundingBox.Width))
                        moveDirection = LD.Horizontal;
                    if (!target.Y.In(CurrentRoom.Y, CurrentRoom.Y + CurrentRoom.BoundingBox.Height))
                        moveDirection = LD.Vertical;
                    
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
            if (_backgrounds != null)
            {
                if (lastRoom != null)
                {
                    if (backgroundAlpha < 1)
                    {
                        if (lastBG != -1)
                            sb.Draw(_backgrounds[lastBG], Position - new Vector2(ViewWidth * .5f, ViewHeight * .5f), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.0001f);
                    }
                }
                if (CurrentRoom != null)
                {
                    var color = new Color(Color.White, backgroundAlpha);
                    if (CurrentBG != -1)
                        sb.Draw(_backgrounds[CurrentBG], Position - new Vector2(ViewWidth * .5f, ViewHeight *.5f), null, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0.0002f);
                }                
            }
        }        
    }
}
