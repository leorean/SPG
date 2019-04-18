using Microsoft.Xna.Framework;
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

        private int roomWidth;
        private int roomHeight;

        // stores how many rooms fit in one map
        private int totalWidth;
        private int totalHeight;

        private GameObject target;

        private Grid<Dictionary<string, object>> RoomData = new Grid<Dictionary<string, object>>();

        public RoomCamera(ResolutionRenderer resolutionRenderer) : base(resolutionRenderer) { }
        
        public void SetRoomSize(int width, int height)
        {
            roomWidth = width;
            roomHeight = height;
        }

        public void InitRoomData(List<Dictionary<string, object>> roomData)
        {
            try
            {
                totalWidth = GameManager.Game.Map.Width / roomWidth;
                totalHeight = GameManager.Game.Map.Height / roomHeight;

                RoomData = new Grid<Dictionary<string, object>>(totalWidth, totalHeight);

                foreach (var data in roomData)
                {
                    var x = MathUtil.Div((int)data["x"], Globals.TILE * roomWidth);
                    var y = MathUtil.Div((int)data["y"], Globals.TILE * roomHeight);

                    var w = data.ContainsKey("w") ? (int)data["w"] : 1;
                    var h = data.ContainsKey("h") ? (int)data["h"] : 1;
                    var bg = data.ContainsKey("bg") ? (int)data["bg"] : 0;

                    var roomInfo = new Dictionary<string, object>();
                    roomInfo.Add("bg", bg);
                    roomInfo.Add("w", w);
                    roomInfo.Add("h", h);

                    RoomData.Set(x, y, roomInfo);
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

            if (target != null)
            {
                //camera.EnableBounds(new Rectangle(0, 0, Map.Width * Globals.TILE, Map.Height * Globals.TILE));

                var tx = MathUtil.Div(target.X, roomWidth * Globals.TILE) * roomWidth * Globals.TILE + .5f * roomWidth * Globals.TILE;
                var ty = MathUtil.Div(target.Y, roomHeight * Globals.TILE) * roomHeight * Globals.TILE + .5f * roomHeight * Globals.TILE;

                var roomX = MathUtil.Div(tx, roomWidth * Globals.TILE);
                var roomY = MathUtil.Div(ty, roomHeight * Globals.TILE);

                //Debug.WriteLine($"{roomX} , {roomY}");

                var currentRoomData = RoomData.Get(roomX, roomY);

                if (currentRoomData != null)
                {

                }

                Position = new Vector2(tx, ty);

            }

            
        }
    }
}
