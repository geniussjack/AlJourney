using System.Collections.Generic;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для генерации лута (наград).
    /// Отвечает за создание случайных предметов снаряжения по итогам прохождения обычных волн или боссов.
    /// </summary>
    public interface ILootSystem
    {
        /// <summary>
        /// Генерирует список предметов в качестве награды за победу над боссом.
        /// Качество и количество предметов зависят от номера волны.
        /// </summary>
        List<EquipmentData> GenerateBossLoot(int waveNumber);

        /// <summary>
        /// Генерирует один предмет снаряжения в качестве награды за прохождение обычной волны.
        /// </summary>
        EquipmentData GenerateNormalLoot(int waveNumber);
    }
}
