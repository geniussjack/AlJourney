using AlJourney.Scripts.Interfaces;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Audio system manager. Responsible for playing music and sound effects, and managing their volume.
    /// </summary>
    public partial class AudioManager : Node, IAudioManager
    {
        /// <summary>
        /// Global instance of the audio manager.
        /// </summary>
        public static AudioManager Instance { get; private set; }

        private AudioStreamPlayer _musicPlayer;
        private List<AudioStreamPlayer> _sfxPlayers;
        private const int SFX_POOL_SIZE = 8;
        private readonly HashSet<string> _missingResourceWarnings = [];

        /// <summary>
        /// Overall volume for all sounds in the game. Ranges from 0.0 to 1.0.
        /// </summary>
        public float MasterVolume
        {
            get;
            set
            {
                field = Mathf.Clamp(value, 0.0f, 1.0f);
                ApplyBusVolume("Master", field);
            }
        } = 1.0f;

        /// <summary>
        /// Background music volume. Ranges from 0.0 to 1.0.
        /// </summary>
        public float MusicVolume
        {
            get;
            set
            {
                field = Mathf.Clamp(value, 0.0f, 1.0f);
                ApplyBusVolume("Music", field);
            }
        } = 0.7f;

        /// <summary>
        /// Sound effects volume. Ranges from 0.0 to 1.0.
        /// </summary>
        public float SfxVolume
        {
            get;
            set
            {
                field = Mathf.Clamp(value, 0.0f, 1.0f);
                ApplyBusVolume("SFX", field);
            }
        } = 0.8f;

        /// <summary>
        /// Applies a linear volume value to the given Godot audio bus, converting it to decibels.
        /// Does nothing if no bus with that name is found (e.g. with a custom audio configuration).
        /// </summary>
        /// <param name="busName">The audio bus name ("Master", "Music" or "SFX").</param>
        /// <param name="linearVolume">The volume on a linear scale from 0.0 to 1.0.</param>
        private static void ApplyBusVolume(string busName, float linearVolume)
        {
            int busIndex = AudioServer.GetBusIndex(busName);
            if (busIndex >= 0)
            {
                AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb(linearVolume));
            }
        }

        /// <summary>
        /// Initializes the music and sound effect audio players when added to the scene tree.
        /// Sets up the sound player pools.
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

            // Initialize bus volumes with the values set by the field initializers.
            ApplyBusVolume("Master", MasterVolume);
            ApplyBusVolume("Music", MusicVolume);
            ApplyBusVolume("SFX", SfxVolume);

            GetTree().NodeAdded += OnNodeAdded;
            HookExistingNodes(GetTree().Root);

            GD.Print("[AudioManager] Initialized");
        }

        /// <summary>
        /// Plays background music from the given resource path.
        /// </summary>
        /// <param name="musicPath">The path to the music audio resource.</param>
        /// <param name="loop">Whether the music should loop.</param>
        public void PlayMusic(string musicPath, bool loop = true)
        {
            _ = TryPlayMusic(musicPath, loop);
        }

        /// <summary>
        /// Attempts to load and play background music. Prints a warning if the resource isn't found.
        /// </summary>
        /// <param name="musicPath">The path to the music audio resource.</param>
        /// <param name="loop">Whether the music should loop.</param>
        /// <returns><c>true</c> if the music was successfully loaded and started playing; otherwise <c>false</c>.</returns>
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
            else if (stream is AudioStreamMP3 mp3Stream)
            {
                mp3Stream.Loop = loop;
            }

            _musicPlayer.Play();
            GD.Print($"[AudioManager] Playing music: {musicPath}");
            return true;
        }

        /// <summary>
        /// Stops the currently playing background music.
        /// </summary>
        public void StopMusic()
        {
            _musicPlayer.Stop();
        }

        /// <summary>
        /// Plays a sound effect from the given path.
        /// </summary>
        /// <param name="sfxPath">The path to the sound effect resource.</param>
        /// <param name="pitchVariation">Random pitch variation for sound variety.</param>
        public void PlaySfx(string sfxPath, float pitchVariation = 0.0f)
        {
            _ = TryPlaySfx(sfxPath, pitchVariation);
        }

        /// <summary>
        /// Attempts to find a free player and play the sound effect.
        /// </summary>
        /// <param name="sfxPath">The path to the sound effect resource.</param>
        /// <param name="pitchVariation">The pitch variation.</param>
        /// <returns><c>true</c> if the effect was found and started playing; otherwise <c>false</c>.</returns>
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
            availablePlayer.VolumeDb = 0.0f; // Reset local volume, Bus handles the global volume

            availablePlayer.PitchScale = pitchVariation > 0.0f ? 1.0f + ((GD.Randf() * pitchVariation * 2.0f) - pitchVariation) : 1.0f;

            availablePlayer.Play();
            return true;
        }

        public void PlayChoiceRightSound()
        {
            PlaySfx("res://Resources/Audio/SFX/choice_right_sound.mp3", 0.05f);
        }

        public void PlayChoiceErrorSound()
        {
            PlaySfx("res://Resources/Audio/SFX/choice_error_sound.mp3", 0.05f);
        }

        private void HookExistingNodes(Node parent)
        {
            OnNodeAdded(parent);
            foreach (Node child in parent.GetChildren())
            {
                HookExistingNodes(child);
            }
        }

        private void OnNodeAdded(Node node)
        {
            if (node is BaseButton button)
            {
                // To avoid multiple connections if somehow called twice
                if (!button.HasSignal("pressed") || button.IsConnected("pressed", Callable.From(PlayChoiceRightSound)))
                {
                    return;
                }

                button.Pressed += PlayChoiceRightSound;
                button.GuiInput += (@event) =>
                {
                    if (button.Disabled && @event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
                    {
                        PlayChoiceErrorSound();
                    }
                };
            }
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
        /// Smoothly fades the current music's volume down to silence, then stops it.
        /// </summary>
        /// <param name="duration">The fade duration, in seconds.</param>
        public void FadeOutMusic(float duration = 1.0f)
        {
            if (_musicPlayer?.Playing != true)
            {
                return;
            }

            Tween tween = CreateTween();
            _ = tween.TweenProperty(_musicPlayer, "volume_db", -80.0f, duration);
            _ = tween.TweenCallback(Callable.From(_musicPlayer.Stop));

            GD.Print($"[AudioManager] Fading out music over {duration}s");
        }

        /// <summary>
        /// Smoothly raises the music's volume from silence up to its target level.
        /// </summary>
        /// <param name="duration">The fade-in duration, in seconds.</param>
        public void FadeInMusic(float duration = 1.0f)
        {
            if (_musicPlayer?.Playing != true)
            {
                return;
            }

            _musicPlayer.VolumeDb = -80.0f;

            Tween tween = CreateTween();
            _ = tween.TweenProperty(_musicPlayer, "volume_db", 0.0f, duration);

            GD.Print($"[AudioManager] Fading in music over {duration}s");
        }

        /// <summary>
        /// Performs a smooth transition between the current music and a new track.
        /// </summary>
        /// <param name="newMusicPath">The path to the new music track.</param>
        /// <param name="duration">The transition duration, in seconds.</param>
        /// <param name="loop">Whether the new track should loop.</param>
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
