using System;
using UnityEngine;

namespace TetrisInv.Runtime
{
    [Serializable]
    public class ItemStack<T> where T : ItemType
    {
        public T itemType;
        public int amount;
        public Vector2Int position;
        
        public ItemStack(T itemType, int amount, Vector2Int position)
        {
            this.itemType = itemType;
            this.amount = amount;
            this.position = position;
        }

        public override string ToString()
        {
            return $"{itemType.name} x{amount} at {position}";
        }
    }
}