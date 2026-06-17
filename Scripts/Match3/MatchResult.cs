using AlJourney.Scripts.Core;
using System.Collections.Generic;

namespace AlJourney.Scripts.Match3
{
    /// <summary>
    /// Представляет результат успешно собранной комбинации элементов.
    /// Хранит данные о типе собранных элементов, их количестве, направлении комбинации
    /// и точных позициях на игровом поле.
    /// </summary>
    public class MatchResult(ElementType elementType, int matchCount, bool isHorizontal)
    {
        /// <summary>
        /// Тип элементов, из которых была собрана данная комбинация.
        /// </summary>
        public ElementType ElementType { get; set; } = elementType;

        /// <summary>
        /// Общее количество элементов в данной комбинации.
        /// Влияет на итоговый уровень комбо.
        /// </summary>
        public int MatchCount { get; set; } = matchCount;

        /// <summary>
        /// Список координат всех элементов на поле, которые участвуют в этой комбинации.
        /// Используется для их удаления и создания визуальных эффектов.
        /// </summary>
        public List<(int x, int y)> MatchedPositions { get; set; } = [];

        /// <summary>
        /// Флаг, указывающий на направление собранной линии.
        /// Если true — комбинация собрана по горизонтали, если false — по вертикали.
        /// </summary>
        public bool IsHorizontal { get; set; } = isHorizontal;

        /// <summary>
        /// Вычисляет и возвращает уровень комбо на основе количества собранных элементов в линии.
        /// 3 элемента = 1 уровень; 4 элемента = 2 уровень; 5 и более = 3 уровень.
        /// </summary>
        /// <returns>Целое число, представляющее уровень комбо.</returns>
        public int GetComboLevel()
        {
            return MatchCount switch
            {
                3 => 1,
                4 => 2,
                >= 5 => 3,
                _ => 0
            };
        }

        /// <summary>
        /// Возвращает строковое представление результата комбинации, содержащее количество элементов, их тип и направление.
        /// Используется для логирования и отладки.
        /// </summary>
        public override string ToString()
        {
            return $"Match: {MatchCount}x {ElementType} ({(IsHorizontal ? "H" : "V")})";
        }
    }
}
