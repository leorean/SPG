using Microsoft.Xna.Framework.Audio;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Main
{
    public static class SoundManager
    {
        public static float SoundVolume { get; set; } = 1.0f;

        public static void Play(SoundEffect sound, float pitch = 0, float pan = 0)
        {
            sound.Play(SoundVolume, pitch, pan);            
        }
    }
}
