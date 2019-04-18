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
            Transitioning,
            Locked
        }

        private State state;
        

        private int viewWidth;
        private int viewHeight;
        
        private GameObject target;

        private Room currentRoom;

        private List<Room> Rooms;

        public RoomCamera(ResolutionRenderer resolutionRenderer) : base(resolutionRenderer) { }
        
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

            var room = ObjectManager.CollisionPoint(target, target.X, target.Y, typeof(Room)).FirstOrDefault();
            
            if (room != null)
            {                
                EnableBounds(new Rectangle((int)room.X, (int)room.Y, (int)room.BoundingBox.Width, (int)room.BoundingBox.Height));

            }
            
            var tx = target.X;
            var ty = target.Y;

            Position = new Vector2(tx, ty);

            /*if (target != null)
            {
                if (state == State.Transitioning)
                {
                    var tx = MathUtil.Div(target.X, roomWidth * Globals.TILE) * roomWidth * Globals.TILE + .5f * roomWidth * Globals.TILE;
                    var ty = MathUtil.Div(target.Y, roomHeight * Globals.TILE) * roomHeight * Globals.TILE + .5f * roomHeight * Globals.TILE;

                    var roomX = MathUtil.Div(tx, roomWidth * Globals.TILE);
                    var roomY = MathUtil.Div(ty, roomHeight * Globals.TILE);

                    //Debug.WriteLine($"{roomX} , {roomY}");

                    var currentRoomData = Rooms.Get(roomX, roomY);

                    if (currentRoomData != null)
                    {
                        var w = (int)(currentRoomData.ContainsKey("w") ? currentRoomData["w"] : 1) * roomWidth * Globals.TILE;
                        var h = (int)(currentRoomData.ContainsKey("h") ? currentRoomData["h"] : 1) * roomHeight * Globals.TILE;

                        var x = (int)roomX * roomWidth * Globals.TILE;
                        var y = (int)roomY * roomHeight * Globals.TILE;

                        EnableBounds(new Rectangle(x, y, w, h));

                        state = State.Locked;
                    }
                }

                Position = new Vector2(target.X, target.Y);

                if (!MathUtil.In(target.X, Position.X + bounds.X, Position.X + bounds.Width)
                    ||
                    !MathUtil.In(target.Y, Position.Y + bounds.Y, Position.Y + bounds.Height))
                {
                    state = State.Transitioning;
                }
            }*/
        }
    }
}
