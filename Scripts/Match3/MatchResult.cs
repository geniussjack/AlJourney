using System.Collections.Generic;
using AltarionsJourney.Core;

namespace AltarionsJourney.Match3
{
    /// <summary>
    /// Represents the result of a match detection.
    /// </summary>
    public class MatchResult
    {
        /// <summary>
        /// Type of element that was matched.
        /// </summary>
        public ElementType ElementType { get; set; }

        /// <summary>
        /// Number of elements in the match (3, 4, or 5).
        /// </summary>
        public int MatchCount { get; set; }

        /// <summary>
        /// List of matched element positions.
        /// </summary>
        public List<(int x, int y)> MatchedPositions { get; set; }

        /// <summary>
        /// Is this a horizontal match.
        /// </summary>
        public bool IsHorizontal { get; set; }

        public MatchResult(ElementType elementType, int matchCount, bool isHorizontal)
        {
            ElementType = elementType;
            MatchCount = matchCount;
            IsHorizontal = isHorizontal;
            MatchedPositions = new List<(int x, int y)>();
        }

        /// <summary>
        /// Returns the combo level (0 for no match, 1 for 3-match, 2 for 4-match, 3 for 5-match).
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

        public override string ToString()
        {
            return $"Match: {MatchCount}x {ElementType} ({(IsHorizontal ? "H" : "V")})";
        }
    }
}