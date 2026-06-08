using AlJourney.Scripts.Core;
using System.Collections.Generic;

namespace AlJourney.Scripts.Match3
{
    /// <summary>
    /// Основной класс MatchResult.
    /// </summary>
    public class MatchResult(ElementType elementType, int matchCount, bool isHorizontal)
    {
        /// <summary>
        /// Элемент ElementType.
        /// </summary>
        public ElementType ElementType { get; set; } = elementType;

        /// <summary>
        /// Элемент MatchCount.
        /// </summary>
        public int MatchCount { get; set; } = matchCount;

        public List<(int x, int y)> MatchedPositions { get; set; } = [];

        /// <summary>
        /// Проверяет, является ли Horizontal.
        /// </summary>
        public bool IsHorizontal { get; set; } = isHorizontal;

        /// <summary>
        /// Возвращает ComboLevel.
        /// </summary>
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
        /// Элемент ToString.
        /// </summary>
        public override string ToString()
        {
            return $"Match: {MatchCount}x {ElementType} ({(IsHorizontal ? "H" : "V")})";
        }
    }
}
