namespace MadSlime.Saves.Interfaces
{
    public interface IAudioStorage
    {
        int GetMusicVolume();

        int GetSfxVolume();

        void SetVolumes(int musicVolume, int sfxVolume);
    }
}
