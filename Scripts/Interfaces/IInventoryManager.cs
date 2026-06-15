using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using System.Collections.Generic;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс управления инвентарем.
    /// Обеспечивает логику добавления, экипировки и снятия предметов снаряжения героев.
    /// </summary>
    public interface IInventoryManager
    {
        /// <summary>
        /// Добавляет список предметов в общий инвентарь игрока.
        /// </summary>
        void AddItems(List<EquipmentData> items);

        /// <summary>
        /// Экипирует предмет из инвентаря указанному классу героя.
        /// Возвращает true, если предмет был успешно экипирован.
        /// </summary>
        bool EquipItem(CharacterClass hero, EquipmentData item);

        /// <summary>
        /// Снимает предмет с указанного слота экипировки героя и возвращает его в инвентарь.
        /// </summary>
        /// <returns>Возвращает снятый предмет, либо null, если слот был пуст.</returns>
        EquipmentData UnequipItem(CharacterClass hero, EquipmentSlot slot);
    }
}
