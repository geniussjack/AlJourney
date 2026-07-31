namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Interface for managing the game's audio system.
    /// Allows adjusting volume, playing and stopping music and sound effects, and applying fade effects.
    /// </summary>
    public interface IAudioManager
    {
        /// <summary>
        /// The game's overall volume. Affects all sounds and music.
        /// </summary>
        float MasterVolume { get; set; }

        /// <summary>
        /// The background music volume level.
        /// </summary>
        float MusicVolume { get; set; }

        /// <summary>
        /// The sound effects volume level.
        /// </summary>
        float SfxVolume { get; set; }

        /// <summary>
        /// Starts playing music from the given path.
        /// </summary>
        void PlayMusic(string musicPath, bool loop = true);

        /// <summary>
        /// Attempts to start playing music. Returns true on success.
        /// </summary>
        bool TryPlayMusic(string musicPath, bool loop = true);

        /// <summary>
        /// Stops the current background music.
        /// </summary>
        void StopMusic();

        /// <summary>
        /// Plays a sound effect from the given path, with optional random pitch variation.
        /// </summary>
        void PlaySfx(string sfxPath, float pitchVariation = 0.0f);

        /// <summary>
        /// Attempts to play a sound effect. Returns true on success.
        /// </summary>
        bool TryPlaySfx(string sfxPath, float pitchVariation = 0.0f);

        /// <summary>
        /// Smoothly fades the current music's volume down to zero over the given duration.
        /// </summary>
        void FadeOutMusic(float duration = 1.0f);

        /// <summary>
        /// Smoothly fades the music's volume up to its target value over the given duration.
        /// </summary>
        void FadeInMusic(float duration = 1.0f);

        /// <summary>
        /// Smoothly crossfades from the current music to a new track over the given duration.
        /// </summary>
        void CrossfadeMusic(string newMusicPath, float duration = 1.0f, bool loop = true);
    }
}
