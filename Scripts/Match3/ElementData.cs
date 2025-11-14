using AltarionsJourney.Core;

namespace AltarionsJourney.Managers
{
    /// <summary>
    /// Represents a match-3 grid element with its type and properties.
    /// </summary>
    public class ElementData
    {
        /// <summary>
        /// Type of this element.
        /// </summary>
        public ElementType Type { get; set; }

        /// <summary>
        /// Grid position X coordinate.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Grid position Y coordinate.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Is this element currently being matched/destroyed.
        /// </summary>
        public bool IsMatched { get; set; }

        /// <summary>
        /// Is this element currently falling.
        /// </summary>
        public bool IsFalling { get; set; }

        public ElementData(ElementType type, int x, int y)
        {
            Type = type;
            X = x;
            Y = y;
            IsMatched = false;
            IsFalling = false;
        }

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