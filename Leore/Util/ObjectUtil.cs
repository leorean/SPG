using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace Leore.Util
{
    public static class ObjectUtil
    {
        public static Direction ToDirection(this Vector2 vec)
        {
            if (Math.Abs(vec.X) > Math.Abs(vec.Y))
            {
                return (Direction)(1 * Math.Sign(vec.X));
            }
            else
            {
                return (Direction)(2 * Math.Sign(vec.Y));
            }
        }
    }

    //public static class ObjectUtil
    //{
    //    public static T DeepClone<T>(this T obj)
    //    {
    //        byte[] bytes = SPG.Save.BinarySerializer.Serialize(obj);
    //        T clone = SPG.Save.BinarySerializer.Deserialize<T>(bytes);
    //        return clone;
    //    }
    //}
}
