using AlJourney.Scripts.Core;
using Godot;

namespace AlJourney.Scripts.Characters
{
    /// <summary>
    /// Represents a player-controlled character (Mage or Warrior).
    /// </summary>
    public partial class PlayerCharacter : Character
    {
        /// <summary>
        /// Character class type.
        /// </summary>
        public CharacterClass CharacterClass { get; private set; }

        /// <summary>
        /// Creates a new player character based on class.
        /// </summary>
        public static PlayerCharacter Create(CharacterClass characterClass)
        {
            PlayerCharacter player = new()
            {
                CharacterClass = characterClass
            };

            switch (characterClass)
            {
                case CharacterClass.Mage:
                    player.Initialize(
                        "Eltarion",
                        GameConstants.MAGE_BASE_HP,
                        GameConstants.MAGE_BASE_DAMAGE,
                        GameConstants.MAGE_BASE_DEFENSE,
                        AttackType.Magical
                    );
                    break;

                case CharacterClass.Warrior:
                    player.Initialize(
                        "Eldric",
                        GameConstants.WARRIOR_BASE_HP,
                        GameConstants.WARRIOR_BASE_DAMAGE,
                        GameConstants.WARRIOR_BASE_DEFENSE,
                        AttackType.Physical
                    );
                    break;
            }

            GD.Print($"[PlayerCharacter] Created {player._name} ({characterClass})");
            return player;
        }

        /// <summary>
        /// Initializes player from save data.
        /// </summary>
        public void InitializeFromSave(string name, int maxHealth, int currentHealth, int damage, int defense, CharacterClass characterClass)
        {
            CharacterClass = characterClass;
            _name = name;
            _maxHealth = maxHealth;
            _currentHealth = currentHealth;
            _baseDamage = damage;
            _baseDefense = defense;
            _currentShield = 0;
            _attackType = characterClass == CharacterClass.Mage ? AttackType.Magical : AttackType.Physical;

            _ = EmitSignal(SignalName.HealthChanged, _currentHealth, _maxHealth);
            GD.Print($"[PlayerCharacter] Loaded {_name} from save - HP: {_currentHealth}/{_maxHealth}");
        }

        /// <summary>
        /// Applies damage with character-specific modifiers.
        /// Mage deals +0% bonus to fire damage (base).
        /// Warrior deals +0% bonus to physical damage (base).
        /// Future: Can be modified by artifacts/upgrades.
        /// </summary>
        public int CalculateDamage(int baseDamage, ElementType _)
        {
            int finalDamage = baseDamage + _baseDamage;

            // Character-specific modifiers (currently none, but ready for artifacts)
            // Example: if (_characterClass == CharacterClass.Mage && elementType == ElementType.Fire)
            //     finalDamage = Mathf.CeilToInt(finalDamage * 1.5f);

            return finalDamage;
        }

        /// <summary>
        /// Applies healing with character-specific modifiers.
        /// </summary>
        public static int CalculateHealing(int baseHealing)
        {
            int finalHealing = baseHealing;

            // Future: Mage could have healing bonus from artifacts
            // Example: if (_characterClass == CharacterClass.Mage)
            //     finalHealing = Mathf.CeilToInt(finalHealing * 1.2f);

            return finalHealing;
        }

        /// <summary>
        /// Applies shield with character-specific modifiers.
        /// </summary>
        public static int CalculateShield(int baseShield)
        {
            int finalShield = baseShield;

            // Future: Warrior could have shield bonus from artifacts
            // Example: if (_characterClass == CharacterClass.Warrior)
            //     finalShield = Mathf.CeilToInt(finalShield * 1.5f);

            return finalShield;
        }

        /// <summary>
        /// Gets character stats for saving.
        /// </summary>
        public (int maxHealth, int currentHealth, int damage, int defense) GetStats()
        {
            return (_maxHealth, _currentHealth, _baseDamage, _baseDefense);
        }
    }
}
