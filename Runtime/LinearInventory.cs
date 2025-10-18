using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TetrisInv.Runtime
{
    public class LinearInventory<T> : BaseInventory<T> where T : ItemType
    {
        [Header("Debug Info DO NOT EDIT")]
        [field: SerializeField] public ItemStack<T>[] Items { get; private set; }

        public LinearInventory(int capacity)
        {
            Items = new ItemStack<T>[capacity];
        }

        /// <summary>
        /// Tries to add item to the index = item.position.x in the ItemStack
        /// </summary>
        /// <param name="item">The item to be added</param>
        /// <returns>True if was fully added, false if not added or partially added - item parameter is modified to leftover amount</returns>
        public override bool AddItemAtPosition(ItemStack<T> item)
        {
            if (Items[item.position.x] == null)
            {
                if (item.amount > item.itemType.StackSize)
                {
                    var itemStack = item.CopyNewAmount(item.itemType.StackSize);
                    Items[item.position.x] = itemStack;
                    OnOnItemAdded(itemStack);
                    item.amount -= item.itemType.StackSize;
                    return false;
                }

                Items[item.position.x] = item;
                OnOnItemAdded(item);
                return true;
            }

            if (Items[item.position.x].itemType != item.itemType)
            {
                return false;
            }

            var itemAtPosition = Items[item.position.x];
            itemAtPosition.amount += item.amount;
            if (itemAtPosition.amount <= item.itemType.StackSize)
            {
                OnOnItemChanged(itemAtPosition);
                return true;
            }

            var diff = itemAtPosition.amount - item.itemType.StackSize;
            itemAtPosition.amount = item.itemType.StackSize;
            OnOnItemChanged(itemAtPosition);
            item.amount = diff;
            return false;
        }

        public override ItemStack<T> GetItemOfType(ItemType type)
        {
            foreach (var itemStack in Items)
            {
                if (itemStack == null) continue;
                if (itemStack.itemType == type) return itemStack;
            }

            return null;
        }

        public override bool AddAnywhere(ItemStack<T> item)
        {
            var sameItem = Items.FirstOrDefault(x =>
                x != null && x.itemType == item.itemType && x.amount <= item.itemType.StackSize);
            if (sameItem != null && sameItem.itemType != null)
            {
                item.position = sameItem.position;
                if (AddItemAtPosition(item))
                {
                    return true;
                }
            }

            for (var i = 0; i < Items.Length; i++)
            {
                item.position.x = i;
                if (AddItemAtPosition(item)) return true;
            }

            return false;
        }

        public override void RemoveAmountFromPosition(Vector2Int position, int amount)
        {
            var itemStack = Items[position.x];
            if (itemStack == null) return;
            itemStack.amount -= amount;
            if (itemStack.amount <= 0)
            {
                Items[position.x] = null;
                OnOnItemRemoved(itemStack);
                return;
            }

            OnOnItemChanged(itemStack);
        }

        public override ItemStack<T> RemoveItem(Vector2Int position)
        {
            if (Items[position.x] == null) return null;
            var itemStack = Items[position.x];
            Items[position.x] = null;
            OnOnItemRemoved(itemStack);
            return itemStack;
        }

        public override ItemStack<T> RemoveItemOfType(ItemType type)
        {
            for (var i = 0; i < Items.Length; i++)
            {
                var itemStack = Items[i];
                if (itemStack == null) continue;
                if (itemStack.itemType == type)
                {
                    Items[i] = null;
                    OnOnItemRemoved(itemStack);
                    return itemStack;
                }
            }

            return null;
        }

        public override void ReplaceItem(Vector2Int replaceItemFromThisPosition, ItemStack<T> replaceWith)
        {
            if (Items[replaceItemFromThisPosition.x] == null)
            {
                Items[replaceItemFromThisPosition.x] = replaceWith;
                OnOnItemAdded(replaceWith);
                return;
            }
            
            var original = Items[replaceItemFromThisPosition.x];
            OnOnItemReplaced(original, replaceWith);
            Items[replaceItemFromThisPosition.x] = replaceWith;
        }

        public override ItemStack<T> GetItemAtPosition(Vector2Int position)
        {
            if (Items[position.x] == null) return null;
            return Items[position.x];
        }
    }
}