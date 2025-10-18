using System;
using System.Collections.Generic;
using UnityEngine;

namespace TetrisInv.Runtime
{
    [Serializable]
    public class InventoryHandler<T> where T : ItemType
    {
        [field: SerializeField]
        public List<BaseInventory<T>> Inventories { get; private set; } = new();
        public event Action<ItemStack<T>> OnItemOverflow;
        
        /// <summary>
        /// Adds a new inventory to the handler's managed collection
        /// </summary>
        /// <param name="inventory">The inventory to add</param>
        public void AddInventory(TetrisInventory<T> inventory)
        {
            Inventories.Add(inventory);
        }
        
        /// <summary>
        /// Tries to add item to the location specified in the ItemStack
        /// </summary>
        /// <param name="item">The item to be added</param>
        /// <returns>True if was fully added, false if not added or partially added - item parameter is modified to leftover amount</returns>
        public bool AddItemAtPosition(ItemStack<T> item)
        {
            return Inventories[item.inventoryIndex].AddItemAtPosition(item);
        }
        
        /// <summary>
        /// Produces the first item found of a given type
        /// </summary>
        /// <param name="type">The wanted item type</param>
        /// <param name="invIndex">Target inventory index, -1 to search all inventories</param>
        /// <returns>The item found, null if not found</returns>
        public ItemStack<T> GetItemOfType(ItemType type, int invIndex = -1)
        {
            if (invIndex != -1) return Inventories[invIndex].GetItemOfType(type);
            
            foreach (var inv in Inventories)
            {
                var itemOfType = inv.GetItemOfType(type);
                if (itemOfType != null)
                {
                    return itemOfType;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Adds a given item anywhere in the inventory
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <param name="invIndex">Target inventory index, -1 to try all inventories</param>
        /// <remarks>Triggers OnItemOverflow event if overflows</remarks>
        /// <returns>True if item was successfully added</returns>
        public bool AddAnywhere(ItemStack<T> item)
        {
            if (item.inventoryIndex != -1)
            {
                var addAnywhere = Inventories[item.inventoryIndex].AddAnywhere(item);

                if (!addAnywhere)
                {
                    OnItemOverflow?.Invoke(item);
                }
                
                return addAnywhere;
            }

            for (var i = 0; i < Inventories.Count; i++)
            {
                var inv = Inventories[i];
                if (inv.AddAnywhere(item))
                {
                    item.inventoryIndex = i;
                    return true;
                }
            }

            OnItemOverflow?.Invoke(item);
            return false;
        }
        
        /// <summary>
        /// Removes the first item of type
        /// </summary>
        /// <param name="type">The item type to remove</param>
        /// <param name="invIndex">Target inventory index, -1 to search all inventories</param>
        /// <returns>The item removed, null if none was removed</returns>
        public ItemStack<T> RemoveItemOfType(ItemType type, int invIndex = -1)
        {
            if (invIndex != -1) return Inventories[invIndex].RemoveItemOfType(type);
            foreach (var inv in Inventories)
            {
                var removed = inv.RemoveItemOfType(type);
                if (removed != null) return removed;
            }
            
            return null;
        }
        
        /// <summary>
        /// Removes a certain amount of items from a given slot
        /// </summary>
        /// <param name="invIndex">Target inventory index</param>
        /// <param name="position">The slot to remove from</param>
        /// <param name="amount">Amount to remove</param>
        public void RemoveAmountFromPosition(int invIndex, Vector2Int position, int amount)
        {
            Inventories[invIndex].RemoveAmountFromPosition(position, amount);
        }
        
        /// <summary>
        /// Removes the item at given location
        /// </summary>
        /// <param name="invIndex">Target inventory index</param>
        /// <param name="position">Location to remove from</param>
        /// <returns>Item removed, null if none was present</returns>
        public ItemStack<T> RemoveItem(int invIndex, Vector2Int position)
        {
            return Inventories[invIndex].RemoveItem(position);
        }
        
        /// <summary>
        /// Replaces the item at the given position with the given item
        /// </summary>
        /// <param name="invIndex">Target inventory index</param>
        /// <param name="replaceItemFromThisPosition">The position to replace</param>
        /// <param name="replaceWith">The item to replace with</param>
        public void ReplaceItem(int invIndex, Vector2Int replaceItemFromThisPosition, ItemStack<T> replaceWith)
        {
            Inventories[invIndex].ReplaceItem(replaceItemFromThisPosition, replaceWith);
        }
        
        /// <summary>
        /// Gets the item at the given position
        /// </summary>
        /// <param name="invIndex">Target inventory index</param>
        /// <param name="position">The position to check</param>
        /// <returns>The item at position, null if none exists</returns>
        public ItemStack<T> GetItemAtPosition(int invIndex, Vector2Int position)
        {
            return Inventories[invIndex].GetItemAtPosition(position);
        }
    }
}
