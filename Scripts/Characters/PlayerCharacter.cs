using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Characters
{
    /// <summary>
    /// Основной класс игрового персонажа, наследующийся от Character.
    /// Управляет базовыми характеристиками, применением экипировки, способностями и расчетом наносимого урона.
    /// </summary>
    public partial class PlayerCharacter : Character
    {
        /// <summary>
        /// Класс данного персонажа. Доступен только для чтения.
        /// </summary>
        public CharacterClass CharacterClass { get; private set; }

        /// <summary>
        /// Фабричный метод для создания и инициализации нового персонажа определенного класса.
        /// </summary>
        /// <param name="characterClass">Тип создаваемого класса.</param>
        /// <returns>Новый настроенный экземпляр персонажа.</returns>
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
        /// Инициализирует персонажа данными, полученными из файла сохранения.
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
        /// Общий показатель защиты персонажа, включающий базовую защиту и бонусы от экипировки и активных способностей.
        /// </summary>
        public override int TotalDefense => _baseDefense + GetEquipmentStat("defense") + GetAbilityStat("defense");

        /// <summary>
        /// Общий максимальный запас здоровья персонажа, рассчитываемый с учетом базового здоровья и как плоских, так и процентных бонусов от экипировки и способностей.
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
        /// Рассчитывает итоговый урон атаки, учитывая базовый урон, бонусы от экипировки, способностей и статусные эффекты.
        /// </summary>
        /// <param name="baseDamage">Базовый урон, наносимый атакой.</param>
        /// <param name="elementType">Тип элемента атаки, определяющий, будет ли урон магическим или физическим.</param>
        /// <returns>Конечное количество урона после всех расчетов.</returns>
        public int CalculateDamage(int baseDamage, ElementType elementType)
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
        /// Рассчитывает итоговое значение лечения, которое может быть усилено дополнительными модификаторами.
        /// </summary>
        /// <param name="baseHealing">Базовое значение исцеления.</param>
        /// <returns>Конечное значение исцеления.</returns>
        public static int CalculateHealing(int baseHealing)
        {
            int finalHealing = baseHealing;


            return finalHealing;
        }

        /// <summary>
        /// Рассчитывает итоговое значение прочности щита, которое накладывается на персонажа.
        /// </summary>
        /// <param name="baseShield">Базовое значение щита.</param>
        /// <returns>Конечное значение прочности щита.</returns>
        public static int CalculateShield(int baseShield)
        {
            int finalShield = baseShield;


            return finalShield;
        }

        /// <summary>
        /// Получает текущие характеристики персонажа: максимальное здоровье, текущее здоровье, урон и защиту.
        /// </summary>
        /// <returns>Кортеж со значениями характеристик персонажа.</returns>
        public (int maxHealth, int currentHealth, int damage, int defense) GetStats()
        {
            int dmg = _baseDamage + GetEquipmentStat("damage") + GetAbilityStat("damage");
            return (TotalMaxHealth, _currentHealth, dmg, TotalDefense);
        }
    }
}
