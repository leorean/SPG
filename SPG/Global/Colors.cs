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
    public static class Colors
    {
        public static Color FromHex(string hexCode)
        {
            hexCode = hexCode.Replace("#", "");

            try
            {
                if (hexCode.Length == 6)
                {

                    var r = int.Parse(hexCode.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    var g = int.Parse(hexCode.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    var b = int.Parse(hexCode.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

                    return new Color(r, g, b);
                }

                if (hexCode.Length == 8)
                {
                    var a = int.Parse(hexCode.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    var r = int.Parse(hexCode.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    var g = int.Parse(hexCode.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                    var b = int.Parse(hexCode.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                    return new Color(r, g, b, a);
                }
            }
            catch (Exception) { }
            throw new ArgumentException($"Color code '{hexCode}' is invalid!");
        }
    }
}
   