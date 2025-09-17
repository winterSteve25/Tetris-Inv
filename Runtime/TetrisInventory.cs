using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TetrisInv.Runtime
{
    [Serializable]
    public class TetrisInventory<T> : IInventory<T> where T : ItemType
    {
        [Header("Debug Info DO NOT EDIT")]
        [field: SerializeField] public List<ItemStack<T>> Items { get; private set; }
        [field: SerializeField] public int Width { get; private set; }
        [field: SerializeField] public int Height { get; private set; }

        public event Action<ItemStack<T>> OnItemAdded;
        public event Action<ItemStack<T>> OnItemChanged;
        public event Action<ItemStack<T>> OnItemRemoved;
        public event Action<ItemStack<T>, ItemStack<T>> OnItemReplaced;
        public event Action<ItemStack<T>> OnItemOverflow; 

        public TetrisInventory(int width, int height)
        {
            Width = width;
            Height = height;
            Items = new List<ItemStack<T>>();
        }

        public bool AddItemAtPosition(ItemStack<T> item)
        {
            var itemAtPosition = GetItemAtPosition(item.position);
            if (itemAtPosition == null)
            {
                if (!CanAddItemToSlot(item)) return false;
                if (item.amount > item.itemType.StackSize)
                {
                    var itemStack = new ItemStack<T>(item.itemType, item.itemType.StackSize, item.position);
                    Items.Add(itemStack);
                    OnItemAdded?.Invoke(itemStack);
                    item.amount -= item.itemType.StackSize;
                    return false;
                }

                Items.Add(item);
                OnItemAdded?.Invoke(item);
                return true;
            }

            if (itemAtPosition.itemType == item.itemType)
            {
                itemAtPosition.amount += item.amount;
                if (itemAtPosition.amount <= item.itemType.StackSize)
                {
                    OnItemChanged?.Invoke(itemAtPosition);
                    return true;
                }

                var diff = itemAtPosition.amount - item.itemType.StackSize;
                itemAtPosition.amount = item.itemType.StackSize;
                OnItemChanged?.Invoke(itemAtPosition);
                item.amount = diff;
            }

            return false;
        }
        
        public ItemStack<T> GetItemOfType(ItemType type)
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

        public void AddAnywhere(ItemStack<T> item)
        {
            var sameItem = Items.FirstOrDefault(x => x.itemType == item.itemType && x.amount <= item.itemType.StackSize);
            if (sameItem != null && sameItem.itemType != null)
            {
                item.position = sameItem.position;
                if (AddItemAtPosition(item))
                {
                    return;
                }
            }

            for (var j = 0; j < Height; j++)
            {
                for (var i = 0; i < Width; i++)
                {
                    item.position.x = i;
                    item.position.y = j;
                    if (AddItemAtPosition(item)) return;
                }
            }

            OnItemOverflow?.Invoke(item);
        }

        public void RemoveAmountFromPosition(Vector2Int position, int amount)
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
                    OnItemRemoved?.Invoke(item);
                    return;
                }
                
                OnItemChanged?.Invoke(item);
                return;
            }
        }
        
        public ItemStack<T> RemoveItem(Vector2Int position)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].position != position &&
                    !Collide(Items[i].position, Items[i].itemType.Size, position, Vector2Int.one)) continue;

                var item = Items[i];
                Items.RemoveAt(i);
                OnItemRemoved?.Invoke(item);
                return item;
            }

            return null;
        }

        public ItemStack<T> RemoveItemOfType(ItemType type)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].itemType != type) continue;
                var item = Items[i];
                Items.RemoveAt(i);
                OnItemRemoved?.Invoke(item);
                return item;
            }

            return null;
        }

        public void ReplaceItem(Vector2Int replaceItemFromThisPosition, ItemStack<T> replaceWith)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var original = Items[i];
                if (original.position == replaceItemFromThisPosition && CanReplace(replaceWith, replaceItemFromThisPosition))
                {
                    OnItemReplaced?.Invoke(original, replaceWith);
                    Items[i] = replaceWith;
                }
            }
        }

        public ItemStack<T> GetItemAtPosition(Vector2Int position)
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