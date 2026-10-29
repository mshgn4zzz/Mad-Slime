using MadSlime.Saves.Interfaces;
using MadSlime.Validation;
using UnityEngine;
using YG;

namespace MadSlime.Saves.Logic
{
    public class AudioStorage : IAudioStorage
    {
        private const int MaxVolumePercent = 100;

        private SavesCore _core;

        public AudioStorage(SavesCore core)
        {
            ArgumentGuard.CheckNotNull(core, nameof(core));

            _core = core;
        }

        public int GetMusicVolume()
        {
            return Mathf.Clamp(_core.LoadData().MusicVolume, 0, MaxVolumePercent);
        }

        public int GetSfxVolume()
        {
            return Mathf.Clamp(_core.LoadData().SfxVolume, 0, MaxVolumePercent);
        }

        public void SetVolumes(int musicVolume, int sfxVolume)
        {
            SavesYG data = _core.LoadData();

            data.MusicVolume = Mathf.Clamp(musicVolume, 0, MaxVolumePercent);
            data.SfxVolume = Mathf.Clamp(sfxVolume, 0, MaxVolumePercent);
            _core.SaveChanges();
        }
    }
}
