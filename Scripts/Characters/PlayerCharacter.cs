using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Characters
{
    /// <summary>
    /// The main player character class, inheriting from Character.
    /// Manages base stats, applying equipment, abilities, and damage calculation.
    /// </summary>
    public partial class PlayerCharacter : Character
    {
        /// <summary>
        /// This character's class. Read-only.
        /// </summary>
        public CharacterClass CharacterClass { get; private set; }

        /// <summary>
        /// Factory method that creates and initializes a new character of the given class.
        /// </summary>
        /// <param name="characterClass">The class to create.</param>
        /// <returns>A new, configured character instance.</returns>
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
                        "CHARACTER_MAGE",
                        GameConstants.MAGE_BASE_HP,
                        GameConstants.MAGE_BASE_DAMAGE,
                        GameConstants.MAGE_BASE_DEFENSE,
                        AttackType.Magical
                    );
                    break;

                case CharacterClass.Warrior:
                    player.Initialize(
                        "CHARACTER_WARRIOR",
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
        /// Initializes the character with data loaded from a save file.
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

            _ = EmitSignal(SignalName.HealthChanged, _currentHealth, TotalMaxHealth);
            GD.Print($"[PlayerCharacter] Loaded {_name} from save - HP: {_currentHealth}/{TotalMaxHealth}");
        }

        private int GetEquipmentStat(string statName)
        {
            if (AlJourney.Scripts.Managers.InventoryManager.Instance == null)
            {
                return 0;
            }

            Dictionary<EquipmentSlot, EquipmentData> equipment = AlJourney.Scripts.Managers.InventoryManager.Instance.GetHeroEquipment(CharacterClass);
            int total = 0;
            foreach (EquipmentData item in equipment.Values)
            {
                if (item.GetTotalStats().TryGetValue(statName, out int value))
                {
                    total += value;
                }
            }
            return total;
        }

        private int GetAbilityStat(string statName)
        {
            return (AlJourney.Scripts.Managers.AbilitySystem.Instance?.GetAbilityEffect(CharacterClass, statName)) ?? 0;
        }

        /// <summary>
        /// The character's total defense, including base defense and bonuses from equipment and active abilities.
        /// </summary>
        public override int TotalDefense => _baseDefense + GetEquipmentStat("defense") + GetAbilityStat("defense");

        /// <summary>
        /// The character's total maximum health, computed from base health plus both flat and percentage bonuses from equipment and abilities.
        /// </summary>
        public override int TotalMaxHealth
        {
            get
            {
                int hpBonus = GetEquipmentStat("hp") + GetAbilityStat("hp");
                int hpPercent = GetEquipmentStat("hp_percent") + GetAbilityStat("hp_percent");
                int baseHp = _maxHealth + hpBonus;
                return baseHp + (baseHp * hpPercent / 100);
            }
        }

        /// <summary>
        /// Computes the final attack damage, accounting for base damage, equipment and ability bonuses, and status effects.
        /// </summary>
        /// <param name="baseDamage">The base damage dealt by the attack (the ability effect's value).</param>
        /// <returns>The final damage amount after every calculation.</returns>
        public int CalculateDamage(int baseDamage)
        {
            int equipBonus = GetEquipmentStat("damage");
            int abilityBonus = GetAbilityStat("damage");
            int totalBaseDamage = _baseDamage + equipBonus + abilityBonus;
            int finalDamage = baseDamage + totalBaseDamage;

            if (HasStatusEffect(StatusEffect.Weakened))
            {
                finalDamage = Mathf.CeilToInt(finalDamage * 0.7f);
                GD.Print($"[{_name}] Damage reduced by Weakened status: {finalDamage}");
            }

            return finalDamage;
        }

        /// <summary>
        /// Computes the final healing value, which can be boosted by additional modifiers.
        /// </summary>
        /// <param name="baseHealing">The base healing value.</param>
        /// <returns>The final healing value.</returns>
        public static int CalculateHealing(int baseHealing)
        {
            return baseHealing;
        }

        /// <summary>
        /// Computes the final shield strength applied to the character.
        /// </summary>
        /// <param name="baseShield">The base shield value.</param>
        /// <returns>The final shield strength.</returns>
        public static int CalculateShield(int baseShield)
        {
            return baseShield;
        }

        /// <summary>
        /// Gets the character's current stats: maximum health, current health, damage and defense.
        /// </summary>
        /// <returns>A tuple with the character's stat values.</returns>
        public (int maxHealth, int currentHealth, int damage, int defense) GetStats()
        {
            int dmg = _baseDamage + GetEquipmentStat("damage") + GetAbilityStat("damage");
            return (TotalMaxHealth, _currentHealth, dmg, TotalDefense);
        }
    }
}
