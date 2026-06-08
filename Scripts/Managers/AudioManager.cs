using Godot;
using AlJourney.Scripts.Interfaces;
using System.Collections.Generic;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Менеджер AudioManager. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public partial class AudioManager : Node, IAudioManager
    {
        public static AudioManager Instance { get; private set; }

        private AudioStreamPlayer _musicPlayer;
        private List<AudioStreamPlayer> _sfxPlayers;
        private const int SFX_POOL_SIZE = 8;
        private readonly HashSet<string> _missingResourceWarnings = [];

        public float MasterVolume
        {
            get; set
            {
                field = Mathf.Clamp(value, 0.0f, 1.0f);
                UpdateVolumes();
            }
        } = 1.0f;

        public float MusicVolume
        {
            get; set
            {
                field = Mathf.Clamp(value, 0.0f, 1.0f);
                UpdateVolumes();
            }
        } = 0.7f;

        /// <summary>
        /// Элемент SfxVolume.
        /// </summary>
        public float SfxVolume { get; set => field = Mathf.Clamp(value, 0.0f, 1.0f); } = 0.8f;

        /// <summary>
        /// Элемент _Ready.
        /// </summary>
        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }

            Instance = this;

            _musicPlayer = new AudioStreamPlayer
            {
                Name = "MusicPlayer",
                Bus = "Music"
            };
            AddChild(_musicPlayer);

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
        /// Воспроизводит Music.
        /// </summary>
        public void PlayMusic(string musicPath, bool loop = true)
        {
            _ = TryPlayMusic(musicPath, loop);
        }

        /// <summary>
        /// Пытается выполнить PlayMusic.
        /// </summary>
        public bool TryPlayMusic(string musicPath, bool loop = true)
        {
            AudioStream stream = GD.Load<AudioStream>(musicPath);
            if (stream == null)
            {
                WarnMissingResourceOnce("music", musicPath);
                return false;
            }

            _musicPlayer.Stream = stream;

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
            return true;
        }

        /// <summary>
        /// Останавливает Music.
        /// </summary>
        public void StopMusic()
        {
            _musicPlayer.Stop();
        }

        /// <summary>
        /// Воспроизводит Sfx.
        /// </summary>
        public void PlaySfx(string sfxPath, float pitchVariation = 0.0f)
        {
            _ = TryPlaySfx(sfxPath, pitchVariation);
        }

        /// <summary>
        /// Пытается выполнить PlaySfx.
        /// </summary>
        public bool TryPlaySfx(string sfxPath, float pitchVariation = 0.0f)
        {
            AudioStream stream = GD.Load<AudioStream>(sfxPath);
            if (stream == null)
            {
                WarnMissingResourceOnce("sfx", sfxPath);
                return false;
            }

            AudioStreamPlayer availablePlayer = null;
            foreach (AudioStreamPlayer player in _sfxPlayers)
            {
                if (!player.Playing)
                {
                    availablePlayer = player;
                    break;
                }
            }

            availablePlayer ??= _sfxPlayers[0];

            availablePlayer.Stream = stream;
            availablePlayer.VolumeDb = Mathf.LinearToDb(SfxVolume * MasterVolume);

            availablePlayer.PitchScale = pitchVariation > 0.0f ? 1.0f + ((GD.Randf() * pitchVariation * 2.0f) - pitchVariation) : 1.0f;

            availablePlayer.Play();
            return true;
        }

        /// <summary>
        /// Элемент FadeOutMusic.
        /// </summary>
        public void FadeOutMusic(float duration = 1.0f)
        {
            if (_musicPlayer?.Playing != true)
            {
                return;
            }

            Tween tween = CreateTween();
            _ = tween.TweenProperty(_musicPlayer, "volume_db", -80.0f, duration);
            _ = tween.TweenCallback(Callable.From(() =>
            {
                _musicPlayer.Stop();
                UpdateVolumes(); 
            }));

            GD.Print($"[AudioManager] Fading out music over {duration}s");
        }

        /// <summary>
        /// Элемент FadeInMusic.
        /// </summary>
        public void FadeInMusic(float duration = 1.0f)
        {
            if (_musicPlayer?.Playing != true)
            {
                return;
            }

            _musicPlayer.VolumeDb = -80.0f;

            float targetVolume = Mathf.LinearToDb(MusicVolume * MasterVolume);
            Tween tween = CreateTween();
            _ = tween.TweenProperty(_musicPlayer, "volume_db", targetVolume, duration);

            GD.Print($"[AudioManager] Fading in music over {duration}s");
        }

        /// <summary>
        /// Элемент CrossfadeMusic.
        /// </summary>
        public void CrossfadeMusic(string newMusicPath, float duration = 1.0f, bool loop = true)
        {
            if (_musicPlayer.Playing)
            {
                FadeOutMusic(duration);
            }

            GetTree().CreateTimer(duration).Timeout += () =>
            {
                PlayMusic(newMusicPath, loop);
                FadeInMusic(duration);
            };

            GD.Print($"[AudioManager] Crossfading to: {newMusicPath}");
        }

        private void UpdateVolumes()
        {
            _ = _musicPlayer?.VolumeDb = Mathf.LinearToDb(MusicVolume * MasterVolume);
        }

        private void WarnMissingResourceOnce(string resourceType, string resourcePath)
        {
            string warningKey = $"{resourceType}:{resourcePath}";
            if (_missingResourceWarnings.Contains(warningKey))
            {
                return;
            }

            _missingResourceWarnings.Add(warningKey);
            GD.PrintErr($"[AudioManager] Missing {resourceType} resource: {resourcePath}");
        }
    }
}
