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

namespace SPG.Objects
{
    public static class ObjectManager
    {
        public static List<GameObject> Objects { get; private set; } = new List<GameObject>();

        private static int idCounter;

        private static double elapsedTime = 0;
        
        public static double GameSpeed { get; set; }

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
            foreach(var o in Objects)
            {
                o.Enabled = false;
            }
        }
        
        public static List<GameObject> Find(GameObject self, float x, float y, Type type)
        {
            Stopwatch sw = Stopwatch.StartNew();

            List<GameObject> candidates = Objects.Where(o => o.Enabled == true).ToList();

            if (candidates.Count == 0) return candidates;
            
            candidates = Objects.Where(o => o.Right >= x + self.BoundingBox.X && o.Left <= x + self.BoundingBox.X + self.BoundingBox.Width && o != self).ToList();
            candidates = candidates.Where(o => o.Bottom >= y + self.BoundingBox.Y && o.Top <= y + self.BoundingBox.Y + self.BoundingBox.Height && o != self).ToList();

            //Debug.WriteLine($"Found {candidates.Count} candidates after {sw.ElapsedMilliseconds}ms.");

            return candidates;
        }

        /// <summary>
        /// Call this in your game Update method.
        /// </summary>
        public static void UpdateObjects(GameTime gameTime)
        {
            if (elapsedTime > GameSpeed)
            {
                elapsedTime -= GameSpeed;
                Objects.Where(o => o.Enabled).ToList().ForEach(o => o.Update(gameTime));
            }
            elapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;            
        }

        /// <summary>
        /// Call this between the SpriteBatch.Begin and SpriteBatch.End in your game Draw method.
        /// </summary>
        public static void DrawObjects(GameTime gameTime)
        {
            SortByID();
            Objects.Where(o => o.Enabled && o.Visible).ToList().ForEach(o => o.Draw(gameTime));
        }
    }
}

