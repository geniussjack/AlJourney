using System.Collections.Generic;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для управления способностями героев.
    /// </summary>
    /// <summary>
    /// Менеджер IAbilitySystem. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public interface IAbilitySystem
    {
        List<AbilityData> GetAvailableAbilities(CharacterClass heroClass);
        List<AbilityData> GetEquippedAbilities(CharacterClass heroClass);
        bool UnlockAbility(CharacterClass hero, AbilityData ability);
        bool EquipAbility(CharacterClass hero, AbilityData ability);
        int GetAbilityEffect(CharacterClass hero, string effectName);
        Dictionary<string, int> GetTotalAbilityStats(CharacterClass hero);
    }
}
