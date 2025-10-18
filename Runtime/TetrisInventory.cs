using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TetrisInv.Runtime
{
    [Serializable]
    public class TetrisInventory<T> : BaseInventory<T> where T : ItemType
    {
        [Header("Debug Info DO NOT EDIT")]
        [field: SerializeField] public List<ItemStack<T>> Items { get; private set; } = new();
        [field: SerializeField] public int Width { get; private set; }
        [field: SerializeField] public int Height { get; private set; }
        
        public TetrisInventory(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Tries to add item to the location specified in the ItemStack
        /// </summary>
        /// <param name="item">The item to be added</param>
        /// <returns>True if was fully added, false if not added or partially added - item parameter is modified to leftover amount</returns>
        public override bool AddItemAtPosition(ItemStack<T> item)
        {
            var itemAtPosition = GetItemAtPosition(item.position);
            if (itemAtPosition == null)
            {
                if (!CanAddItemToSlot(item)) return false;
                if (item.amount > item.itemType.StackSize)
                {
                    var itemStack = item.CopyNewAmount(item.itemType.StackSize);
                    Items.Add(itemStack);
                    OnOnItemAdded(itemStack);
                    item.amount -= item.itemType.StackSize;
                    return false;
                }

                Items.Add(item);
                OnOnItemAdded(item);
                return true;
            }

            if (itemAtPosition.itemType == item.itemType)
            {
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
            }

            return false;
        }
        
        /// <summary>
        /// Produces the first item found in the inventory of a given type not necessarily the first in order of items on the grid
        /// </summary>
        /// <param name="type">The wanted item type</param>
        /// <returns>The item found, null if not found</returns>
        public override ItemStack<T> GetItemOfType(ItemType type)
        {
            foreach (var item in Items)
            {
                if (item.itemType == type)
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Follows the rules below to try to add the item anywhere in the inventory:
        /// - Tries first to find a non-full slot of item of the same type
        /// - Left over goes top to down left to right to the next available slot
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <returns>True if successfully added all items, false if there are left overs</returns>
        public override bool AddAnywhere(ItemStack<T> item)
        {
            var sameItem = Items.FirstOrDefault(x => x.itemType == item.itemType && x.amount <= item.itemType.StackSize);
            if (sameItem != null && sameItem.itemType != null)
            {
                item.position = sameItem.position;
                if (AddItemAtPosition(item))
                {
                    return true;
                }
            }

            for (var j = 0; j < Height; j++)
            {
                for (var i = 0; i < Width; i++)
                {
                    item.position.x = i;
                    item.position.y = j;
                    if (AddItemAtPosition(item)) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes a certain amount of items from a given slot
        /// </summary>
        /// <param name="position">The slot to remove from</param>
        /// <param name="amount">Amount to remove</param>
        public override void RemoveAmountFromPosition(Vector2Int position, int amount)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].position != position &&
                    !Collide(Items[i].position, Items[i].itemType.Size, position, Vector2Int.one)) continue;

                var item = Items[i];
                item.amount -= amount;
                
                if (item.amount <= 0)
                {
                    Items.RemoveAt(i);
                    OnOnItemRemoved(item);
                    return;
                }
                
                OnOnItemChanged(item);
                return;
            }
        }
        
        /// <summary>
        /// Removes the item at given location
        /// </summary>
        /// <param name="position">Location to remove from</param>
        /// <returns>Item removed, null if none was present</returns>
        public override ItemStack<T> RemoveItem(Vector2Int position)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].position != position &&
                    !Collide(Items[i].position, Items[i].itemType.Size, position, Vector2Int.one)) continue;

                var item = Items[i];
                Items.RemoveAt(i);
                OnOnItemRemoved(item);
                return item;
            }

            return null;
        }

        /// <summary>
        /// Removes the first item of type not necessarily the first in order of items on the grid
        /// </summary>
        /// <param name="type">The item type to remove</param>
        /// <returns>The item removed, null if none was removed</returns>
        public override ItemStack<T> RemoveItemOfType(ItemType type)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].itemType != type) continue;
                var item = Items[i];
                Items.RemoveAt(i);
                OnOnItemRemoved(item);
                return item;
            }

            return null;
        }

        /// <summary>
        /// Replaces the item at the given strict position (top left) with the given item
        /// </summary>
        /// <param name="replaceItemFromThisPosition">The position to replace</param>
        /// <param name="replaceWith">The item to replace with</param>
        public override void ReplaceItem(Vector2Int replaceItemFromThisPosition, ItemStack<T> replaceWith)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var original = Items[i];
                if (original.position == replaceItemFromThisPosition && CanReplace(replaceWith, replaceItemFromThisPosition))
                {
                    OnOnItemReplaced(original, replaceWith);
                    Items[i] = replaceWith;
                }
            }
        }

        /// <summary>
        /// Gets the item that covers the given position, does not need to be the strict position (top left)
        /// </summary>
        /// <param name="position">The given position</param>
        /// <returns>The item at position</returns>
        public override ItemStack<T> GetItemAtPosition(Vector2Int position)
        {
            return Items.FirstOrDefault(x => Collide(x.position, x.itemType.Size, position, Vector2Int.one));
        }

        /// <summary>
        /// Checks if the given item collides with anything besides the one at the given position
        /// </summary>
        /// <param name="item">The item to check collisions for</param>
        /// <param name="ignore">The position to ignore when checking</param>
        /// <returns>True if no other collisions are found</returns>
        private bool CanReplace(ItemStack<T> item, Vector2Int ignore)
        {
            return !Items.Any(x =>
            {
                if (x.position == ignore) return false;
                return Collide(x.position, x.itemType.Size, item.position, item.itemType.Size);
            });
        }

        private bool CanAddItemToSlot(ItemStack<T> item)
        {
            return !Items.Any(x => Collide(x.position, x.itemType.Size, item.position, item.itemType.Size));
        }

        private static bool Collide(
            Vector2Int rect1TopLeft, Vector2Int rect1Size,
            Vector2Int rect2TopLeft, Vector2Int rect2Size)
        {
            if (rect2TopLeft.x >= rect1TopLeft.x && rect2TopLeft.x < rect1TopLeft.x + rect1Size.x &&
                rect2TopLeft.y >= rect1TopLeft.y && rect2TopLeft.y < rect1TopLeft.y + rect1Size.y)
            {
                return true;
            }


            if (rect1TopLeft.x >= rect2TopLeft.x && rect1TopLeft.x < rect2TopLeft.x + rect2Size.x &&
                rect1TopLeft.y >= rect2TopLeft.y && rect1TopLeft.y < rect2TopLeft.y + rect2Size.y)
            {
                return true;
            }

            return false;
        }
    }
}