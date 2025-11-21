using Godot;

namespace AlJourney.Scripts.Utils
{
    /// <summary>
    /// Provides camera shake effects for visual feedback.
    /// Attach to a Camera2D node.
    /// </summary>
    public partial class CameraShake : Node
    {
        private Camera2D _camera;
        private Vector2 _originalOffset;
        private float _shakeIntensity;
        private float _shakeDuration;
        private float _shakeTimer;
        private bool _isShaking;

        public override void _Ready()
        {
            // Get camera (can be parent or specified)
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

        public override void _Process(double delta)
        {
            if (!_isShaking || _camera == null)
            {
                return;
            }

            _shakeTimer -= (float)delta;

            if (_shakeTimer <= 0)
            {
                // Stop shaking
                _isShaking = false;
                _camera.Offset = _originalOffset;
            }
            else
            {
                // Apply shake
                float currentIntensity = _shakeIntensity * (_shakeTimer / _shakeDuration);
                Vector2 randomOffset = new(
                    (GD.Randf() * currentIntensity * 2) - currentIntensity,
                    (GD.Randf() * currentIntensity * 2) - currentIntensity
                );
                _camera.Offset = _originalOffset + randomOffset;
            }
        }

        /// <summary>
        /// Triggers camera shake effect.
        /// </summary>
        /// <param name="intensity">Shake intensity (pixel displacement)</param>
        /// <param name="duration">Shake duration in seconds</param>
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
        /// Triggers light shake (for minor events).
        /// </summary>
        public void ShakeLight()
        {
            Shake(5.0f, 0.2f);
        }

        /// <summary>
        /// Triggers medium shake (for normal attacks).
        /// </summary>
        public void ShakeMedium()
        {
            Shake(10.0f, 0.3f);
        }

        /// <summary>
        /// Triggers strong shake (for critical hits/boss attacks).
        /// </summary>
        public void ShakeStrong()
        {
            Shake(20.0f, 0.5f);
        }

        /// <summary>
        /// Stops shake immediately.
        /// </summary>
        public void StopShake()
        {
            _isShaking = false;
            _ = _camera?.Offset = _originalOffset;
        }
    }
}
