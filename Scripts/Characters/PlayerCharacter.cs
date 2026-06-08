using AlJourney.Scripts.Core;
using Godot;

namespace AlJourney.Scripts.Characters
{
    /// <summary>
    /// Основной класс PlayerCharacter.
    /// </summary>
    public partial class PlayerCharacter : Character
    {
        public CharacterClass CharacterClass { get; private set; }

        /// <summary>
        /// Элемент Create.
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
                        "Altarion",
                        GameConstants.MAGE_BASE_HP,
                        GameConstants.MAGE_BASE_DAMAGE,
                        GameConstants.MAGE_BASE_DEFENSE,
                        AttackType.Magical
                    );
                    break;

                case CharacterClass.Warrior:
                    player.Initialize(
                        "Aldric",
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
        /// Инициализирует FromSave.
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
            if (AlJourney.Scripts.Managers.InventoryManager.Instance == null) return 0;
            var equipment = AlJourney.Scripts.Managers.InventoryManager.Instance.GetHeroEquipment(CharacterClass);
            int total = 0;
            foreach (var item in equipment.Values)
            {
                if (item.GetTotalStats().TryGetValue(statName, out int value)) total += value;
            }
            return total;
        }

        private int GetAbilityStat(string statName)
        {
            if (AlJourney.Scripts.Managers.AbilitySystem.Instance == null) return 0;
            return AlJourney.Scripts.Managers.AbilitySystem.Instance.GetAbilityEffect(CharacterClass, statName);
        }

        /// <summary>
        /// Элемент TotalDefense.
        /// </summary>
        public override int TotalDefense => _baseDefense + GetEquipmentStat("defense") + GetAbilityStat("defense");

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
        /// Элемент CalculateDamage.
        /// </summary>
        public int CalculateDamage(int baseDamage, ElementType elementType)
        {
            string statName = elementType == ElementType.Fire || elementType == ElementType.Heal ? "magic_damage" : "damage";
            int equipBonus = GetEquipmentStat(statName) + GetEquipmentStat("damage");
            int abilityBonus = GetAbilityStat(statName) + GetAbilityStat("damage");
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
        /// Элемент CalculateHealing.
        /// </summary>
        public static int CalculateHealing(int baseHealing)
        {
            int finalHealing = baseHealing;


            return finalHealing;
        }

        /// <summary>
        /// Элемент CalculateShield.
        /// </summary>
        public static int CalculateShield(int baseShield)
        {
            int finalShield = baseShield;


            return finalShield;
        }

        public (int maxHealth, int currentHealth, int damage, int defense) GetStats()
        {
            int dmg = _baseDamage + GetEquipmentStat("damage") + GetAbilityStat("damage");
            return (TotalMaxHealth, _currentHealth, dmg, TotalDefense);
        }
    }
}
