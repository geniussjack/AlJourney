using Godot;

namespace AlJourney.Scripts.Utils
{
    /// <summary>
    /// Component that briefly changes the color of the parent CanvasItem.
    /// Used to create a "flash" effect when taking damage, healing, or on other events.
    /// </summary>
    public partial class DamageFlash : Node
    {
        private CanvasItem _target;
        private Color _originalModulate;

        /// <summary>
        /// Initializes the component. Looks for a parent node of type CanvasItem
        /// and stores its original color so it can be correctly restored after the flash animation ends.
        /// </summary>
        public override void _Ready()
        {
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
        /// Starts an animation changing the parent node's color to the given one,
        /// then smoothly returns it to its original state.
        /// </summary>
        /// <param name="flashColor">The color the object will flash.</param>
        /// <param name="duration">The total duration of the effect, in seconds.</param>
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
        /// Starts a red flash.
        /// Intended to visualize taking damage.
        /// </summary>
        public void FlashDamage()
        {
            Flash(new Color(1.5f, 0.5f, 0.5f, 1.0f), 0.2f);
        }

        /// <summary>
        /// Starts a green flash.
        /// Intended to visualize a healing effect.
        /// </summary>
        public void FlashHeal()
        {
            Flash(new Color(0.5f, 1.5f, 0.5f, 1.0f), 0.2f);
        }

        /// <summary>
        /// Starts a blue flash.
        /// Intended to visualize gaining a shield or magical protection.
        /// </summary>
        public void FlashShield()
        {
            Flash(new Color(0.5f, 0.5f, 1.5f, 1.0f), 0.2f);
        }

        /// <summary>
        /// Starts a bright white flash.
        /// Intended to visualize critical hits or other powerful events.
        /// </summary>
        public void FlashCritical()
        {
            Flash(new Color(2.0f, 2.0f, 2.0f, 1.0f), 0.15f);
        }

        /// <summary>
        /// Starts a flash with a custom color and the given parameters.
        /// Allows flexible use of the effect for non-standard situations.
        /// </summary>
        /// <param name="color">The specific flash color.</param>
        /// <param name="duration">The flash duration.</param>
        public void FlashCustom(Color color, float duration = 0.15f)
        {
            Flash(color, duration);
        }
    }
}
