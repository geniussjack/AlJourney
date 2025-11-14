using Godot;
using System.Collections.Generic;

namespace AltarionsJourney.Managers
{
    /// <summary>
    /// Manages game audio including music and sound effects.
    /// Singleton autoload node.
    /// </summary>
    public partial class AudioManager : Node
    {
        private static AudioManager _instance;

        /// <summary>
        /// Singleton instance accessor.
        /// </summary>
        public static AudioManager Instance => _instance;

        private AudioStreamPlayer _musicPlayer;
        private List<AudioStreamPlayer> _sfxPlayers;
        private const int SFX_POOL_SIZE = 8;

        private float _masterVolume = 1.0f;
        private float _musicVolume = 0.7f;
        private float _sfxVolume = 0.8f;

        /// <summary>
        /// Master volume level (0.0 to 1.0).
        /// </summary>
        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp(value, 0.0f, 1.0f);
                UpdateVolumes();
            }
        }

        /// <summary>
        /// Music volume level (0.0 to 1.0).
        /// </summary>
        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp(value, 0.0f, 1.0f);
                UpdateVolumes();
            }
        }

        /// <summary>
        /// Sound effects volume level (0.0 to 1.0).
        /// </summary>
        public float SfxVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp(value, 0.0f, 1.0f);
            }
        }

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }

            _instance = this;

            // Setup music player
            _musicPlayer = new AudioStreamPlayer
            {
                Name = "MusicPlayer",
                Bus = "Music"
            };
            AddChild(_musicPlayer);

            // Setup SFX player pool
            _sfxPlayers = new List<AudioStreamPlayer>();
            for (int i = 0; i < SFX_POOL_SIZE; i++)
            {
                var sfxPlayer = new AudioStreamPlayer
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
            var stream = GD.Load<AudioStream>(musicPath);
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
            var stream = GD.Load<AudioStream>(sfxPath);
            if (stream == null)
            {
                GD.PrintErr($"[AudioManager] Failed to load SFX: {sfxPath}");
                return;
            }

            // Find available player
            AudioStreamPlayer availablePlayer = null;
            foreach (var player in _sfxPlayers)
            {
                if (!player.Playing)
                {
                    availablePlayer = player;
                    break;
                }
            }

            // If all busy, use first one
            if (availablePlayer == null)
            {
                availablePlayer = _sfxPlayers[0];
            }

            availablePlayer.Stream = stream;
            availablePlayer.VolumeDb = Mathf.LinearToDb(_sfxVolume * _masterVolume);

            // Apply pitch variation
            if (pitchVariation > 0.0f)
            {
                availablePlayer.PitchScale = 1.0f + (GD.Randf() * pitchVariation * 2.0f - pitchVariation);
            }
            else
            {
                availablePlayer.PitchScale = 1.0f;
            }

            availablePlayer.Play();
        }

        /// <summary>
        /// Updates volume for all audio players.
        /// </summary>
        private void UpdateVolumes()
        {
            if (_musicPlayer != null)
            {
                _musicPlayer.VolumeDb = Mathf.LinearToDb(_musicVolume * _masterVolume);
            }
        }
    }
}