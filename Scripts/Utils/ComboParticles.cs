using AlJourney.Scripts.Core;
using Godot;

namespace AlJourney.Scripts.Utils
{
    /// <summary>
    /// Вспомогательный класс для создания визуальных эффектов и всплывающего текста
    /// при срабатывании комбо-эффектов, получении урона, лечении и наложении щитов.
    /// </summary>
    public partial class ComboParticles : Node2D
    {
        /// <summary>
        /// Создает и запускает эффект частиц на месте сработавшего комбо.
        /// Цвет частиц зависит от типа элемента, а их количество — от уровня комбо.
        /// Частицы автоматически удаляются после завершения анимации.
        /// </summary>
        /// <param name="parent">Узел, к которому будут прикреплены частицы.</param>
        /// <param name="position">Позиция появления частиц.</param>
        /// <param name="elementType">Тип элемента для определения цвета эффекта.</param>
        /// <param name="comboLevel">Уровень комбо, определяющий количество частиц.</param>
        public static void SpawnComboEffect(Node parent, Vector2 position, ElementType elementType, int comboLevel)
        {
            CpuParticles2D particles = new()
            {
                Position = position,
                Emitting = true,
                OneShot = true,
                Amount = GetParticleAmount(comboLevel),
                Lifetime = 0.5f,
                Explosiveness = 0.8f,
                Spread = 360.0f,
                InitialVelocityMin = 50.0f,
                InitialVelocityMax = 150.0f,
                ScaleAmountMin = 0.5f,
                ScaleAmountMax = 1.5f,
                Color = GetElementColor(elementType)
            };

            parent.AddChild(particles);

            SceneTreeTimer timer = parent.GetTree().CreateTimer(particles.Lifetime + 0.1f);
            timer.Timeout += particles.QueueFree;

            GD.Print($"[ComboParticles] Spawned {elementType} particles (combo {comboLevel})");
        }

        private static int GetParticleAmount(int comboLevel)
        {
            return comboLevel switch
            {
                1 => 10,
                2 => 20,
                3 => 30,
                _ => 10
            };
        }

        private static Color GetElementColor(ElementType elementType)
        {
            return elementType switch
            {
                ElementType.Fire => new Color(1.0f, 0.3f, 0.0f),
                ElementType.Heal => new Color(0.0f, 1.0f, 0.3f),
                ElementType.Sword => new Color(1.0f, 0.6f, 0.0f),
                ElementType.Shield => new Color(0.2f, 0.5f, 1.0f),
                _ => Colors.White
            };
        }

        /// <summary>
        /// Создает анимированный всплывающий текст, который поднимается вверх и постепенно исчезает.
        /// Текст автоматически удаляется после завершения анимации.
        /// </summary>
        /// <param name="parent">Узел, к которому будет прикреплен текст.</param>
        /// <param name="position">Начальная позиция появления текста.</param>
        /// <param name="text">Строка для отображения.</param>
        /// <param name="color">Цвет отображаемого текста.</param>
        public static void SpawnFloatingText(Node parent, Vector2 position, string text, Color color)
        {
            Label label = new()
            {
                Position = position,
                Text = text,
                Modulate = color
            };
            label.AddThemeFontSizeOverride("font_size", 24);

            parent.AddChild(label);

            Tween tween = label.CreateTween();
            _ = tween.SetParallel(true);
            _ = tween.TweenProperty(label, "position:y", position.Y - 50, 1.0f);
            _ = tween.TweenProperty(label, "modulate:a", 0.0f, 1.0f);
            _ = tween.Chain().TweenCallback(Callable.From(label.QueueFree));
        }

        /// <summary>
        /// Создает всплывающий текст красного цвета для отображения полученного урона.
        /// </summary>
        /// <param name="parent">Узел-родитель.</param>
        /// <param name="position">Позиция появления текста.</param>
        /// <param name="damage">Количество урона.</param>
        public static void SpawnDamageNumber(Node parent, Vector2 position, int damage)
        {
            SpawnFloatingText(parent, position, $"-{damage}", new Color(1.0f, 0.3f, 0.3f));
        }

        /// <summary>
        /// Создает всплывающий текст зеленого цвета для отображения полученного лечения.
        /// </summary>
        /// <param name="parent">Узел-родитель.</param>
        /// <param name="position">Позиция появления текста.</param>
        /// <param name="healing">Количество восстановленных очков здоровья.</param>
        public static void SpawnHealNumber(Node parent, Vector2 position, int healing)
        {
            SpawnFloatingText(parent, position, $"+{healing}", new Color(0.3f, 1.0f, 0.3f));
        }

        /// <summary>
        /// Создает всплывающий текст синего цвета для отображения полученного щита.
        /// </summary>
        /// <param name="parent">Узел-родитель.</param>
        /// <param name="position">Позиция появления текста.</param>
        /// <param name="shield">Количество очков щита.</param>
        public static void SpawnShieldNumber(Node parent, Vector2 position, int shield)
        {
            SpawnFloatingText(parent, position, $"+{shield} Shield", new Color(0.3f, 0.5f, 1.0f));
        }
    }
}
