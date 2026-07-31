using Godot;

namespace AlJourney.Scripts.Utils
{
    /// <summary>
    /// Component that creates a camera shake effect.
    /// Must be added as a child node of a Camera2D.
    /// Used to reinforce visual feedback when taking damage, on powerful attacks, or during explosions.
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
        /// Initialization method. Checks for a parent camera
        /// and stores its original offset so it can be restored afterward.
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
        /// Runs every frame.
        /// If shaking is active, applies a random offset to the parent camera based on the current intensity.
        /// Once the timer runs out, restores the camera to its original position.
        /// </summary>
        /// <param name="delta">Time elapsed since the previous frame.</param>
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
        /// Starts the camera shake effect with the given parameters.
        /// </summary>
        /// <param name="intensity">Shake intensity, in pixels.</param>
        /// <param name="duration">Effect duration, in seconds.</param>
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
        /// Starts a light camera shake.
        /// Suited for weak hits or minor events.
        /// </summary>
        public void ShakeLight()
        {
            Shake(5.0f, 0.2f);
        }

        /// <summary>
        /// Starts a medium camera shake.
        /// Suited for regular attacks or standard effects.
        /// </summary>
        public void ShakeMedium()
        {
            Shake(10.0f, 0.3f);
        }

        /// <summary>
        /// Starts a strong camera shake.
        /// Suited for critical hits, explosions, or powerful spells.
        /// </summary>
        public void ShakeStrong()
        {
            Shake(20.0f, 0.5f);
        }

        /// <summary>
        /// Immediately stops the camera shake and returns it to its original position.
        /// </summary>
        public void StopShake()
        {
            _isShaking = false;
            _camera?.Offset = _originalOffset;
        }
    }
}
