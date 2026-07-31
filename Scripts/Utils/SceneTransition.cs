using Godot;

namespace AlJourney.Scripts.Utils
{
    /// <summary>
    /// Component that creates smooth transitions between scenes.
    /// Uses a full-screen rectangle to create fade-out, fade-in
    /// and flash effects. Helps avoid jarring frame changes in the game.
    /// </summary>
    public partial class SceneTransition : CanvasLayer
    {
        private ColorRect _fadeRect;
        private bool _isTransitioning;

        /// <summary>
        /// Initializes the component. Creates a full-screen ColorRect, makes it fully transparent,
        /// and sets it up so it doesn't intercept mouse events.
        /// </summary>
        public override void _Ready()
        {
            _fadeRect = new ColorRect
            {
                Color = Colors.Black,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            _fadeRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(_fadeRect);

            _fadeRect.Modulate = new Color(1, 1, 1, 0);

            GD.Print("[SceneTransition] Initialized");
        }

        /// <summary>
        /// Performs a smooth transition to the given scene.
        /// First fades the screen to black, then loads the new scene and fades back in.
        /// </summary>
        /// <param name="scenePath">The path to the target scene file.</param>
        /// <param name="duration">The total transition time, in seconds.</param>
        public void TransitionToScene(string scenePath, float duration = 0.5f)
        {
            if (_isTransitioning)
            {
                GD.PrintErr("[SceneTransition] Already transitioning!");
                return;
            }

            _isTransitioning = true;
            GD.Print($"[SceneTransition] Transitioning to: {scenePath}");

            Tween tween = CreateTween();
            _ = tween.TweenProperty(_fadeRect, "modulate:a", 1.0f, duration / 2);
            _ = tween.TweenCallback(Callable.From(() =>
            {
                _ = GetTree().ChangeSceneToFile(scenePath);

                Tween fadeTween = CreateTween();
                _ = fadeTween.TweenProperty(_fadeRect, "modulate:a", 0.0f, duration / 2);
                _ = fadeTween.TweenCallback(Callable.From(() => _isTransitioning = false));
            }));
        }

        /// <summary>
        /// Smoothly fades the screen out to fully opaque.
        /// </summary>
        /// <param name="duration">The fade-out duration, in seconds.</param>
        public void FadeOut(float duration = 0.3f)
        {
            Tween tween = CreateTween();
            _ = tween.TweenProperty(_fadeRect, "modulate:a", 1.0f, duration);
        }

        /// <summary>
        /// Smoothly fades the screen in to fully transparent.
        /// </summary>
        /// <param name="duration">The fade-in duration, in seconds.</param>
        public void FadeIn(float duration = 0.3f)
        {
            Tween tween = CreateTween();
            _ = tween.TweenProperty(_fadeRect, "modulate:a", 0.0f, duration);
        }

        /// <summary>
        /// Creates a brief screen flash of the given color.
        /// Useful for visualizing heavy damage, critical hits, or other significant events.
        /// </summary>
        /// <param name="color">The flash color.</param>
        /// <param name="duration">The flash effect's duration, in seconds.</param>
        public void Flash(Color color, float duration = 0.2f)
        {
            Color originalColor = _fadeRect.Color;
            _fadeRect.Color = color;

            Tween tween = CreateTween();
            _ = tween.TweenProperty(_fadeRect, "modulate:a", 0.7f, duration / 2);
            _ = tween.TweenProperty(_fadeRect, "modulate:a", 0.0f, duration / 2);
            _ = tween.TweenCallback(Callable.From(() => _fadeRect.Color = originalColor));
        }
    }
}
