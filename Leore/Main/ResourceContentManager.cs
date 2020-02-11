using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
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
            assetName = typeof(EmbeddedResourceContentManager).Assembly.GetName().Name + "." + assetName.Replace('\\', '.') + ".xnb";            
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(assetName);            
        }
    }
}
