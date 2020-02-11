using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Util
{
    public static class ContentUtil
    {
        public static (Stream, string) GetResourceStream(string assetName, string suffix = ".tmx")
        {
            var assembly = Assembly.GetExecutingAssembly();
            var streamPath = assembly.GetName().Name + ".Content." + assetName + suffix;
            return (assembly.GetManifestResourceStream(streamPath), assetName);
        }
    }
}
