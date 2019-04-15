using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Xml;

namespace SPG.Util
{
    public static class Xml
    {
        /// <summary>
        /// Loads a file in XML format. File must have build type "Content" and "Copy if newer" so it gets found.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static XmlDocument Load(string fileName)
        {
            fileName = "Content/" + fileName;
            var content = "";
            if (File.Exists(fileName))
            {
                using (var stream = TitleContainer.OpenStream(fileName))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        content = reader.ReadToEnd();
                        var doc = new XmlDocument();
                        doc.LoadXml(content);
                        return doc;
                    }
                }
            }
            return null;
        }
    }    
}