using UnityEngine;

namespace TetrisInv.Runtime
{
    public interface IInventory<T> where T : ItemType
    {
        /// <summary>
        /// Tries to add item to the location specified in the ItemStack
        /// </summary>
        /// <param name="item">The item to be added</param>
        /// <returns>True if was fully added, false if not added or partially added - item parameter is modified to leftover amount</returns>
        public bool AddItemAtPosition(ItemStack<T> item);

        /// <summary>
        /// Produces the first item found in the inventory of a given type not necessarily the first in order of items on the grid
        /// </summary>
        /// <param name="type">The wanted item type</param>
        /// <returns>The item found, null if not found</returns>
        public ItemStack<T> GetItemOfType(ItemType type);

        /// <summary>
        /// Follows the rules below to try to add the item anywhere in the inventory:
        /// - Tries first to find a non-full slot of item of the same type
        /// - Left over goes top to down left to right to the next available slot
        /// - Left over causes OnItemOverflow event
        /// </summary>
        /// <remarks>Triggers OnItemOverflow event if after filling the inventory there is still left over items.</remarks>
        /// <param name="item">The item to add</param>
        public void AddAnywhere(ItemStack<T> item);

        /// <summary>
        /// Removes a certain amount of items from a given slot
        /// </summary>
        /// <param name="position">The slot to remove from</param>
        /// <param name="amount">Amount to remove</param>
        public void RemoveAmountFromPosition(Vector2Int position, int amount);

        /// <summary>
        /// Removes the item at given location
        /// </summary>
        /// <param name="position">Location to remove from</param>
        /// <returns>Item removed, null if none was present</returns>
        public ItemStack<T> RemoveItem(Vector2Int position);

        /// <summary>
        /// Removes the first item of type not necessarily the first in order of items on the grid
        /// </summary>
        /// <param name="type">The item type to remove</param>
        /// <returns>The item removed, null if none was removed</returns>
        public ItemStack<T> RemoveItemOfType(ItemType type);

        /// <summary>
        /// Replaces the item at the given strict position (top left) with the given item
        /// </summary>
        /// <param name="replaceItemFromThisPosition">The position to replace</param>
        /// <param name="replaceWith">The item to replace with</param>
        public void ReplaceItem(Vector2Int replaceItemFromThisPosition, ItemStack<T> replaceWith);

        /// <summary>
        /// Gets the item that covers the given position, does not need to be the strict position (top left)
        /// </summary>
        /// <param name="position">The given position</param>
        /// <returns>The item at position</returns>
        public ItemStack<T> GetItemAtPosition(Vector2Int position);
    }
}