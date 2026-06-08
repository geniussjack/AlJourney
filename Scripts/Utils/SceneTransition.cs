using Godot;

namespace AlJourney.Scripts.Utils
{
    /// <summary>
    /// Основной класс SceneTransition.
    /// </summary>
    public partial class SceneTransition : CanvasLayer
    {
        private ColorRect _fadeRect;
        private bool _isTransitioning;

        /// <summary>
        /// Элемент _Ready.
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
        /// Элемент TransitionToScene.
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
        /// Элемент FadeOut.
        /// </summary>
        public void FadeOut(float duration = 0.3f)
        {
            Tween tween = CreateTween();
            _ = tween.TweenProperty(_fadeRect, "modulate:a", 1.0f, duration);
        }

        /// <summary>
        /// Элемент FadeIn.
        /// </summary>
        public void FadeIn(float duration = 0.3f)
        {
            Tween tween = CreateTween();
            _ = tween.TweenProperty(_fadeRect, "modulate:a", 0.0f, duration);
        }

        /// <summary>
        /// Элемент Flash.
        /// </summary>
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
