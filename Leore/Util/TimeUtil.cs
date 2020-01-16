using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Util
{
    public static class TimeUtil
    {
        public static string TimeStringFromMilliseconds(long milliseconds)
        {
            var t = TimeSpan.FromMilliseconds(milliseconds);
            return string.Format("{0:00}:{1:00}:{2:00}", t.Hours, t.Minutes, t.Seconds);            
        }
    }
}
