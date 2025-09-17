using System.Collections.Generic;
using UnityEngine;

namespace TetrisInv.Runtime
{
    public class TetrisInventoryHandler<T> where T : ItemType
    {
        private List<IInventory<T>> _inventories = new();

        public void AddInventory(IInventory<T> inventory)
        {
            _inventories.Add(inventory);
        }

        public bool AddItemAtPosition(int invIndex, ItemStack<T> item)
        {
            return _inventories[invIndex].AddItemAtPosition(item);
        }

        public ItemStack<T> GetItemOfType(int invIndex, ItemType type)
        {
            return _inventories[invIndex].GetItemOfType(type);
        }

        public void AddAnywhere(int invIndex, ItemStack<T> item)
        {
            _inventories[invIndex].AddAnywhere(item);
        }

        public void RemoveAmountFromPosition(int invIndex, Vector2Int position, int amount)
        {
            _inventories[invIndex].RemoveAmountFromPosition(position, amount);
        }

        public ItemStack<T> RemoveItem(int invIndex, Vector2Int position)
        {
            return _inventories[invIndex].RemoveItem(position);
        }

        public ItemStack<T> RemoveItemOfType(int invIndex, ItemType type)
        {
            return _inventories[invIndex].RemoveItemOfType(type);
        }

        public void ReplaceItem(int invIndex, Vector2Int replaceItemFromThisPosition, ItemStack<T> replaceWith)
        {
            _inventories[invIndex].ReplaceItem(replaceItemFromThisPosition, replaceWith);
        }

        public ItemStack<T> GetItemAtPosition(int invIndex, Vector2Int position)
        {
            return _inventories[invIndex].GetItemAtPosition(position);
        }
    }
}