using System;
using System.Collections.Generic;
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
    }
}

