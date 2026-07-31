using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using System.Collections.Generic;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Interface for inventory management.
    /// Provides the logic for adding, equipping and unequipping heroes' equipment items.
    /// </summary>
    public interface IInventoryManager
    {
        /// <summary>
        /// Adds a list of items to the player's shared inventory.
        /// </summary>
        void AddItems(List<EquipmentData> items);

        /// <summary>
        /// Equips an item from the inventory to the given hero class.
        /// Returns true if the item was successfully equipped.
        /// </summary>
        bool EquipItem(CharacterClass hero, EquipmentData item);

        /// <summary>
        /// Unequips an item from the given hero's equipment slot and returns it to the inventory.
        /// </summary>
        /// <returns>The unequipped item, or null if the slot was empty.</returns>
        EquipmentData UnequipItem(CharacterClass hero, EquipmentSlot slot);
    }
}
