using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace SPG
{
    /*public static class Util
    {
        public static int Div(int first, int second)
        {
            return (int)Math.Floor(first / (float)second);
        }
        
        /// <summary>
        /// Loads a file in XML format. File must have build type "Content" and "Copy if newer" so it gets found.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static XmlDocument LoadXML(string fileName)
        {
            fileName = "Content/" + fileName;
            var content =  "";
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
    }*/
    
    
}