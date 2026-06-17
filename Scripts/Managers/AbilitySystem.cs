using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Interfaces;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Менеджер системы способностей. Отвечает за управление разблокировкой, экипировкой и применением эффектов способностей для персонажей.
    /// </summary>
    public partial class AbilitySystem : Node, IAbilitySystem
    {
        /// <summary>
        /// Глобальный экземпляр менеджера системы способностей.
        /// </summary>
        public static AbilitySystem Instance { get; private set; } = null!;

        private readonly Dictionary<string, AbilityData> _abilityTemplates = AbilityDatabase.Templates;
        private readonly Dictionary<CharacterClass, List<AbilityData>> _equippedAbilities = [];

        /// <summary>
        /// Инициализирует узел системы способностей при его добавлении в дерево сцены.
        /// Гарантирует существование только одного экземпляра менеджера.
        /// </summary>
        public override void _Ready()
        {
            if (Instance is not null)
            {
                QueueFree();
                return;
            }
            Instance = this;
            GD.Print("[AbilitySystem] Initialized");
        }

        /// <summary>
        /// Получает список всех способностей, которые доступны для указанного класса персонажа.
        /// </summary>
        /// <param name="heroClass">Класс персонажа, для которого необходимо получить доступные способности.</param>
        /// <returns>Список доступных способностей.</returns>
        public List<AbilityData> GetAvailableAbilities(CharacterClass heroClass)
        {
            return [.. _abilityTemplates.Values.Where(ability => IsAbilityForHero(ability, heroClass))];
        }

        /// <summary>
        /// Получает список способностей, которые в данный момент экипированы указанным классом персонажа.
        /// </summary>
        /// <param name="heroClass">Класс персонажа, экипированные способности которого нужно получить.</param>
        /// <returns>Список экипированных способностей или пустой список, если ни одна способность не экипирована.</returns>
        public List<AbilityData> GetEquippedAbilities(CharacterClass heroClass)
        {
            return _equippedAbilities.TryGetValue(heroClass, out List<AbilityData> abilities) ? abilities : [];
        }

        /// <summary>
        /// Разблокирует указанную способность для заданного персонажа за игровую валюту.
        /// </summary>
        /// <param name="hero">Класс персонажа, для которого разблокируется способность.</param>
        /// <param name="ability">Данные способности, которую необходимо разблокировать.</param>
        /// <returns><c>true</c>, если способность была успешно разблокирована; <c>false</c>, если не хватает монет.</returns>
        public bool UnlockAbility(CharacterClass hero, AbilityData ability)
        {
            if (GameStateManager.Instance.Coins < ability.UnlockCost)
            {
                return false;
            }

            _ = GameStateManager.Instance.SpendCoins(ability.UnlockCost);

            if (!_equippedAbilities.TryGetValue(hero, out List<AbilityData> value))
            {
                value = [];
                _equippedAbilities[hero] = value;
            }

            value.Add(ability);
            GD.Print($"[SimpleAbilitySystem] Unlocked ability {ability.Name} for {hero}");
            return true;
        }

        /// <summary>
        /// Экипирует указанную способность для заданного персонажа. У одного персонажа не может быть экипировано более 3 способностей.
        /// </summary>
        /// <param name="hero">Класс персонажа, которому экипируется способность.</param>
        /// <param name="ability">Способность, которую нужно экипировать.</param>
        /// <returns><c>true</c>, если способность была успешно экипирована; <c>false</c>, если достигнут лимит в 3 способности.</returns>
        public bool EquipAbility(CharacterClass hero, AbilityData ability)
        {
            if (!_equippedAbilities.TryGetValue(hero, out List<AbilityData> value))
            {
                value = [];
                _equippedAbilities[hero] = value;
            }

            if (value.Count >= 3)
            {
                return false;
            }

            value.Add(ability);
            GD.Print($"[SimpleAbilitySystem] Equipped ability {ability.Name} for {hero}");
            return true;
        }

        /// <summary>
        /// Вычисляет и возвращает суммарное значение конкретного эффекта от всех экипированных способностей персонажа.
        /// </summary>
        /// <param name="hero">Класс персонажа.</param>
        /// <param name="effectName">Название эффекта для поиска.</param>
        /// <returns>Суммарное значение эффекта.</returns>
        public int GetAbilityEffect(CharacterClass hero, string effectName)
        {
            return !_equippedAbilities.TryGetValue(hero, out List<AbilityData> abilities) ? 0 : abilities.Sum(ability => ability.GetEffect(effectName));
        }

        /// <summary>
        /// Получает словарь с суммарными значениями всех характеристик от экипированных способностей указанного персонажа.
        /// </summary>
        /// <param name="hero">Класс персонажа.</param>
        /// <returns>Словарь, где ключ — название характеристики, а значение — её суммарный бонус.</returns>
        public Dictionary<string, int> GetTotalAbilityStats(CharacterClass hero)
        {
            Dictionary<string, int> totalStats = [];

            if (!_equippedAbilities.TryGetValue(hero, out List<AbilityData> abilities))
            {
                return totalStats;
            }

            foreach (AbilityData ability in abilities)
            {
                foreach (KeyValuePair<string, int> effect in ability.Effects)
                {
                    if (totalStats.ContainsKey(effect.Key))
                    {
                        totalStats[effect.Key] += effect.Value;
                    }
                    else
                    {
                        totalStats[effect.Key] = effect.Value;
                    }
                }
            }

            return totalStats;
        }

        private static bool IsAbilityForHero(AbilityData ability, CharacterClass heroClass)
        {
            return (heroClass == CharacterClass.Mage && (ability.Element == AbilityElement.Fire || ability.Element == AbilityElement.Heal)) ||
                   (heroClass == CharacterClass.Warrior && (ability.Element == AbilityElement.Sword || ability.Element == AbilityElement.Shield));
        }
    }
}
