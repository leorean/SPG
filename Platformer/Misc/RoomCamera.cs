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

        Vector2 targetSpd;

        private Room lastRoom;
        private Room currentRoom;

        private List<Room> Rooms;

        public RoomCamera(ResolutionRenderer resolutionRenderer) : base(resolutionRenderer) { }
        
        float curX, curY, curW, curH, tarX, tarY, tarW, tarH;

        public void InitRoomData(List<Dictionary<string, object>> roomData)
        {
            try
            {
                viewWidth = ResolutionRenderer.ViewWidth;
                viewHeight = ResolutionRenderer.ViewHeight;
                
                Rooms = new List<Room>();

                foreach (var data in roomData)
                {
                    var x = (int)data["x"];
                    var y = (int)data["y"];

                    var w = data.ContainsKey("width") ? (int)data["width"] : viewWidth;
                    var h = data.ContainsKey("height") ? (int)data["height"] : viewHeight;
                    var bg = data.ContainsKey("bg") ? (int)data["bg"] : 0;

                    var room = new Room(x, y, w, h);
                    room.Background = bg;

                    Rooms.Add(room);
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
            {
                return;
            }
            
            if (target == null)
                return;

            // if no room is available, always resort to this state
            if (state == State.Default)
            {
                if (currentRoom == null)
                {
                    currentRoom = ObjectManager.CollisionPoint<Room>(target, target.X, target.Y).FirstOrDefault();
                    if (currentRoom != null)
                    {
                        lastRoom = currentRoom;
                        EnableBounds(new Rectangle((int)currentRoom.X, (int)currentRoom.Y, (int)currentRoom.BoundingBox.Width, (int)currentRoom.BoundingBox.Height));
                    }
                }                
            }

            // if outside view, try to find new room
            if (!MathUtil.In(target.X, bounds.X, bounds.X + bounds.Width)
                    || !MathUtil.In(target.Y, bounds.Y, bounds.Y + bounds.Height))
            {
                lastRoom = currentRoom;
                currentRoom = ObjectManager.CollisionPoint<Room>(target, target.X, target.Y).FirstOrDefault();

                if (currentRoom != null)
                    state = State.PrepareRoomTransition;
            }

            // prepare for transition
            if (state == State.PrepareRoomTransition)
            {
                curX = (int)(Position.X - .5f * viewWidth);
                curY = (int)(Position.Y - .5f * viewHeight);
                curW = (int)viewWidth;
                curH = (int)viewHeight;

                tarX = (int)currentRoom.X;
                tarY = (int)currentRoom.Y;
                tarW = (int)currentRoom.BoundingBox.Width;
                tarH = (int)currentRoom.BoundingBox.Height;

                state = State.RoomTransition;                
            }
            
            // do transition until eligable for default state
            if (state == State.RoomTransition)
            {
                if (currentRoom == null)
                    state = State.Default;

                var s = 10f;
                var max = 8;

                var spdX = (tarX - curX) / s;
                var spdY = (tarY - curY) / s;
                var spdW = (tarW - curW) / s;
                var spdH = (tarH - curH) / s;

                //spdX = Math.Sign(spdX) * Math.Min(Math.Abs(spdX), max);
                //spdY = Math.Sign(spdY) * Math.Min(Math.Abs(spdY), max);
                //spdW = Math.Sign(spdW) * Math.Min(Math.Abs(spdW), max);
                //spdH = Math.Sign(spdH) * Math.Min(Math.Abs(spdH), max);

                curX += spdX;
                curY += spdY;
                curW += spdW;
                curH += spdH;

                Debug.WriteLine($"{curX}, {curY}, {curW}, {curH}");

                var prec = 3;

                if (Math.Round(spdX * prec) == 0 && Math.Round(spdY * prec) == 0 && Math.Round(spdW * prec) == 0 && Math.Round(spdH * prec) == 0)
                {
                    curX = (int)currentRoom.X;
                    curY = (int)currentRoom.Y;
                    curW = (int)currentRoom.BoundingBox.Width;
                    curH = (int)currentRoom.BoundingBox.Height;

                    state = State.Default;                    
                }

                EnableBounds(new Rectangle((int)Math.Ceiling(curX), (int)Math.Ceiling(curY), (int)curW, (int)curH));
            }
            
            Position = target.Position;            
        }
    }
}
