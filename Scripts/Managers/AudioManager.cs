using AlJourney.Scripts.Interfaces;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Менеджер аудиосистемы. Отвечает за воспроизведение музыки и звуковых эффектов, а также управление их громкостью.
    /// </summary>
    public partial class AudioManager : Node, IAudioManager
    {
        /// <summary>
        /// Глобальный экземпляр менеджера аудио.
        /// </summary>
        public static AudioManager Instance { get; private set; }

        private AudioStreamPlayer _musicPlayer;
        private List<AudioStreamPlayer> _sfxPlayers;
        private const int SFX_POOL_SIZE = 8;
        private readonly HashSet<string> _missingResourceWarnings = [];

        /// <summary>
        /// Общая громкость для всех звуков в игре. Значение от 0.0 до 1.0.
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
        /// Громкость фоновой музыки. Значение от 0.0 до 1.0.
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
        /// Громкость звуковых эффектов. Значение от 0.0 до 1.0.
        /// </summary>
        public float SfxVolume { get; set => field = Mathf.Clamp(value, 0.0f, 1.0f); } = 0.8f;

        /// <summary>
        /// Инициализирует аудиоплееры для музыки и эффектов при добавлении в дерево сцены.
        /// Настраивает пулы звуковых плееров.
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
        /// Воспроизводит фоновую музыку по указанному пути к ресурсу.
        /// </summary>
        /// <param name="musicPath">Путь к файлу аудиоресурса музыки.</param>
        /// <param name="loop">Указывает, должна ли музыка воспроизводиться в цикле.</param>
        public void PlayMusic(string musicPath, bool loop = true)
        {
            _ = TryPlayMusic(musicPath, loop);
        }

        /// <summary>
        /// Пытается загрузить и воспроизвести фоновую музыку. Выводит предупреждение, если ресурс не найден.
        /// </summary>
        /// <param name="musicPath">Путь к файлу аудиоресурса музыки.</param>
        /// <param name="loop">Должна ли музыка зацикливаться.</param>
        /// <returns><c>true</c>, если музыка успешно загружена и воспроизводится; иначе <c>false</c>.</returns>
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
        /// Останавливает воспроизведение текущей фоновой музыки.
        /// </summary>
        public void StopMusic()
        {
            _musicPlayer.Stop();
        }

        /// <summary>
        /// Воспроизводит звуковой эффект по указанному пути.
        /// </summary>
        /// <param name="sfxPath">Путь к ресурсу звукового эффекта.</param>
        /// <param name="pitchVariation">Случайная вариация высоты тона для разнообразия звучания.</param>
        public void PlaySfx(string sfxPath, float pitchVariation = 0.0f)
        {
            _ = TryPlaySfx(sfxPath, pitchVariation);
        }

        /// <summary>
        /// Пытается найти свободный плеер и воспроизвести звуковой эффект.
        /// </summary>
        /// <param name="sfxPath">Путь к ресурсу звукового эффекта.</param>
        /// <param name="pitchVariation">Вариация высоты тона.</param>
        /// <returns><c>true</c>, если эффект найден и начал воспроизводиться; иначе <c>false</c>.</returns>
        public bool TryPlaySfx(string sfxPath, float pitchVariation = 0.0f)
        {
            if (!ResourceLoader.Exists(sfxPath))
            {
                WarnMissingResourceOnce("sfx", sfxPath);
                return false;
            }

            AudioStream stream = GD.Load<AudioStream>(sfxPath);
            if (stream == null)
            {
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

        public void PlaySwapSound()
        {
            PlaySfx("res://Resources/Audio/SFX/swap.wav", 0.1f);
        }

        public void PlayMatchSound()
        {
            PlaySfx("res://Resources/Audio/SFX/match.wav", 0.15f);
        }

        public void PlayAttackSound()
        {
            PlaySfx("res://Resources/Audio/SFX/attack.wav", 0.1f);
        }

        public void PlayHitSound()
        {
            PlaySfx("res://Resources/Audio/SFX/hit.wav", 0.1f);
        }

        public void PlayNewGameSound()
        {
            PlaySfx("res://Resources/Audio/SFX/new_game.wav");
        }

        /// <summary>
        /// Плавно уменьшает громкость текущей музыки до полного затухания, а затем останавливает её.
        /// </summary>
        /// <param name="duration">Продолжительность затухания в секундах.</param>
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
        /// Плавно увеличивает громкость музыки от минимального значения до целевого.
        /// </summary>
        /// <param name="duration">Продолжительность нарастания звука в секундах.</param>
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
        /// Выполняет плавный переход между текущей музыкой и новым треком.
        /// </summary>
        /// <param name="newMusicPath">Путь к новому музыкальному треку.</param>
        /// <param name="duration">Длительность перехода в секундах.</param>
        /// <param name="loop">Указывает, должен ли новый трек воспроизводиться в цикле.</param>
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

            _ = _missingResourceWarnings.Add(warningKey);
            GD.PrintErr($"[AudioManager] Missing {resourceType} resource: {resourcePath}");
        }
    }
}
