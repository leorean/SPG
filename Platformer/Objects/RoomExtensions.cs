using Platformer.Main;
using SPG;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects
{
    public static class RoomExtensions
    {
        /// <summary>
        /// Gets a list of all neighbors for a given room, based on the room camera view bounds.
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static List<Room> Neighbors(this Room room)
        {
            List<Room> neighbours = new List<Room>();

            var camera = RoomCamera.Current;

            for (var i = room.X - .5f * camera.ViewWidth; i <= room.X + room.BoundingBox.Width + .5f * camera.ViewWidth; i += camera.ViewWidth)
            {
                for (var j = room.Y - .5f * camera.ViewHeight; j <= room.Y + room.BoundingBox.Height + .5f * camera.ViewHeight; j += camera.ViewHeight)
                {
                    var candidates = ObjectManager.CollisionPoint<Room>(room, i, j);
                    foreach (var c in candidates)
                        if (!neighbours.Contains(c))
                            neighbours.Add(c);
                }
            }
            //Debug.WriteLine($"Found {neighbours.Count} neighbors.");

            return neighbours;
        }
    }
}
