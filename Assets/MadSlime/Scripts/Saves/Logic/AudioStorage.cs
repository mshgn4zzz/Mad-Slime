using System;
using UnityEngine;
using YG;

namespace Saves
{
    public class AudioStorage : IAudioStorage
    {
        private const int MaxVolumePercent = 100;

        private SavesCore _core;

        public AudioStorage(SavesCore core)
        {
            if (core == null)
            {
                throw new ArgumentNullException(nameof(core),
                    "AudioStorage requires a SavesCore instance.");
            }

            _core = core;
        }

        public int GetMusicVolume()
        {
            return ToVolumePercent(_core.LoadData().musicVolume);
        }

        public int GetSfxVolume()
        {
            return ToVolumePercent(_core.LoadData().sfxVolume);
        }

        public void SetVolumes(int musicVolume, int sfxVolume)
        {
            SavesYG data = _core.LoadData();

            data.musicVolume = Mathf.Clamp(musicVolume, 0, MaxVolumePercent);
            data.sfxVolume = Mathf.Clamp(sfxVolume, 0, MaxVolumePercent);
            _core.SaveChanges();
        }

        private static int ToVolumePercent(float storedVolume)
        {
            float percent;

            if (storedVolume < 1f)
            {
                percent = storedVolume * 100f;
            }
            else
            {
                percent = storedVolume;
            }

            return Mathf.Clamp(Mathf.RoundToInt(percent), 0, MaxVolumePercent);
        }
    }
}
