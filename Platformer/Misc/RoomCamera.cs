using Microsoft.Xna.Framework;
using Platformer.Objects;
using SPG;
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
        
        private int viewWidth;
        private int viewHeight;
        
        private GameObject target;
        
        private Room lastRoom;
        private Room currentRoom;
        
        private bool moveX = false;
        private bool moveY = false;

        float curX, curY;

        private float tx0, ty0;
        private float tx, ty;
        
        public RoomCamera(ResolutionRenderer resolutionRenderer) : base(resolutionRenderer) { }

        public void InitRoomData(List<Dictionary<string, object>> roomData)
        {
            try
            {
                viewWidth = ResolutionRenderer.ViewWidth;
                viewHeight = ResolutionRenderer.ViewHeight;
                
                //Rooms = new List<Room>();

                foreach (var data in roomData)
                {
                    var x = (int)data["x"];
                    var y = (int)data["y"];

                    var w = data.ContainsKey("width") ? (int)data["width"] : viewWidth;
                    var h = data.ContainsKey("height") ? (int)data["height"] : viewHeight;
                    var bg = data.ContainsKey("bg") ? (int)data["bg"] : 0;

                    var room = new Room(x, y, w, h);
                    room.Background = bg;

                    //Rooms.Add(room);
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
        }

        public override void Update(GameTime gt)
        {
            base.Update(gt);

            if (ObjectManager.ElapsedTime < ObjectManager.GameSpeed)
                return;
            
            if (target == null)
                return;

            Position = target.Position;

            tx0 = MathUtil.Div(target.X, viewWidth) * viewWidth;
            ty0 = MathUtil.Div(target.Y, viewHeight) * viewHeight;

            // if no room is available, always resort to this state
            if (state == State.Default)
            {
                // if no room is yet found, try to find first room
                if (currentRoom == null)
                {
                    currentRoom = ObjectManager.CollisionPoint<Room>(target, target.X, target.Y).FirstOrDefault();
                    if (currentRoom != null)
                    {
                        lastRoom = currentRoom;
                        EnableBounds(new Rectangle((int)currentRoom.X, (int)currentRoom.Y, (int)currentRoom.BoundingBox.Width, (int)currentRoom.BoundingBox.Height));
                    }
                }

                // if outside view, try to find new room
                if (!MathUtil.In(target.X, bounds.X, bounds.X + bounds.Width)
                        || !MathUtil.In(target.Y, bounds.Y, bounds.Y + bounds.Height))
                {
                    lastRoom = currentRoom;
                    currentRoom = ObjectManager.CollisionPoint<Room>(target, target.X, target.Y).FirstOrDefault();

                    if (currentRoom != null)
                    {
                        state = State.PrepareRoomTransition;
                    }
                }
            }

            // prepare for transition
            if (state == State.PrepareRoomTransition)
            {
                DisableBounds();

                moveX = (currentRoom.X != lastRoom.X);
                moveY = (currentRoom.Y != lastRoom.Y);

                tx = Position.X;
                ty = Position.Y;

                curX = tx;
                curY = ty;

                state = State.RoomTransition;
            }

            // do transition until eligable for default state
            if (state == State.RoomTransition)
            {
                if (currentRoom == null)
                {
                    state = State.Default;
                    return;
                }

                if (!moveX)
                    tx = viewWidth * .5f + tx0;
                else
                {
                    tx = Math.Min(Math.Max(target.X, currentRoom.X + viewWidth * .5f), currentRoom.X + currentRoom.BoundingBox.Width - .5f * viewWidth + .5f);
                }

                if (!moveY)
                    ty = viewHeight * .5f + ty0;
                else
                {
                    ty = Math.Min(Math.Max(target.Y, currentRoom.Y + viewHeight * .5f), currentRoom.Y + currentRoom.BoundingBox.Height - .5f * viewHeight + .5f);
                }

                curX += (tx - curX) / 6f;
                curY += (ty - curY) / 6f;

                if (Math.Abs(curX - tx) < 1)
                    curX = tx;
                if (Math.Abs(curY - ty) < 1)
                    curY = ty;
                
                Vector2 t = new Vector2(curX, curY);                
                Position = t;

                if (curX == tx && curY == ty)
                {
                    state = State.Default;
                    //EnableBounds(new Rectangle((int)currentRoom.X, (int)currentRoom.Y, (int)currentRoom.BoundingBox.Width, (int)currentRoom.BoundingBox.Height));
                }
            }            
        }

        public void DrawBackground(GameTime gt)
        {

        }
    }
}
