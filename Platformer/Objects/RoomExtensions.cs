using Platformer.Main;
using SPG.Objects;
using System.Collections.Generic;
using System.Linq;

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
                if (i < 0)
                    continue;
                
                for (var j = room.Y - .5f * camera.ViewHeight; j <= room.Y + room.BoundingBox.Height + .5f * camera.ViewHeight; j += camera.ViewHeight)
                {
                    if (j < 0)
                        continue;

                    var candidates = ObjectManager.CollisionPoints<Room>(room, i, j).Where(o => !neighbours.Contains(o));
                    foreach (var c in candidates)
                        if (!neighbours.Contains(c))
                        {
                            //Debug.WriteLine($"{c.X}, {c.Y}, ... {c.BoundingBox}");
                            neighbours.Add(c);
                        }
                }
            }
            return neighbours;
        }
    }
}
