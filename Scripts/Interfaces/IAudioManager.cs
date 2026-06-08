namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для управления аудио.
    /// </summary>
    /// <summary>
    /// Менеджер IAudioManager. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public interface IAudioManager
    {
        float MasterVolume { get; set; }
        float MusicVolume { get; set; }
        float SfxVolume { get; set; }

        void PlayMusic(string musicPath, bool loop = true);
        bool TryPlayMusic(string musicPath, bool loop = true);
        void StopMusic();
        void PlaySfx(string sfxPath, float pitchVariation = 0.0f);
        bool TryPlaySfx(string sfxPath, float pitchVariation = 0.0f);
        void FadeOutMusic(float duration = 1.0f);
        void FadeInMusic(float duration = 1.0f);
        void CrossfadeMusic(string newMusicPath, float duration = 1.0f, bool loop = true);
    }
}
