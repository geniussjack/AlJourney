using Godot;

namespace AlJourney.Scripts.Utils
{
    /// <summary>
    /// Компонент для создания эффекта тряски камеры.
    /// Должен быть добавлен как дочерний узел к объекту Camera2D.
    /// Используется для усиления визуальной обратной связи при получении урона, мощных атаках или взрывах.
    /// </summary>
    public partial class CameraShake : Node
    {
        private Camera2D _camera;
        private Vector2 _originalOffset;
        private float _shakeIntensity;
        private float _shakeDuration;
        private float _shakeTimer;
        private bool _isShaking;

        /// <summary>
        /// Метод инициализации. Проверяет наличие родительской камеры (Camera2D) 
        /// и сохраняет ее изначальное смещение (Offset) для последующего возврата в исходное состояние.
        /// </summary>
        public override void _Ready()
        {
            _camera = GetParentOrNull<Camera2D>();

            if (_camera != null)
            {
                _originalOffset = _camera.Offset;
                GD.Print("[CameraShake] Initialized for Camera2D");
            }
            else
            {
                GD.PrintErr("[CameraShake] No Camera2D found! Add this as child of Camera2D.");
            }
        }

        /// <summary>
        /// Выполняется каждый кадр. 
        /// Если тряска активна, применяет случайное смещение к родительской камере с учетом текущей интенсивности.
        /// По истечении времени возвращает камеру в исходную позицию.
        /// </summary>
        /// <param name="delta">Время, прошедшее с предыдущего кадра.</param>
        public override void _Process(double delta)
        {
            if (!_isShaking || _camera == null)
            {
                return;
            }

            _shakeTimer -= (float)delta;

            if (_shakeTimer <= 0)
            {
                _isShaking = false;
                _camera.Offset = _originalOffset;
            }
            else
            {
                float currentIntensity = _shakeIntensity * (_shakeTimer / _shakeDuration);
                Vector2 randomOffset = new(
                    (GD.Randf() * currentIntensity * 2) - currentIntensity,
                    (GD.Randf() * currentIntensity * 2) - currentIntensity
                );
                _camera.Offset = _originalOffset + randomOffset;
            }
        }

        /// <summary>
        /// Запускает эффект тряски камеры с заданными параметрами.
        /// </summary>
        /// <param name="intensity">Интенсивность (амплитуда) тряски в пикселях.</param>
        /// <param name="duration">Продолжительность эффекта в секундах.</param>
        public void Shake(float intensity = 10.0f, float duration = 0.3f)
        {
            if (_camera == null)
            {
                return;
            }

            _shakeIntensity = intensity;
            _shakeDuration = duration;
            _shakeTimer = duration;
            _isShaking = true;

            GD.Print($"[CameraShake] Shake started - Intensity: {intensity}, Duration: {duration}");
        }

        /// <summary>
        /// Запускает легкую тряску камеры.
        /// Подходит для слабых ударов или незначительных событий.
        /// </summary>
        public void ShakeLight()
        {
            Shake(5.0f, 0.2f);
        }

        /// <summary>
        /// Запускает среднюю тряску камеры.
        /// Подходит для обычных атак или стандартных эффектов.
        /// </summary>
        public void ShakeMedium()
        {
            Shake(10.0f, 0.3f);
        }

        /// <summary>
        /// Запускает сильную тряску камеры.
        /// Подходит для критических ударов, взрывов или мощных заклинаний.
        /// </summary>
        public void ShakeStrong()
        {
            Shake(20.0f, 0.5f);
        }

        /// <summary>
        /// Немедленно останавливает тряску камеры и возвращает ее в исходное положение.
        /// </summary>
        public void StopShake()
        {
            _isShaking = false;
            _ = _camera?.Offset = _originalOffset;
        }
    }
}
