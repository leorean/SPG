using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SPG.Util;

namespace SPG.Objects
{
    public static class ObjectManager
    {
        public static List<GameObject> Objects { get; private set; } = new List<GameObject>();
        
        private static int idCounter;

        public static double ElapsedTime { get; private set; } = 0;
        
        public static double GameSpeed { get; set; }
        public static RectF Region { get; set; }

        public static int Add(GameObject o)
        {
            if (!Objects.Contains(o))
            {
                idCounter++;
                Objects.Add(o);
                return idCounter;
            } else
            {
                throw new Exception("Object already added to the object manager!");
            }
        }

        public static void Remove(GameObject o)
        {
            if (Objects.Contains(o))
                Objects.Remove(o);            
        }

        public static void SortByID()
        {
            Objects.Sort(
                delegate (GameObject o1, GameObject o2)
                {
                    if (o1.ID < o2.ID) return -1;
                    if (o1.ID > o2.ID) return 1;
                    return 0;
                }
            );
        }

        public static void SortByX(this List<GameObject> objects)
        {
            objects.Sort(delegate (GameObject o1, GameObject o2)
            {
                if (o1.X < o2.X) return -1;
                if (o1.X > o2.X) return 1;
                return 0;
            });
        }

        public static void SortByY(this List<GameObject> objects)
        {
            objects.Sort(delegate (GameObject o1, GameObject o2)
            {
                if (o1.Y < o2.Y) return -1;
                if (o1.Y > o2.Y) return 1;
                return 0;
            });
        }

        public static void DisableAll(Type objectType)
        {
            Objects.Where(o => o.GetType() == objectType).ToList().ForEach(o => o.Enabled = false);            
        }

        public static void EnableAll(Type objectType)
        {
            Objects.Where(o => o.GetType() == objectType).ToList().ForEach(o => o.Enabled = true);
        }

        /// <summary>
        /// Finds a list of game objects of a certain type that overlap a certain point x , y.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<T> CollisionPoint<T>(GameObject self, float x, float y) where T : GameObject
        {
            List<T> candidates = Objects.Where(o => o != self && o.GetType() == typeof(T) && o.Enabled == true).Cast<T>().ToList();

            if (candidates.Count == 0) return candidates;

            candidates = candidates.Where(
                o => 
                    MathUtil.In(x, o.Left, o.Right) 
                    && MathUtil.In(y, o.Top, o.Bottom)
                ).ToList();
            
            return candidates;
        }

        /// <summary>
        /// Finds a list of game objects of a certain type that have their bounding coordinates within the own bounding coordinates, based on x and y value.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<T> CollisionBounds<T>(GameObject self, float x, float y) where T : GameObject
        {
            List<T> candidates = Objects.Where(o => o != self && o.GetType() == typeof(T) && o.Enabled == true).Cast<T>().ToList();

            if (candidates.Count == 0) return candidates;
            
            candidates = candidates.Where(
                o => 
                    o.Right >= x + self.BoundingBox.X && o.Left <= x + self.BoundingBox.X + self.BoundingBox.Width
                    &&
                    o.Bottom >= y + self.BoundingBox.Y && o.Top <= y + self.BoundingBox.Y + self.BoundingBox.Height
                ).ToList();

            return candidates;
        }

        /// <summary>
        /// Call this in your game Update method.
        /// </summary>
        public static void UpdateObjects(GameTime gameTime)
        {
            if (ElapsedTime > GameSpeed)
            {
                ElapsedTime -= GameSpeed;
                Objects.Where(o => o.Enabled).ToList().ForEach(o => o.Update(gameTime));
            }
            ElapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;            
        }

        /// <summary>
        /// Call this between the SpriteBatch.Begin and SpriteBatch.End in your game Draw method.
        /// </summary>
        public static void DrawObjects(GameTime gameTime)
        {
            SortByID();
            Objects.Where(o => o.Visible).ToList().ForEach(o => o.Draw(gameTime));
        }

        public static void SetRegionEnabled(float x, float y, float width, float height, bool enabled)
        {
            foreach (var o in Objects)
            {
                if (o.X >= x && o.Y >= y && o.X <= x + width && o.Y <= y + height) o.Enabled = enabled;
            }
        }
    }
}

