using AlJourney.Scripts.Data;
using Godot;
using System;
using System.Collections.Generic;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Reusable full-screen cutscene player: shows a <see cref="CutsceneData"/>'s slides one at a time
    /// and calls back when finished. Advances a slide on tap/click, and skips the whole cutscene when
    /// Enter is held down continuously for <see cref="SkipHoldDurationSeconds"/> — polled directly via
    /// <see cref="Input.IsPhysicalKeyPressed"/> rather than through a project InputMap action, so this
    /// works without touching project.godot. Built entirely in code, similar to
    /// <see cref="BattleHUD"/>/<see cref="Scenes.CampaignMapScene"/> — the scene file only carries the
    /// root Control and this script.
    /// </summary>
    public partial class CutscenePlayer : Control
    {
        private const float SkipHoldDurationSeconds = 2.0f;

        private TextureRect _slideImage;
        private Label _slideLabel;
        private Label _skipHintLabel;
        private ProgressBar _skipProgressBar;

        private IReadOnlyList<CutsceneSlide> _slides;
        private Action _onFinished;
        private int _currentSlideIndex;
        private float _skipHoldTime;
        private bool _isFinished;

        /// <summary>
        /// Builds the (data-less) visual layout. Actual slide content is supplied afterwards via
        /// <see cref="Initialize"/>.
        /// </summary>
        public override void _Ready()
        {
            SetAnchorsPreset(LayoutPreset.FullRect);
            MouseFilter = MouseFilterEnum.Stop;
            ZIndex = 100;

            ColorRect background = new() { Color = new Color(0, 0, 0, 0.92f) };
            background.SetAnchorsPreset(LayoutPreset.FullRect);
            background.MouseFilter = MouseFilterEnum.Ignore;
            AddChild(background);

            CenterContainer center = new();
            center.SetAnchorsPreset(LayoutPreset.FullRect);
            center.MouseFilter = MouseFilterEnum.Ignore;
            AddChild(center);

            VBoxContainer layout = new()
            {
                Alignment = BoxContainer.AlignmentMode.Center,
                MouseFilter = MouseFilterEnum.Ignore
            };
            layout.AddThemeConstantOverride("separation", 24);
            center.AddChild(layout);

            _slideImage = new TextureRect
            {
                CustomMinimumSize = new Vector2(0, 280),
                ExpandMode = TextureRect.ExpandModeEnum.FitWidth,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                Visible = false
            };
            layout.AddChild(_slideImage);

            _slideLabel = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                AutowrapMode = TextServer.AutowrapMode.Word,
                CustomMinimumSize = new Vector2(900, 0)
            };
            _slideLabel.AddThemeFontSizeOverride("font_size", 28);
            layout.AddChild(_slideLabel);

            layout.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });

            _skipHintLabel = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = Tr("UI_CUTSCENE_SKIP_HINT"),
                Modulate = new Color(1, 1, 1, 0.6f)
            };
            _skipHintLabel.AddThemeFontSizeOverride("font_size", 16);
            layout.AddChild(_skipHintLabel);

            _skipProgressBar = new ProgressBar
            {
                CustomMinimumSize = new Vector2(220, 6),
                ShowPercentage = false,
                MaxValue = 1.0
            };
            layout.AddChild(_skipProgressBar);
        }

        /// <summary>
        /// Supplies the cutscene to play and the callback to invoke once it finishes (either by reaching
        /// the last slide or by being skipped).
        /// </summary>
        /// <param name="data">The cutscene to play.</param>
        /// <param name="onFinished">Invoked exactly once, after this node has removed itself.</param>
        public void Initialize(CutsceneData data, Action onFinished)
        {
            _slides = data.Slides;
            _onFinished = onFinished;
            _currentSlideIndex = 0;

            ShowCurrentSlide();
        }

        /// <summary>
        /// Advances one slide on a tap/click anywhere on the cutscene.
        /// </summary>
        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton { Pressed: false, ButtonIndex: MouseButton.Left })
            {
                AdvanceSlide();
            }
        }

        /// <summary>
        /// Polls the physical Enter key every frame: a continuous hold of
        /// <see cref="SkipHoldDurationSeconds"/> skips the whole cutscene, while a shorter tap advances a
        /// single slide (detected on release, since a genuine hold and a tap are only distinguishable in
        /// hindsight).
        /// </summary>
        public override void _Process(double delta)
        {
            if (_isFinished)
            {
                return;
            }

            bool isHoldingSkipKey = Input.IsPhysicalKeyPressed(Key.Enter) || Input.IsPhysicalKeyPressed(Key.KpEnter);

            if (isHoldingSkipKey)
            {
                _skipHoldTime += (float)delta;
                _skipProgressBar.Value = Mathf.Clamp(_skipHoldTime / SkipHoldDurationSeconds, 0.0, 1.0);

                if (_skipHoldTime >= SkipHoldDurationSeconds)
                {
                    Finish();
                }

                return;
            }

            bool wasTap = _skipHoldTime is > 0f and < SkipHoldDurationSeconds;
            _skipHoldTime = 0f;
            _skipProgressBar.Value = 0.0;

            if (wasTap)
            {
                AdvanceSlide();
            }
        }

        private void ShowCurrentSlide()
        {
            CutsceneSlide slide = _slides[_currentSlideIndex];
            _slideLabel.Text = Tr(slide.TextKey);

            if (!string.IsNullOrEmpty(slide.ImagePath) && ResourceLoader.Exists(slide.ImagePath))
            {
                _slideImage.Texture = GD.Load<Texture2D>(slide.ImagePath);
                _slideImage.Visible = true;
            }
            else
            {
                _slideImage.Texture = null;
                _slideImage.Visible = false;
            }
        }

        private void AdvanceSlide()
        {
            _currentSlideIndex++;

            if (_currentSlideIndex >= _slides.Count)
            {
                Finish();
                return;
            }

            ShowCurrentSlide();
        }

        private void Finish()
        {
            if (_isFinished)
            {
                return;
            }

            _isFinished = true;

            Action onFinished = _onFinished;
            QueueFree();
            onFinished?.Invoke();
        }

        /// <summary>
        /// Instantiates a <see cref="CutscenePlayer"/> as a child of <paramref name="parent"/> and starts
        /// playing the given cutscene. This is the intended entry point — callers should not instantiate
        /// the scene directly.
        /// </summary>
        /// <param name="parent">The node the player is attached to (typically the current UI screen).</param>
        /// <param name="data">The cutscene to play.</param>
        /// <param name="onFinished">Invoked once the cutscene ends or is skipped.</param>
        public static CutscenePlayer Play(Node parent, CutsceneData data, Action onFinished)
        {
            PackedScene scene = GD.Load<PackedScene>("res://Scenes/UI/CutscenePlayer.tscn");
            CutscenePlayer player = scene.Instantiate<CutscenePlayer>();

            parent.AddChild(player);
            player.Initialize(data, onFinished);

            return player;
        }
    }
}
