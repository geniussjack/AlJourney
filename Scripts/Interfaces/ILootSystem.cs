using System.Collections.Generic;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для генерации лута.
    /// </summary>
    /// <summary>
    /// Менеджер ILootSystem. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public interface ILootSystem
    {
        List<EquipmentData> GenerateBossLoot(int waveNumber);
        EquipmentData GenerateNormalLoot(int waveNumber);
    }
}
