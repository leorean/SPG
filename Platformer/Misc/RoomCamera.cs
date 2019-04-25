using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Objects;
using Platformer.Objects.Enemy;
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

namespace Platformer.Misc
{
    public class RoomCamera : Camera
    {

        public enum State
        {
            Default,
            PrepareRoomTransition,
            RoomTransition
        }
        
        private State state = State.Default;
        
        private GameObject target;
        
        // room <-> view calculation vars

        public List<Room> Rooms { get; private set; } = new List<Room>();

        private Room lastRoom;
        public Room CurrentRoom { get; private set; }
        
        private float curX, curY;
        private float tx0, ty0;
        private float tx, ty;

        // player-related vars

        private float dirOffsetX = 0;

        // background vars

        private TextureSet _backgrounds;
        private float backgroundAlpha = 1f;

        // events

        /// <summary>
        /// Is called when a room change is initiated. Provides last room and current room as arguments.
        /// </summary>
        public event EventHandler<Tuple<Room, Room>> OnRoomChange;
        
        public RoomCamera(ResolutionRenderer resolutionRenderer) : base(resolutionRenderer) { }
        
        public void InitRoomData(List<Dictionary<string, object>> roomData)
        {
            try
            {
                foreach (var data in roomData)
                {
                    var x = (int)data["x"];
                    var y = (int)data["y"];

                    var w = data.ContainsKey("width") ? (int)data["width"] : ViewWidth;
                    var h = data.ContainsKey("height") ? (int)data["height"] : ViewHeight;
                    var bg = data.ContainsKey("bg") ? (int)data["bg"] : 0;

                    var room = new Room(x, y, w, h);
                    room.Background = bg;
                    Rooms.Add(room);
                }

                // load rooms of standard size when there is none
                for(var i = 0; i < GameManager.Game.Map.Width * Globals.TILE; i += ViewWidth)
                {
                    for (var j = 0; j < GameManager.Game.Map.Height * Globals.TILE; j += ViewHeight)
                    {
                        if (ObjectManager.CollisionPoint<Room>(i + Globals.TILE, j + Globals.TILE).Count == 0)
                        {
                            var room = new Room(i, j, ViewWidth, ViewHeight);
                            Rooms.Add(room);                            
                        }
                    }
                }
            }
            catch(Exception)
            {
                Debug.WriteLine("Unable to initialize room from data!");
                throw;
            }
        }

        public void SetTarget(GameObject target)
        {
            this.target = target;
            Position = target != null ? target.Position : Vector2.Zero;
        }
        
        public override void Update(GameTime gt)
        {
            base.Update(gt);

            if (ObjectManager.ElapsedTime < ObjectManager.GameSpeed)
                return;
            
            if (target == null)
                return;

            ObjectManager.Enable<Room>();

            tx0 = MathUtil.Div(target.X, ViewWidth) * ViewWidth;
            ty0 = MathUtil.Div(target.Y, ViewHeight) * ViewHeight;

            // if no room is available, always resort to this state
            if (state == State.Default)
            {
                // if no room is yet found, try to find first room
                if (CurrentRoom == null)
                {
                    CurrentRoom = ObjectManager.CollisionPoint<Room>(target, target.X, target.Y).FirstOrDefault();
                    if (CurrentRoom != null)
                    {
                        lastRoom = CurrentRoom;
                        EnableBounds(new Rectangle((int)CurrentRoom.X, (int)CurrentRoom.Y, (int)CurrentRoom.BoundingBox.Width, (int)CurrentRoom.BoundingBox.Height));                        
                    }
                    else
                        return;
                }
                
                if (target is Player)
                {
                    var dir = (target as Player).Direction;

                    dirOffsetX += Math.Sign((int)dir) * 1f;
                    dirOffsetX = dirOffsetX.Clamp(-4 * Globals.TILE, 4 * Globals.TILE);
                }

                var tarX = Math.Min(Math.Max(target.X + dirOffsetX, CurrentRoom.X + .5f * ViewWidth), CurrentRoom.X + CurrentRoom.BoundingBox.Width - .5f * ViewWidth);
                var tarY = Math.Min(Math.Max(target.Y - Globals.TILE, CurrentRoom.Y + .5f * ViewHeight), CurrentRoom.Y + CurrentRoom.BoundingBox.Height - .5f * ViewHeight);
                
                var vel = new Vector2((tarX - Position.X) / 12f, (tarY - Position.Y) / 6f);

                Position = new Vector2(Position.X + vel.X, Position.Y + vel.Y);
                
                // if outside view, try to find new room                
                if ((!MathUtil.In(target.X, CurrentRoom.X, CurrentRoom.X + CurrentRoom.BoundingBox.Width)
                        || !MathUtil.In(target.Y, CurrentRoom.Y, CurrentRoom.Y + CurrentRoom.BoundingBox.Height)))
                {
                    lastRoom = CurrentRoom;
                    CurrentRoom = ObjectManager.CollisionPoint<Room>(target, target.X, target.Y).FirstOrDefault();

                    if (CurrentRoom != null)
                    {
                        dirOffsetX = 0;
                        state = State.PrepareRoomTransition;
                        
                        // switch backgrounds           
                        backgroundAlpha = 0;
                    }
                }
            }

            // prepare for transition
            if (state == State.PrepareRoomTransition)
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

        internal void Reset()
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

        public void DrawBackground(GameTime gt)
        {
            if (_backgrounds != null)
            {
                if (lastRoom != null)
                {
                    if (backgroundAlpha < 1)
                    {
                        GameManager.Game.SpriteBatch.Draw(_backgrounds[lastRoom.Background], Position - new Vector2(ViewWidth * .5f, ViewHeight * .5f), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.0001f);
                    }
                }
                if (CurrentRoom != null)
                {
                    var color = new Color(Color.White, backgroundAlpha);
                    GameManager.Game.SpriteBatch.Draw(_backgrounds[CurrentRoom.Background], Position - new Vector2(ViewWidth * .5f, ViewHeight *.5f), null, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0.0002f);
                }                
            }
        }
    }
}
