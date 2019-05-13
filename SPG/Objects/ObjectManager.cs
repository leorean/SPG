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
        
        //private static int idCounter;

        public static double ElapsedTime { get; private set; } = 0;
        
        public static double GameSpeed { get; set; }
        public static RectF Region { get; set; }

        public static void Add(GameObject o)
        {
            if (!Objects.Contains(o))
            {
                //idCounter++;
                Objects.Add(o);
                //return idCounter;
            } else
            {                
                throw new Exception("Object already registered!");
            }
        }

        public static void CreateID(this GameObject o)
        {
            string strX = MathUtil.Div(o.X, Globals.TILE).ToString();
            string strY = MathUtil.Div(o.Y, Globals.TILE).ToString();

            o.ID = int.Parse(strX + strY);
        }

        public static bool Exists<T>() where T:GameObject
        {
            return Objects.Find(x => x is T) != null;
        }

        public static bool Exists(GameObject target)
        {
            return target != null && Objects.Find(x => x == target) != null;
        }

        /// <summary>
        /// Are you sure you need to use this? Try using object.Destroy() instead.
        /// </summary>
        /// <param name="o"></param>
        public static void Remove(GameObject o)
        {
            if (Objects.Contains(o))
                Objects.Remove(o);
        }

        /// <summary>
        /// Destroys all game objects that have a specific ID. If not set, it simply destroys all of that type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="specificID"></param>
        public static void DestroyAll<T>(int specificID = -1) where T : GameObject
        {
            var candidates = Objects.Where(o => o is T && ((o.ID == specificID) || specificID == -1)).Cast<T>().ToList();
            T[] array = new T[candidates.Count];

            candidates.CopyTo(array);

            foreach(var obj in array)
            {
                obj.Destroy();
            }            
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

        /// <summary>
        /// Returns the number of alive objects of a given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int Count<T>() where T : GameObject
        {
            return Objects.Where(o => o is T).Count();
        }

        /// <summary>
        /// Disables all objects of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Disable<T>() where T : GameObject
        {
            Objects.Where(o => o is T).ToList().ForEach(o => o.Enabled = false);
        }

        /// <summary>
        /// Enables all objects of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Enable<T>() where T : GameObject
        {
            Objects.Where(o => o is T).ToList().ForEach(o => o.Enabled = true);
        }

        public static List<T> CollisionPoint<T>(float x, float y) where T : GameObject
        {
            return CollisionPoint<T>(null, x, y);
        }

        /// <summary>
        /// Finds a list of game objects (excluding self) of a certain type that overlap a certain point (x, y).
        /// </summary>
        /// <param name="self"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<T> CollisionPoint<T>(this GameObject self, float x, float y) where T : GameObject
        {
            List<T> candidates = Objects.Where(o => o != self && o is T && o.Enabled == true).Cast<T>().ToList();

            if (candidates.Count == 0) return candidates;

            candidates = candidates.Where(
                o => 
                    MathUtil.In(x, o.Left, o.Right) 
                    && MathUtil.In(y, o.Top, o.Bottom)
                ).ToList();
            
            return candidates;
        }

        public static bool CollisionRectangle(GameObject o, float x1, float y1, float x2, float y2)
        {
            return (x2 >= o.Left && x1 <= o.Right)
                    &&
                    (y2 >= o.Top && y1 <= o.Bottom);
            
        }

        public static List<T> CollisionRectangle<T>(this GameObject self, float x1, float y1, float x2, float y2) where T : GameObject
        {
            List<T> candidates = Objects.Where(o => o != self && o is T && o.Enabled == true).Cast<T>().ToList();

            if (candidates.Count == 0) return candidates;

            candidates = candidates.Where(
                o =>
                    (x2 >= o.Left && x1 <= o.Right)
                    &&
                    (y2 >= o.Top && y1 <= o.Bottom)
                ).ToList();

            return candidates;
        }

        public static bool CollisionBounds(this GameObject self, GameObject other, float x, float y)
        {
            if (other == null) return false;

            if (other.Right >= x + self.BoundingBox.X && other.Left <= x + self.BoundingBox.X + self.BoundingBox.Width
                    &&
                    other.Bottom >= y + self.BoundingBox.Y && other.Top <= y + self.BoundingBox.Y + self.BoundingBox.Height)
                return true;

            return false;
        }

        /// <summary>
        /// Finds a list of game objects of a certain type that have their bounding coordinates within the own bounding coordinates, based on x and y value.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<T> CollisionBounds<T>(this GameObject self, float x, float y) where T : GameObject
        {
            List<T> candidates = Objects.Where(o => o != self && o is T && o.Enabled == true).Cast<T>().ToList();

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
        /// Sets a region of a type T of objects enabled or disabled.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="enabled"></param>
        public static void SetRegionEnabled<T>(float x, float y, float width, float height, bool enabled) where T : GameObject
        {
            foreach (var o in Objects)
            {
                if (o is T)
                {
                    if (o.X >= x && o.Y >= y && o.X < x + width && o.Y < y + height) o.Enabled = enabled;
                }
            }
        }

        /// <summary>
        /// Call this in your game Update method.
        /// </summary>
        public static void UpdateObjects(GameTime gameTime)
        {
            if (ElapsedTime > GameSpeed)
            {
                ElapsedTime -= GameSpeed;
                Objects.Where(o => o.Enabled == true).ToList().ForEach(o => o.BeginUpdate(gameTime));
                Objects.Where(o => o.Enabled == true).ToList().ForEach(o => o.Update(gameTime));
                Objects.Where(o => o.Enabled == true).ToList().ForEach(o => o.EndUpdate(gameTime));
            }
            ElapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;            
        }

        /// <summary>
        /// Call this between the SpriteBatch.Begin and SpriteBatch.End in your game Draw method.
        /// </summary>
        public static void DrawObjects(SpriteBatch sb, GameTime gameTime)
        {
            SortByID();
            Objects.Where(o => o.Visible).ToList().ForEach(o => o.Draw(sb, gameTime));
        }

        
    }
}

