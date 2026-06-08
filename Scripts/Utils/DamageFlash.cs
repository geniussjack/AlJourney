using Godot;

namespace AlJourney.Scripts.Utils
{
    /// <summary>
    /// Основной класс DamageFlash.
    /// </summary>
    public partial class DamageFlash : Node
    {
        private CanvasItem _target;
        private Color _originalModulate;

        /// <summary>
        /// Элемент _Ready.
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
        /// Элемент Flash.
        /// </summary>
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
        /// Элемент FlashDamage.
        /// </summary>
        public void FlashDamage()
        {
            Flash(new Color(1.5f, 0.5f, 0.5f, 1.0f), 0.2f);
        }

        /// <summary>
        /// Элемент FlashHeal.
        /// </summary>
        public void FlashHeal()
        {
            Flash(new Color(0.5f, 1.5f, 0.5f, 1.0f), 0.2f);
        }

        /// <summary>
        /// Элемент FlashShield.
        /// </summary>
        public void FlashShield()
        {
            Flash(new Color(0.5f, 0.5f, 1.5f, 1.0f), 0.2f);
        }

        /// <summary>
        /// Элемент FlashCritical.
        /// </summary>
        public void FlashCritical()
        {
            Flash(new Color(2.0f, 2.0f, 2.0f, 1.0f), 0.15f);
        }

        /// <summary>
        /// Элемент FlashCustom.
        /// </summary>
        public void FlashCustom(Color color, float duration = 0.15f)
        {
            Flash(color, duration);
        }
    }
}
