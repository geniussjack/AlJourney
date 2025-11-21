using Godot;

namespace AlJourney.Scripts.Utils
{
    /// <summary>
    /// Provides smooth scene transition effects with fade in/out.
    /// Add as autoload singleton for easy access.
    /// </summary>
    public partial class SceneTransition : CanvasLayer
    {
        private ColorRect _fadeRect;
        private bool _isTransitioning;

        public override void _Ready()
        {
            // Create fullscreen fade rectangle
            _fadeRect = new ColorRect
            {
                Color = Colors.Black,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            _fadeRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(_fadeRect);

            // Start transparent
            _fadeRect.Modulate = new Color(1, 1, 1, 0);

            GD.Print("[SceneTransition] Initialized");
        }

        /// <summary>
        /// Transitions to a new scene with fade effect.
        /// </summary>
        public void TransitionToScene(string scenePath, float duration = 0.5f)
        {
            if (_isTransitioning)
            {
                GD.PrintErr("[SceneTransition] Already transitioning!");
                return;
            }

            _isTransitioning = true;
            GD.Print($"[SceneTransition] Transitioning to: {scenePath}");

            // Fade out
            Tween tween = CreateTween();
            _ = tween.TweenProperty(_fadeRect, "modulate:a", 1.0f, duration / 2);
            _ = tween.TweenCallback(Callable.From(() =>
            {
                // Change scene
                _ = GetTree().ChangeSceneToFile(scenePath);

                // Fade in
                Tween fadeTween = CreateTween();
                _ = fadeTween.TweenProperty(_fadeRect, "modulate:a", 0.0f, duration / 2);
                _ = fadeTween.TweenCallback(Callable.From(() =>
                {
                    _isTransitioning = false;
                }));
            }));
        }

        /// <summary>
        /// Fades to black (without scene change).
        /// </summary>
        public void FadeOut(float duration = 0.3f)
        {
            Tween tween = CreateTween();
            _ = tween.TweenProperty(_fadeRect, "modulate:a", 1.0f, duration);
        }

        /// <summary>
        /// Fades from black.
        /// </summary>
        public void FadeIn(float duration = 0.3f)
        {
            Tween tween = CreateTween();
            _ = tween.TweenProperty(_fadeRect, "modulate:a", 0.0f, duration);
        }

        /// <summary>
        /// Quick flash effect.
        /// </summary>
        public void Flash(Color color, float duration = 0.2f)
        {
            Color originalColor = _fadeRect.Color;
            _fadeRect.Color = color;

            Tween tween = CreateTween();
            _ = tween.TweenProperty(_fadeRect, "modulate:a", 0.7f, duration / 2);
            _ = tween.TweenProperty(_fadeRect, "modulate:a", 0.0f, duration / 2);
            _ = tween.TweenCallback(Callable.From(() =>
            {
                _fadeRect.Color = originalColor;
            }));
        }
    }
}
