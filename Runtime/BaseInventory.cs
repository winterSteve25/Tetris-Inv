using System;
using System.Collections.Generic;
using UnityEngine;

namespace TetrisInv.Runtime
{
    [Serializable]
    public class BaseInventory<T> where T : ItemType
    {
        public event Action<ItemStack<T>> OnItemAdded;
        public event Action<ItemStack<T>> OnItemChanged;
        public event Action<ItemStack<T>> OnItemRemoved;
        public event Action<ItemStack<T>, ItemStack<T>> OnItemReplaced;
        
        public virtual bool AddItemAtPosition(ItemStack<T> item)
        {
            throw new NotImplementedException();
        }

        public virtual ItemStack<T> GetItemOfType(ItemType type)
        {
            throw new NotImplementedException();
        }

        public virtual bool AddAnywhere(ItemStack<T> item)
        {
            throw new NotImplementedException();
        }

        public virtual void RemoveAmountFromPosition(Vector2Int position, int amount)
        {
            throw new NotImplementedException();
        }

        public virtual ItemStack<T> RemoveItem(Vector2Int position)
        {
            throw new NotImplementedException();
        }

        public virtual ItemStack<T> RemoveItemOfType(ItemType type)
        {
            throw new NotImplementedException();
        }

        public virtual ItemStack<T> RemoveItemOfType(ItemType type, int amount)
        {
            throw new NotImplementedException();
        }

        public virtual void ReplaceItem(Vector2Int replaceItemFromThisPosition, ItemStack<T> replaceWith)
        {
            throw new NotImplementedException();
        }

        public virtual ItemStack<T> GetItemAtPosition(Vector2Int position)
        {
            throw new NotImplementedException();
        }

        protected virtual void OnOnItemAdded(ItemStack<T> obj)
        {
            OnItemAdded?.Invoke(obj);
        }

        protected virtual void OnOnItemChanged(ItemStack<T> obj)
        {
            OnItemChanged?.Invoke(obj);
        }

        protected virtual void OnOnItemRemoved(ItemStack<T> obj)
        {
            OnItemRemoved?.Invoke(obj);
        }

        protected virtual void OnOnItemReplaced(ItemStack<T> arg1, ItemStack<T> arg2)
        {
            OnItemReplaced?.Invoke(arg1, arg2);
        }
    }
}