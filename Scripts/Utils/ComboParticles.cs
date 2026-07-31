using AlJourney.Scripts.Core;
using Godot;

namespace AlJourney.Scripts.Utils
{
    /// <summary>
    /// Helper class for creating visual effects and floating text
    /// when combo effects trigger, damage is taken, healing occurs, or shields are applied.
    /// </summary>
    public partial class ComboParticles : Node2D
    {
        /// <summary>
        /// Creates and starts a particle effect at the location of a triggered combo.
        /// The particle color depends on the element type, and the amount depends on the combo level.
        /// Particles are automatically removed once the animation finishes.
        /// </summary>
        /// <param name="parent">The node the particles will be attached to.</param>
        /// <param name="position">The position where the particles appear.</param>
        /// <param name="element">The ability's element, used to determine the effect color.</param>
        /// <param name="comboLevel">The effect's level, determining the number of particles.</param>
        public static void SpawnComboEffect(Node parent, Vector2 position, AbilityElement element, int comboLevel)
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
                Color = GetElementColor(element)
            };

            parent.AddChild(particles);

            SceneTreeTimer timer = parent.GetTree().CreateTimer(particles.Lifetime + 0.1f);
            timer.Timeout += particles.QueueFree;

            GD.Print($"[ComboParticles] Spawned {element} particles (combo {comboLevel})");
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

        private static Color GetElementColor(AbilityElement element)
        {
            return element switch
            {
                AbilityElement.Fire => new Color(1.0f, 0.3f, 0.0f),
                AbilityElement.Heal => new Color(0.0f, 1.0f, 0.3f),
                AbilityElement.Sword => new Color(1.0f, 0.6f, 0.0f),
                AbilityElement.Shield => new Color(0.2f, 0.5f, 1.0f),
                _ => Colors.White
            };
        }

        /// <summary>
        /// Creates animated floating text that rises upward and gradually fades out.
        /// The text is automatically removed once the animation finishes.
        /// </summary>
        /// <param name="parent">The node the text will be attached to.</param>
        /// <param name="position">The starting position where the text appears.</param>
        /// <param name="text">The string to display.</param>
        /// <param name="color">The color of the displayed text.</param>
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
        /// Creates red floating text to display damage taken.
        /// </summary>
        /// <param name="parent">The parent node.</param>
        /// <param name="position">The position where the text appears.</param>
        /// <param name="damage">The amount of damage.</param>
        public static void SpawnDamageNumber(Node parent, Vector2 position, int damage)
        {
            SpawnFloatingText(parent, position, $"-{damage}", new Color(1.0f, 0.3f, 0.3f));
        }

        /// <summary>
        /// Creates green floating text to display healing received.
        /// </summary>
        /// <param name="parent">The parent node.</param>
        /// <param name="position">The position where the text appears.</param>
        /// <param name="healing">The amount of health restored.</param>
        public static void SpawnHealNumber(Node parent, Vector2 position, int healing)
        {
            SpawnFloatingText(parent, position, $"+{healing}", new Color(0.3f, 1.0f, 0.3f));
        }

        /// <summary>
        /// Creates blue floating text to display shield gained.
        /// </summary>
        /// <param name="parent">The parent node.</param>
        /// <param name="position">The position where the text appears.</param>
        /// <param name="shield">The amount of shield points.</param>
        public static void SpawnShieldNumber(Node parent, Vector2 position, int shield)
        {
            SpawnFloatingText(parent, position, $"+{shield} Shield", new Color(0.3f, 0.5f, 1.0f));
        }
    }
}
