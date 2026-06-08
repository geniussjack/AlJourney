using Godot;

namespace AlJourney.Scripts.Utils
{
    /// <summary>
    /// Основной класс CameraShake.
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
        /// Элемент _Ready.
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
        /// Элемент _Process.
        /// </summary>
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
        /// Элемент Shake.
        /// </summary>
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
        /// Элемент ShakeLight.
        /// </summary>
        public void ShakeLight()
        {
            Shake(5.0f, 0.2f);
        }

        /// <summary>
        /// Элемент ShakeMedium.
        /// </summary>
        public void ShakeMedium()
        {
            Shake(10.0f, 0.3f);
        }

        /// <summary>
        /// Элемент ShakeStrong.
        /// </summary>
        public void ShakeStrong()
        {
            Shake(20.0f, 0.5f);
        }

        /// <summary>
        /// Останавливает Shake.
        /// </summary>
        public void StopShake()
        {
            _isShaking = false;
            _ = _camera?.Offset = _originalOffset;
        }
    }
}
