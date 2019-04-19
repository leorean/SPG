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
            None,
            PrepareRoomTransition,
            OpenTransition,
            CloseTransition
        }

        private State state = State.None;
        

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

            if (target == null)
                return;

            if (state == State.None)
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

                if (!MathUtil.In(target.X, bounds.X, bounds.X + bounds.Width)
                    || !MathUtil.In(target.Y, bounds.Y, bounds.Y + bounds.Height))
                {
                    lastRoom = currentRoom;
                    currentRoom = ObjectManager.CollisionPoint<Room>(target, target.X, target.Y).FirstOrDefault();

                    if (currentRoom != null)
                        state = State.PrepareRoomTransition;
                }
            }

            if (state == State.PrepareRoomTransition)
            {
                curX = (int)lastRoom.X;
                curY = (int)lastRoom.Y;
                curW = (int)lastRoom.BoundingBox.Width;
                curH = (int)lastRoom.BoundingBox.Height;

                tarX = (int)Math.Min(currentRoom.X, lastRoom.X);
                tarY = (int)Math.Min(currentRoom.Y, lastRoom.Y);
                tarW = (int)Math.Max(currentRoom.BoundingBox.Width + currentRoom.X, lastRoom.BoundingBox.Width + lastRoom.X);
                tarH = (int)Math.Max(currentRoom.BoundingBox.Height + currentRoom.Y, lastRoom.BoundingBox.Height + lastRoom.Y);
                
                state = State.OpenTransition;                
            }

            if (state == State.OpenTransition)
            {
                var spdX = (tarX - curX) / 4f;
                var spdY = (tarY - curY) / 4f;
                var spdW = (tarW - curW) / 4f;
                var spdH = (tarH - curH) / 4f;

                curX += spdX;
                curY += spdY;
                curW += spdW;
                curH += spdH;

                if (Math.Round(spdX) == 0 && Math.Round(spdY) == 0 && Math.Round(spdW) == 0 && Math.Round(spdH) == 0)
                {
                    curX = (int)Math.Min(currentRoom.X, lastRoom.X);
                    curY = (int)Math.Min(currentRoom.Y, lastRoom.Y);
                    curW = (int)Math.Max(currentRoom.BoundingBox.Width + currentRoom.X, lastRoom.BoundingBox.Width + lastRoom.X);
                    curH = (int)Math.Max(currentRoom.BoundingBox.Height + currentRoom.Y, lastRoom.BoundingBox.Height + lastRoom.Y);

                    tarX = (int)currentRoom.X;
                    tarY = (int)currentRoom.Y;
                    tarW = (int)currentRoom.BoundingBox.Width;
                    tarH = (int)currentRoom.BoundingBox.Height;

                    state = State.CloseTransition;
                }
                
                EnableBounds(new Rectangle((int)curX, (int)curY, (int)curW, (int)curH));
            }

            if (state == State.CloseTransition)
            {
                var spdX = (tarX - curX) / 4f;
                var spdY = (tarY - curY) / 4f;
                var spdW = (tarW - curW) / 4f;
                var spdH = (tarH - curH) / 4f;

                curX += spdX;
                curY += spdY;
                curW += spdW;
                curH += spdH;

                if (Math.Round(spdX) == 0 && Math.Round(spdY) == 0 && Math.Round(spdW) == 0 && Math.Round(spdH) == 0)
                {
                    curX = (int)currentRoom.X;
                    curY = (int)currentRoom.Y;
                    curW = (int)currentRoom.BoundingBox.Width;
                    curH = (int)currentRoom.BoundingBox.Height;

                    state = State.None;
                }

                EnableBounds(new Rectangle((int)curX, (int)curY, (int)curW, (int)curH));
            }
            
            Position = target.Position;            
        }
    }
}
