using System.Collections.Generic;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс управления инвентарем.
    /// </summary>
    /// <summary>
    /// Менеджер IInventoryManager. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public interface IInventoryManager
    {
        void AddItems(List<EquipmentData> items);
        bool EquipItem(CharacterClass hero, EquipmentData item);
        EquipmentData UnequipItem(CharacterClass hero, EquipmentSlot slot);
    }
}
