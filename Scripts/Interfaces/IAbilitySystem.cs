using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using System.Collections.Generic;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для управления способностями героев.
    /// Предоставляет методы для получения, разблокировки и экипировки способностей, а также расчета их эффектов.
    /// </summary>
    public interface IAbilitySystem
    {
        /// <summary>
        /// Возвращает список всех способностей, доступных для указанного класса героя.
        /// </summary>
        List<AbilityData> GetAvailableAbilities(CharacterClass heroClass);

        /// <summary>
        /// Возвращает список способностей, которые в данный момент экипированы у указанного героя.
        /// </summary>
        List<AbilityData> GetEquippedAbilities(CharacterClass heroClass);

        /// <summary>
        /// Разблокирует указанную способность для заданного героя, делая ее доступной для экипировки.
        /// </summary>
        bool UnlockAbility(CharacterClass hero, AbilityData ability);

        /// <summary>
        /// Экипирует способность герою, применяя её эффекты в игре.
        /// </summary>
        bool EquipAbility(CharacterClass hero, AbilityData ability);

        /// <summary>
        /// Получает суммарное числовое значение конкретного эффекта, исходя из экипированных способностей героя.
        /// </summary>
        int GetAbilityEffect(CharacterClass hero, string effectName);

        /// <summary>
        /// Возвращает словарь всех суммарных бонусов к характеристикам, которые дают активные способности героя.
        /// </summary>
        Dictionary<string, int> GetTotalAbilityStats(CharacterClass hero);
    }
}
