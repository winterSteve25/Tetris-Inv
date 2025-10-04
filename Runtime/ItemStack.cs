using System;
using System.Collections.Generic;
using UnityEngine;

namespace TetrisInv.Runtime
{
    [Serializable]
    public class ItemStack<T> where T : ItemType
    {
        public T itemType;
        public int amount;
        public Vector2Int position;
        public int inventoryIndex;
        
        public ItemStack(T itemType, int amount, Vector2Int position, int inventoryIndex = -1)
        {
            this.itemType = itemType;
            this.amount = amount;
            this.position = position;
            this.inventoryIndex = inventoryIndex;
        }

        public static ItemStack<T> CopyNewAmount(ItemStack<T> other, int amount)
        {
            return new ItemStack<T>(other.itemType, amount, other.position, other.inventoryIndex);
        }

        public ItemStack<T> CopyNewAmount(int amount)
        {
            return CopyNewAmount(this, amount);
        }

        public override string ToString()
        {
            return $"{itemType.name} x{amount} at {position} in {inventoryIndex}";
        }
    }
}