namespace Saves
{
    public interface IAudioStorage
    {
        int GetMusicVolume();

        int GetSfxVolume();

        void SetVolumes(int musicVolume, int sfxVolume);
    }
}
