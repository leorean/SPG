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
using SPG.Exceptions;
using SPG.Util;

namespace SPG.Objects
{
    public static class ObjectManager
    {
        public static List<GameObject> Objects { get; set; } = new List<GameObject>();

        public static List<GameObject> ActiveObjects { get; private set; } = new List<GameObject>();
        
        public static double ElapsedTime { get; private set; } = 0;
        
        public static double GameDelay { get; set; }
        public static RectF Region { get; set; }

        public static long GlobalIDCounter { get; private set; }
        
        public static void Add(GameObject o)
        {
            if (!Objects.Contains(o))
            {
                Objects.Add(o);
                ActiveObjects.Add(o);

                GlobalIDCounter++;
            } else
            {
                throw new ObjectException(o, "Object already registered!");
            }
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
            {
                Objects.Remove(o);                
                //UpdateActiveObjectList();
            }
            if (ActiveObjects.Contains(o))
                ActiveObjects.Remove(o);
        }

        /// <summary>
        /// Destroys all game objects that have a specific ID. If not set, it simply destroys all of that type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="specificID"></param>
        public static void DestroyAll<T>(IID specificID = null) where T : GameObject
        {
            for (var i = 0; i < Objects.Count; i++)
            {
                var o = Objects[i];

                if (!(o is T))
                    continue;
                if (specificID == null || o.IDEquals(specificID))
                    o.Destroy();
            }
        }
        
        /// <summary>
        /// Returns the number of alive objects of a given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int Count<T>() where T : GameObject
        {
            int t = 0;
            for (var i = 0; i < Objects.Count; i++)
            {
                if (Objects[i] is T)
                    t++;
            }
            return t;
        }

        /// <summary>
        /// Disables all objects of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Disable<T>()
        {
            for (var i = 0; i < ActiveObjects.Count; i++)
            {
                var o = ActiveObjects[i];

                if (!(o is T))
                    continue;

                o.Enabled = false;
            }

            UpdateActiveObjectList();
        }

        /// <summary>
        /// Enables all objects of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Enable<T>()
        {
            for (var i = 0; i < Objects.Count; i++)
            {
                var o = Objects[i];

                if (!(o is T))
                    continue;

                o.Enabled = true;
            }

            UpdateActiveObjectList();
        }

        public static void Enable(GameObject gameObject)
        {
            Enable(gameObject, true);
        }

        private static void Enable(GameObject gameObject, bool updateActiveObjectList)
        {
            if (gameObject == null)
                return;

            gameObject.Enabled = true;

            if (updateActiveObjectList)
            {
                UpdateActiveObjectList();
            }
        }

        public static List<T> CollisionPoints<T>(float x, float y) where T : GameObject
        {
            return CollisionPoints<T>(null, x, y);
        }

        public static T CollisionPointFirstOrDefault<T>(float x, float y) where T : GameObject
        {
            return CollisionPointFirstOrDefault<T>(null, x, y);
        }

        /// <summary>
        /// Finds a list of game objects (excluding self) of a certain type that overlap a certain point (x, y).
        /// </summary>
        /// <param name="self"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<T> CollisionPoints<T>(this ICollidable self, float x, float y) where T : ICollidable
        {
            List<T> list = new List<T>();

            for (var i = 0; i < ActiveObjects.Count; i++)
            {
                var o = ActiveObjects[i];
                
                if (!(o is T))
                    continue;
                if (o == self)
                    continue;

                if (MathUtil.In(x, o.Left, o.Right)
                    && MathUtil.In(y, o.Top, o.Bottom))
                    list.Add((T)(object)o);
            }

            return list;
        }

        public static T CollisionPointFirstOrDefault<T>(this ICollidable self, float x, float y) where T : ICollidable
        {
            for (var i = 0; i < ActiveObjects.Count; i++)
            {
                var o = ActiveObjects[i];
                
                if (!(o is T))
                    continue;
                if (o == self)
                    continue;

                if (MathUtil.In(x, o.Left, o.Right)
                    && MathUtil.In(y, o.Top, o.Bottom))
                    return (T)(object)o;
            }

            return default(T);
        }

        public static bool CollisionRectangle(ICollidable o, float x1, float y1, float x2, float y2)
        {
            return (x2 >= o.Left && x1 <= o.Right)
                    &&
                    (y2 >= o.Top && y1 <= o.Bottom);
            
        }

        public static List<T> CollisionRectangles<T>(this ICollidable self, float x1, float y1, float x2, float y2) where T : ICollidable
        {
            List<T> list = new List<T>();

            for (var i = 0; i < ActiveObjects.Count; i++)
            {
                var o = ActiveObjects[i];
                
                if (!(o is T))
                    continue;
                if (o == self)
                    continue;

                if ((x2 >= o.Left && x1 <= o.Right)
                    &&
                    (y2 >= o.Top && y1 <= o.Bottom))
                {
                    list.Add((T)(object)o);
                }
            }
            return list;
        }

        public static T CollisionRectangleFirstOrDefault<T>(this ICollidable self, float x1, float y1, float x2, float y2) where T : ICollidable
        {
            for (var i = 0; i < ActiveObjects.Count; i++)
            {
                var o = ActiveObjects[i];
                
                if (!(o is T))
                    continue;
                if (o == self)
                    continue;

                if ((x2 >= o.Left && x1 <= o.Right)
                    &&
                    (y2 >= o.Top && y1 <= o.Bottom))
                {
                    return (T)(object)o;
                }
            }
            return default(T);
        }

