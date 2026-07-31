using AlJourney.Scripts.Data;
using System.Collections.Generic;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Interface for loot generation.
    /// Responsible for creating random equipment items as rewards for clearing normal waves or defeating bosses.
    /// </summary>
    public interface ILootSystem
    {
        /// <summary>
        /// Generates a list of items as a reward for defeating a boss.
        /// Item quality and quantity depend on the wave number.
        /// </summary>
        List<EquipmentData> GenerateBossLoot(int waveNumber);

        /// <summary>
        /// Generates a single equipment item as a reward for clearing a normal wave.
        /// </summary>
        EquipmentData GenerateNormalLoot(int waveNumber);
    }
}
