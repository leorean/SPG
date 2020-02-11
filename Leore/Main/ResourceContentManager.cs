using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Main
{
    public class EmbeddedResourceContentManager : ContentManager
    {
        public EmbeddedResourceContentManager(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public EmbeddedResourceContentManager(IServiceProvider serviceProvider, string rootDirectory) : base(serviceProvider, rootDirectory)
        {
        }
        
        protected override Stream OpenStream(string assetName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            assetName = assembly.GetName().Name + ".Content." + assetName.Replace('\\', '.') + ".xnb";
            var stream = assembly.GetManifestResourceStream(assetName);
            
            return stream;
        }
    }
}
