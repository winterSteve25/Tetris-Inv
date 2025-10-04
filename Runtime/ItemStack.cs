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
        public Dictionary<string, object> Data;
        
        public ItemStack(T itemType, int amount, Vector2Int position, int inventoryIndex = -1, Dictionary<string, object> data = null)
        {
            this.itemType = itemType;
            this.amount = amount;
            this.position = position;
            this.inventoryIndex = inventoryIndex;
            Data = data;
        }

        public static ItemStack<T> CopyNewAmount(ItemStack<T> other, int amount)
        {
            return new ItemStack<T>(other.itemType, amount, other.position, other.inventoryIndex, other.Data);
        }

        public ItemStack<T> CopyNewAmount(int amount)
        {
            return CopyNewAmount(this, amount);
        }

        public bool TryGetData<TD>(string key, out TD data)
        {
            if (Data.TryGetValue(key, out var d))
            {
                data = (TD) d;
                return true;
            }

            data = default;
            return false;
        }

        public TD GetData<TD>(string key)
        {
            return (TD)Data[key];
        }

        public void SetData<TD>(string key, TD value)
        {
            Data[key] = value;
        }

        public override string ToString()
        {
            return $"{itemType.name} x{amount} at {position} in {inventoryIndex}";
        }
    }
}