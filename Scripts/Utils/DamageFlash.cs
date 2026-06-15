using Godot;

namespace AlJourney.Scripts.Utils
{
    /// <summary>
    /// Компонент для кратковременного изменения цвета родительского CanvasItem.
    /// Используется для создания эффекта "вспышки" при получении урона, лечении или других событиях.
    /// </summary>
    public partial class DamageFlash : Node
    {
        private CanvasItem _target;
        private Color _originalModulate;

        /// <summary>
        /// Инициализирует компонент. Ищет родительский узел типа CanvasItem
        /// и сохраняет его исходный цвет для корректного возврата после завершения анимации вспышки.
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
        /// Запускает анимацию изменения цвета родительского узла на указанный, 
        /// а затем плавно возвращает его к исходному состоянию.
        /// </summary>
        /// <param name="flashColor">Цвет, в который окрасится объект во время вспышки.</param>
        /// <param name="duration">Общая продолжительность эффекта в секундах.</param>
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
        /// Запускает вспышку красного цвета. 
        /// Предназначено для визуализации получения урона.
        /// </summary>
        public void FlashDamage()
        {
            Flash(new Color(1.5f, 0.5f, 0.5f, 1.0f), 0.2f);
        }

        /// <summary>
        /// Запускает вспышку зеленого цвета.
        /// Предназначено для визуализации применения эффектов лечения.
        /// </summary>
        public void FlashHeal()
        {
            Flash(new Color(0.5f, 1.5f, 0.5f, 1.0f), 0.2f);
        }

        /// <summary>
        /// Запускает вспышку синего цвета.
        /// Предназначено для визуализации получения щита или магической защиты.
        /// </summary>
        public void FlashShield()
        {
            Flash(new Color(0.5f, 0.5f, 1.5f, 1.0f), 0.2f);
        }

        /// <summary>
        /// Запускает яркую белую вспышку.
        /// Предназначено для визуализации критических попаданий или мощных событий.
        /// </summary>
        public void FlashCritical()
        {
            Flash(new Color(2.0f, 2.0f, 2.0f, 1.0f), 0.15f);
        }

        /// <summary>
        /// Запускает вспышку пользовательского цвета с заданными параметрами.
        /// Позволяет гибко использовать эффект для нестандартных ситуаций.
        /// </summary>
        /// <param name="color">Специфический цвет вспышки.</param>
        /// <param name="duration">Длительность вспышки.</param>
        public void FlashCustom(Color color, float duration = 0.15f)
        {
            Flash(color, duration);
        }
    }
}
