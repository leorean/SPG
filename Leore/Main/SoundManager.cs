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

        private static Dictionary<string, SoundEffectInstance> soundInst = new Dictionary<string, SoundEffectInstance>();

        public static bool IsPlaying(SoundEffect sound)
        {
            if (soundInst.ContainsKey(sound.Name) && soundInst[sound.Name].State == SoundState.Playing)
                return true;

            return false;
        }

        public static void Play(SoundEffect sound, float pitch = 0, float pan = 0)
        {
            if (!soundInst.ContainsKey(sound.Name))
            {
                soundInst.Add(sound.Name, sound.CreateInstance());
            }

            if (IsPlaying(sound))
            {
                soundInst[sound.Name].Stop();
            }

            soundInst[sound.Name].Pitch = pitch;
            soundInst[sound.Name].Pan = pan;
            soundInst[sound.Name].Play();

            //sound.Play(SoundVolume, pitch, pan);
        }
    }
}
