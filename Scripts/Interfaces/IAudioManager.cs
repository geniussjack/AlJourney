namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для управления аудиосистемой игры.
    /// Позволяет настраивать громкость, воспроизводить и останавливать музыку и звуковые эффекты, а также применять эффекты затухания.
    /// </summary>
    public interface IAudioManager
    {
        /// <summary>
        /// Общая громкость игры. Влияет на все звуки и музыку.
        /// </summary>
        float MasterVolume { get; set; }

        /// <summary>
        /// Уровень громкости фоновой музыки.
        /// </summary>
        float MusicVolume { get; set; }

        /// <summary>
        /// Уровень громкости звуковых эффектов.
        /// </summary>
        float SfxVolume { get; set; }

        /// <summary>
        /// Запускает воспроизведение музыки по указанному пути.
        /// </summary>
        void PlayMusic(string musicPath, bool loop = true);

        /// <summary>
        /// Пытается запустить воспроизведение музыки. Возвращает true в случае успеха.
        /// </summary>
        bool TryPlayMusic(string musicPath, bool loop = true);

        /// <summary>
        /// Останавливает текущую фоновую музыку.
        /// </summary>
        void StopMusic();

        /// <summary>
        /// Воспроизводит звуковой эффект по указанному пути с возможностью случайного изменения высоты тона.
        /// </summary>
        void PlaySfx(string sfxPath, float pitchVariation = 0.0f);

        /// <summary>
        /// Пытается воспроизвести звуковой эффект. Возвращает true в случае успеха.
        /// </summary>
        bool TryPlaySfx(string sfxPath, float pitchVariation = 0.0f);

        /// <summary>
        /// Плавно уменьшает громкость текущей музыки до нуля в течение заданного времени.
        /// </summary>
        void FadeOutMusic(float duration = 1.0f);

        /// <summary>
        /// Плавно увеличивает громкость музыки до целевого значения в течение заданного времени.
        /// </summary>
        void FadeInMusic(float duration = 1.0f);

        /// <summary>
        /// Плавно переключает текущую музыку на новую композицию за указанное время.
        /// </summary>
        void CrossfadeMusic(string newMusicPath, float duration = 1.0f, bool loop = true);
    }
}
