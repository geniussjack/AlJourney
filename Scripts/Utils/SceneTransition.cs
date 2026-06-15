using Godot;

namespace AlJourney.Scripts.Utils
{
    /// <summary>
    /// Компонент для создания плавных переходов между сценами.
    /// Использует полноэкранный прямоугольник для создания эффектов затемнения,
    /// осветления и вспышек. Помогает избежать резких смен кадров в игре.
    /// </summary>
    public partial class SceneTransition : CanvasLayer
    {
        private ColorRect _fadeRect;
        private bool _isTransitioning;

        /// <summary>
        /// Инициализирует компонент. Создает полноэкранный ColorRect, делает его полностью прозрачным 
        /// и настраивает так, чтобы он не перехватывал события мыши.
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
        /// Выполняет плавный переход к указанной сцене.
        /// Сначала затемняет экран, затем загружает новую сцену и осветляет экран обратно.
        /// </summary>
        /// <param name="scenePath">Путь к файлу целевой сцены.</param>
        /// <param name="duration">Общее время перехода в секундах.</param>
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
        /// Выполняет плавное затемнение экрана до полностью непрозрачного состояния.
        /// </summary>
        /// <param name="duration">Длительность затемнения в секундах.</param>
        public void FadeOut(float duration = 0.3f)
        {
            Tween tween = CreateTween();
            _ = tween.TweenProperty(_fadeRect, "modulate:a", 1.0f, duration);
        }

        /// <summary>
        /// Выполняет плавное осветление экрана до полностью прозрачного состояния.
        /// </summary>
        /// <param name="duration">Длительность осветления в секундах.</param>
        public void FadeIn(float duration = 0.3f)
        {
            Tween tween = CreateTween();
            _ = tween.TweenProperty(_fadeRect, "modulate:a", 0.0f, duration);
        }

        /// <summary>
        /// Создает кратковременную вспышку экрана указанным цветом.
        /// Полезно для визуализации получения сильного урона, критических атак или других значимых событий.
        /// </summary>
        /// <param name="color">Цвет вспышки.</param>
        /// <param name="duration">Длительность эффекта вспышки в секундах.</param>
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
