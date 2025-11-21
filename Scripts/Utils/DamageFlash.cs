using Godot;

namespace AlJourney.Scripts.Utils
{
    /// <summary>
    /// Provides damage flash effects for visual feedback.
    /// Can be applied to any CanvasItem (Control, Sprite2D, etc.).
    /// </summary>
    public partial class DamageFlash : Node
    {
        private CanvasItem _target;
        private Color _originalModulate;

        public override void _Ready()
        {
            // Get target (parent by default)
            _target = GetParentOrNull<CanvasItem>();

            if (_target != null)
            {
                _originalModulate = _target.Modulate;
                GD.Print($"[DamageFlash] Initialized for {_target.Name}");
            }
            else
            {
                GD.PrintErr("[DamageFlash] No CanvasItem parent found!");
            }
        }

        /// <summary>
        /// Triggers damage flash effect.
        /// </summary>
        /// <param name="flashColor">Color to flash</param>
        /// <param name="duration">Flash duration in seconds</param>
        public void Flash(Color flashColor, float duration = 0.15f)
        {
            if (_target == null)
            {
                return;
            }

            Tween tween = CreateTween();
            _ = tween.TweenProperty(_target, "modulate", flashColor, duration / 2);
            _ = tween.TweenProperty(_target, "modulate", _originalModulate, duration / 2);
        }

        /// <summary>
        /// Triggers red damage flash (for taking damage).
        /// </summary>
        public void FlashDamage()
        {
            Flash(new Color(1.5f, 0.5f, 0.5f, 1.0f), 0.2f);
        }

        /// <summary>
        /// Triggers green heal flash (for healing).
        /// </summary>
        public void FlashHeal()
        {
            Flash(new Color(0.5f, 1.5f, 0.5f, 1.0f), 0.2f);
        }

        /// <summary>
        /// Triggers blue shield flash (for gaining shield).
        /// </summary>
        public void FlashShield()
        {
            Flash(new Color(0.5f, 0.5f, 1.5f, 1.0f), 0.2f);
        }

        /// <summary>
        /// Triggers white critical flash (for critical hits).
        /// </summary>
        public void FlashCritical()
        {
            Flash(new Color(2.0f, 2.0f, 2.0f, 1.0f), 0.15f);
        }

        /// <summary>
        /// Triggers custom flash with color.
        /// </summary>
        public void FlashCustom(Color color, float duration = 0.15f)
        {
            Flash(color, duration);
        }
    }
}
