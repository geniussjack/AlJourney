using AlJourney.Scripts.Core;

namespace AlJourney.Scripts.Match3
{
    /// <summary>
    /// Represents a match-3 grid element with its type and properties.
    /// </summary>
    public class ElementData(ElementType type, int x, int y)
    {
        /// <summary>
        /// Type of this element.
        /// </summary>
        public ElementType Type { get; set; } = type;

        /// <summary>
        /// Grid position X coordinate.
        /// </summary>
        public int X { get; set; } = x;

        /// <summary>
        /// Grid position Y coordinate.
        /// </summary>
        public int Y { get; set; } = y;

        /// <summary>
        /// Is this element currently being matched/destroyed.
        /// </summary>
        public bool IsMatched { get; set; } = false;

        /// <summary>
        /// Is this element currently falling.
        /// </summary>
        public bool IsFalling { get; set; } = false;

        /// <summary>
        /// Creates a random element (excluding None).
        /// </summary>
        public static ElementData CreateRandom(int x, int y)
        {
            // Get random element type (1-4, excluding None)
            var randomType = (ElementType)Godot.GD.RandRange(1, 4);
            return new ElementData(randomType, x, y);
        }

        /// <summary>
        /// Checks if this element can match with another.
        /// </summary>
        public bool CanMatchWith(ElementData other)
        {
            if (other == null) return false;
            return Type == other.Type && Type != ElementType.None;
        }

        public override string ToString()
        {
            return $"Element({Type}, {X},{Y})";
        }
    }
}