        /// <summary>
        /// Returns the first object hit by a raycast in a given angle within n steps at a maximum distance.
        /// </summary>
        /// <param name="degAngle"></param>
        /// <param name="n"></param>
        /// <param name="maxDist"></param>
        /// <returns></returns>
        public static (T, float) RayCast<T>(this GameObject obj, int degAngle, float n = 1, float maxDist = 9999) where T : ICollidable
        {
            var pos = obj.Center;
            var kx = MathUtil.LengthDirX(degAngle);
            var ky = MathUtil.LengthDirY(degAngle);

            for (float i = 0; i < maxDist; i += n)
            {
                var collision = obj.CollisionPointFirstOrDefault<T>(pos.X + kx * i, pos.Y + ky * i);
                if (collision != null)
                {
                    return (collision, i);
                }
            }
            return (default(T), 0);
        }
        public static (ICollidable, float) RayCast(this GameObject obj, int degAngle, float n = 1, float maxDist = 9999) => RayCast<ICollidable>(obj, degAngle, n, maxDist);

        public static List<T> FindAll<T>() where T : GameObject
        {
            List<T> list = new List<T>();

            for (var i = 0; i < ActiveObjects.Count; i++)
            {
                var o = ActiveObjects[i];
                
                if (!(o is T))
                    continue;

                list.Add((T)(object)o);
            }
            return list;
        }

        public static bool CollisionBounds(this ICollidable self, ICollidable other, float x, float y)
        {
            if (other == null || !other.Enabled) return false;

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
        public static List<T> CollisionBounds<T>(this ICollidable self, float x, float y) where T : ICollidable
        {
            List<T> list = new List<T>();
            
            for(var i = 0; i < ActiveObjects.Count; i++)
            {
                var o = ActiveObjects[i];

                if (!o.Enabled)
                    continue;
                if (!(o is T))
                    continue;
                if (o == self)
                    continue;

                if (o.Right >= x + self.BoundingBox.X && o.Left <= x + self.BoundingBox.X + self.BoundingBox.Width
                    &&
                    o.Bottom >= y + self.BoundingBox.Y && o.Top <= y + self.BoundingBox.Y + self.BoundingBox.Height)
                    list.Add((T)(object)o);
                
            }

            return list;
        }

        public static T CollisionBoundsFirstOrDefault<T>(this ICollidable self, float x, float y) where T : ICollidable
        {
            for (var i = 0; i < ActiveObjects.Count; i++)
            {
                var o = ActiveObjects[i];

                if (!o.Enabled)
                    continue;
                if (!(o is T))
                    continue;
                if (o == self)
                    continue;
                
                if (o.Right >= x + self.BoundingBox.X && o.Left <= x + self.BoundingBox.X + self.BoundingBox.Width
                    &&
                    o.Bottom >= y + self.BoundingBox.Y && o.Top <= y + self.BoundingBox.Y + self.BoundingBox.Height)
                    return ((T)(object)o);
            }

            return default(T);
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
            foreach (var o in Objects.ToList())
            {
                if (o is T)
                {
                    if (o.X >= x && o.Y >= y && o.X < x + width && o.Y < y + height) o.Enabled = enabled;
                }
            }

            UpdateActiveObjectList();
        }

        private static void UpdateActiveObjectList()
        {
            ActiveObjects = Objects.ToList().Where(o => o != null && o.Enabled).ToList();
        }

        /// <summary>
        /// Call this in your game Update method.
        /// </summary>
        public static void UpdateObjects(GameTime gameTime)
        {
            UpdateActiveObjectList();
            
            if (ElapsedTime > GameDelay)
            {
                ElapsedTime -= GameDelay;

                for(var i = 0; i < ActiveObjects.Count; i++)
                {
                    var o = ActiveObjects[i];
                    if (!o.Enabled)
                        continue;
                    o.BeginUpdate(gameTime);
                }

                for (var i = 0; i < ActiveObjects.Count; i++)
                {
                    var o = ActiveObjects[i];
                    if (!o.Enabled)
                        continue;
                    o.Update(gameTime);
                }

                for (var i = 0; i < ActiveObjects.Count; i++)
                {
                    var o = ActiveObjects[i];
                    if (!o.Enabled)
                        continue;
                    o.EndUpdate(gameTime);
                }
            }
            ElapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;            
        }

        /// <summary>
        /// Call this between the SpriteBatch.Begin and SpriteBatch.End in your game Draw method.
        /// </summary>
        public static void DrawObjects(SpriteBatch sb, GameTime gameTime, Rectangle visibleRect = new Rectangle())
        {
            for (var i = 0; i < ActiveObjects.Count; i++)
            {
                var o = ActiveObjects[i];
                if (!o.Visible)
                    continue;

                //if (visibleRect.Width > 0 && visibleRect.Height > 0)
                //{
                //    if (o.Right < visibleRect.X || o.Left > visibleRect.X + visibleRect.Width
                //        || o.Bottom < visibleRect.Y || o.Top > visibleRect.Y + visibleRect.Height)
                //        continue;
                //}

                o.Draw(sb, gameTime);
            }            
        }

        public static void Enable(Func<GameObject, bool> predicate)
        {
            for (var i = 0; i < Objects.Count; i++)
            {
                var o = Objects[i];
                if (o.Enabled)
                    continue;
                if (predicate(o))
                {
                    Enable(o, false);
                }
            }

            UpdateActiveObjectList();
        }
    }
}

