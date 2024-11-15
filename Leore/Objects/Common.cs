﻿using Leore.Main;
using SPG.Objects;
using System.Collections.Generic;

namespace Leore
{
    public enum Direction
    {
        NONE = 0,
        LEFT = -1,
        RIGHT = 1,
        UP = -2,
        DOWN = 2
    }    

    public static class ObjectExtensions
    {
        public static bool IsOutsideCurrentRoom(this GameObject o, float threshold = 0)
        {
            var cam = RoomCamera.Current;
            
            if (cam == null)
                return true;

            //return o.X < cam.ViewX || o.Y < cam.ViewY || o.X > cam.ViewX + cam.ViewWidth || o.Y > cam.ViewY + cam.ViewHeight;

            if (cam.CurrentRoom == null || o == null)
                return false;

            return o.X < cam.CurrentRoom.X - threshold || o.Y < cam.CurrentRoom.Y - threshold || o.X > cam.CurrentRoom.X + cam.CurrentRoom.BoundingBox.Width + threshold || o.Y > cam.CurrentRoom.Y + cam.CurrentRoom.BoundingBox.Height + threshold;
        }

        public static int IndexOf<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            int i = 0;
            foreach (var pair in dictionary)
            {
                if (pair.Key.Equals(key))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }
    }    
}