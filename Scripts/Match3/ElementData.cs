using AlJourney.Scripts.Core;

namespace AlJourney.Scripts.Match3
{
    /// <summary>
    /// Представляет данные отдельного игрового элемента на поле Match-3.
    /// Хранит тип элемента, его текущие координаты, состояние (участвует ли в совпадении или падает),
    /// а также героя, которому принадлежит данный элемент.
    /// </summary>
    public class ElementData(ElementType type, int x, int y)
    {
        /// <summary>
        /// Тип элемента (например, Огонь, Меч, Лечение, Щит).
        /// Определяет, какой эффект сработает при сборе комбинации.
        /// </summary>
        public ElementType Type { get; set; } = type;

        /// <summary>
        /// Текущая позиция элемента на игровом поле по оси X (столбец).
        /// </summary>
        public int X { get; set; } = x;

        /// <summary>
        /// Текущая позиция элемента на игровом поле по оси Y (строка).
        /// </summary>
        public int Y { get; set; } = y;

        /// <summary>
        /// Указывает, был ли данный элемент отмечен как часть собранной комбинации.
        /// Если true, элемент будет удален с поля на следующем этапе обработки.
        /// </summary>
        public bool IsMatched { get; set; } = false;

        /// <summary>
        /// Указывает, находится ли элемент в состоянии падения (смещения вниз) после удаления элементов под ним.
        /// </summary>
        public bool IsFalling { get; set; } = false;

        /// <summary>
        /// Класс персонажа (героя), который привязан к данному типу элемента.
        /// Определяется автоматически в зависимости от типа элемента (например, мечи принадлежат Воину).
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
        /// Создает новый элемент случайного типа по указанным координатам.
        /// Тип выбирается случайным образом из доступного пула элементов.
        /// </summary>
        /// <param name="x">Позиция по оси X для нового элемента.</param>
        /// <param name="y">Позиция по оси Y для нового элемента.</param>
        /// <returns>Новый экземпляр данных элемента (ElementData).</returns>
        public static ElementData CreateRandom(int x, int y)
        {
            ElementType randomType = (ElementType)Godot.GD.RandRange(1, 4);
            return new ElementData(randomType, x, y);
        }

        /// <summary>
        /// Проверяет, может ли данный элемент образовать совпадение с другим переданным элементом.
        /// Совпадение возможно только если оба элемента имеют одинаковый тип, и этот тип не является пустóтой (None).
        /// </summary>
        /// <param name="other">Другой элемент для проверки совпадения.</param>
        /// <returns>True, если элементы могут образовать комбинацию; иначе False.</returns>
        public bool CanMatchWith(ElementData other)
        {
            return other != null && Type == other.Type && Type != ElementType.None;
        }

        /// <summary>
        /// Возвращает строковое представление элемента, включающее его тип, координаты и владельца.
        /// Удобно для вывода отладочной информации в консоль.
        /// </summary>
        public override string ToString()
        {
            return $"Element({Type}, {X},{Y}, Owner:{OwningHero})";
        }
    }
}
