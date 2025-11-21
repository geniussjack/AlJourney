using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Manages game audio including music and sound effects.
    /// Singleton autoload node.
    /// </summary>
    public partial class AudioManager : Node
    {
        /// <summary>
        /// Singleton instance accessor.
        /// </summary>
        public static AudioManager Instance { get; private set; }

        private AudioStreamPlayer _musicPlayer;
        private List<AudioStreamPlayer> _sfxPlayers;
        private const int SFX_POOL_SIZE = 8;

        /// <summary>
        /// Master volume level (0.0 to 1.0).
        /// </summary>
        public float MasterVolume
        {
            get; set
            {
                field = Mathf.Clamp(value, 0.0f, 1.0f);
                UpdateVolumes();
            }
        } = 1.0f;

        /// <summary>
        /// Music volume level (0.0 to 1.0).
        /// </summary>
        public float MusicVolume
        {
            get; set
            {
                field = Mathf.Clamp(value, 0.0f, 1.0f);
                UpdateVolumes();
            }
        } = 0.7f;

        /// <summary>
        /// Sound effects volume level (0.0 to 1.0).
        /// </summary>
        public float SfxVolume { get; set => field = Mathf.Clamp(value, 0.0f, 1.0f); } = 0.8f;

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }

            Instance = this;

            // Setup music player
            _musicPlayer = new AudioStreamPlayer
            {
                Name = "MusicPlayer",
                Bus = "Music"
            };
            AddChild(_musicPlayer);

            // Setup SFX player pool
            _sfxPlayers = [];
            for (int i = 0; i < SFX_POOL_SIZE; i++)
            {
                AudioStreamPlayer sfxPlayer = new()
                {
                    Name = $"SFXPlayer_{i}",
                    Bus = "SFX"
                };
                AddChild(sfxPlayer);
                _sfxPlayers.Add(sfxPlayer);
            }

            UpdateVolumes();
            GD.Print("[AudioManager] Initialized");
        }

        /// <summary>
        /// Plays background music from path. Loops by default.
        /// </summary>
        public void PlayMusic(string musicPath, bool loop = true)
        {
            AudioStream stream = GD.Load<AudioStream>(musicPath);
            if (stream == null)
            {
                GD.PrintErr($"[AudioManager] Failed to load music: {musicPath}");
                return;
            }

            _musicPlayer.Stream = stream;

            // Enable looping if supported
            if (stream is AudioStreamOggVorbis oggStream)
            {
                oggStream.Loop = loop;
            }
            else if (stream is AudioStreamWav wavStream)
            {
                wavStream.LoopMode = loop ? AudioStreamWav.LoopModeEnum.Forward : AudioStreamWav.LoopModeEnum.Disabled;
            }

            _musicPlayer.Play();
            GD.Print($"[AudioManager] Playing music: {musicPath}");
        }

        /// <summary>
        /// Stops current music.
        /// </summary>
        public void StopMusic()
        {
            _musicPlayer.Stop();
        }

        /// <summary>
        /// Plays a sound effect from path.
        /// </summary>
        public void PlaySfx(string sfxPath, float pitchVariation = 0.0f)
        {
            AudioStream stream = GD.Load<AudioStream>(sfxPath);
            if (stream == null)
            {
                GD.PrintErr($"[AudioManager] Failed to load SFX: {sfxPath}");
                return;
            }

            // Find available player
            AudioStreamPlayer availablePlayer = null;
            foreach (AudioStreamPlayer player in _sfxPlayers)
            {
                if (!player.Playing)
                {
                    availablePlayer = player;
                    break;
                }
            }

            // If all busy, use first one
            availablePlayer ??= _sfxPlayers[0];

            availablePlayer.Stream = stream;
            availablePlayer.VolumeDb = Mathf.LinearToDb(SfxVolume * MasterVolume);

            // Apply pitch variation
            availablePlayer.PitchScale = pitchVariation > 0.0f ? 1.0f + ((GD.Randf() * pitchVariation * 2.0f) - pitchVariation) : 1.0f;

            availablePlayer.Play();
        }

        /// <summary>
        /// Fades out current music over duration.
        /// </summary>
        public void FadeOutMusic(float duration = 1.0f)
        {
            if (_musicPlayer == null || !_musicPlayer.Playing)
            {
                return;
            }

            Tween tween = CreateTween();
            _ = tween.TweenProperty(_musicPlayer, "volume_db", -80.0f, duration);
            _ = tween.TweenCallback(Callable.From(() =>
            {
                _musicPlayer.Stop();
                UpdateVolumes(); // Restore original volume
            }));

            GD.Print($"[AudioManager] Fading out music over {duration}s");
        }

        /// <summary>
        /// Fades in music over duration. Music should already be playing.
        /// </summary>
        public void FadeInMusic(float duration = 1.0f)
        {
            if (_musicPlayer == null || !_musicPlayer.Playing)
            {
                return;
            }

            // Start at silent
            _musicPlayer.VolumeDb = -80.0f;

            // Fade to target volume
            float targetVolume = Mathf.LinearToDb(MusicVolume * MasterVolume);
            Tween tween = CreateTween();
            _ = tween.TweenProperty(_musicPlayer, "volume_db", targetVolume, duration);

            GD.Print($"[AudioManager] Fading in music over {duration}s");
        }

        /// <summary>
        /// Crossfades from current music to new music.
        /// </summary>
        public void CrossfadeMusic(string newMusicPath, float duration = 1.0f, bool loop = true)
        {
            // Fade out current
            if (_musicPlayer.Playing)
            {
                FadeOutMusic(duration);
            }

            // Wait and play new music with fade in
            GetTree().CreateTimer(duration).Timeout += () =>
            {
                PlayMusic(newMusicPath, loop);
                FadeInMusic(duration);
            };

            GD.Print($"[AudioManager] Crossfading to: {newMusicPath}");
        }

        /// <summary>
        /// Updates volume for all audio players.
        /// </summary>
        private void UpdateVolumes()
        {
            _ = _musicPlayer?.VolumeDb = Mathf.LinearToDb(MusicVolume * MasterVolume);
        }
    }
}
