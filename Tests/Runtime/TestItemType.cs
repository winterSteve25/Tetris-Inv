using TetrisInv.Runtime;
using UnityEngine;

namespace TetrisInv.Tests.Runtime
{
    // Test implementation of ItemType for testing purposes
    public class TestItemType : ItemType
    {
        public void Initialize(string itemName, int stackSize, Vector2Int size)
        {
            name = itemName;
            // Using reflection to set private setters for testing
            var stackSizeProperty = typeof(ItemType).GetProperty("StackSize");
            stackSizeProperty?.SetValue(this, stackSize);

            var sizeProperty = typeof(ItemType).GetProperty("Size");
            sizeProperty?.SetValue(this, size);
        }
    }
}