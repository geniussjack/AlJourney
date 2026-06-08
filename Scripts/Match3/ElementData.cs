using AlJourney.Scripts.Core;

namespace AlJourney.Scripts.Match3
{
    /// <summary>
    /// Класс данных ElementData. Сохраняет информацию и параметры.
    /// </summary>
    public class ElementData(ElementType type, int x, int y)
    {
        /// <summary>
        /// Элемент Type.
        /// </summary>
        public ElementType Type { get; set; } = type;

        /// <summary>
        /// Элемент X.
        /// </summary>
        public int X { get; set; } = x;

        /// <summary>
        /// Элемент Y.
        /// </summary>
        public int Y { get; set; } = y;

        /// <summary>
        /// Проверяет, является ли Matched.
        /// </summary>
        public bool IsMatched { get; set; } = false;

        /// <summary>
        /// Проверяет, является ли Falling.
        /// </summary>
        public bool IsFalling { get; set; } = false;

        /// <summary>
        /// Элемент OwningHero.
        /// </summary>
        public CharacterClass OwningHero { get; set; } = GetOwnerForElementType(type);

        private static CharacterClass GetOwnerForElementType(ElementType elementType)
        {
            return elementType switch
            {
                ElementType.Fire => CharacterClass.Mage,
                ElementType.Heal => CharacterClass.Mage,
                ElementType.Sword => CharacterClass.Warrior,
                ElementType.Shield => CharacterClass.Warrior,
                _ => CharacterClass.Mage 
            };
        }

        /// <summary>
        /// Элемент CreateRandom.
        /// </summary>
        public static ElementData CreateRandom(int x, int y)
        {
            ElementType randomType = (ElementType)Godot.GD.RandRange(1, 4);
            return new ElementData(randomType, x, y);
        }

        /// <summary>
        /// Элемент CanMatchWith.
        /// </summary>
        public bool CanMatchWith(ElementData other)
        {
            return other != null && Type == other.Type && Type != ElementType.None;
        }

        /// <summary>
        /// Элемент ToString.
        /// </summary>
        public override string ToString()
        {
            return $"Element({Type}, {X},{Y}, Owner:{OwningHero})";
        }
    }
}
