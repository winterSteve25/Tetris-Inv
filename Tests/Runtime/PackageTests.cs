using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using TetrisInv.Runtime;

namespace TetrisInv.Tests
{
    [TestFixture]
    public class LinearInventoryTests
    {
        private LinearInventory<TestItemType> inventory;
        private TestItemType testItem;
        private List<ItemStack<TestItemType>> addedItems;
        private List<ItemStack<TestItemType>> changedItems;
        private List<ItemStack<TestItemType>> removedItems;

        [SetUp]
        public void Setup()
        {
            inventory = new LinearInventory<TestItemType>(10);
            testItem = ScriptableObject.CreateInstance<TestItemType>();
            testItem.SetStackSize(64);
            testItem.SetSize(Vector2Int.one);
            
            addedItems = new List<ItemStack<TestItemType>>();
            changedItems = new List<ItemStack<TestItemType>>();
            removedItems = new List<ItemStack<TestItemType>>();

            inventory.OnItemAdded += item => addedItems.Add(item);
            inventory.OnItemChanged += item => changedItems.Add(item);
            inventory.OnItemRemoved += item => removedItems.Add(item);
        }

        [Test]
        public void AddItemAtPosition_EmptySlot_AddsSuccessfully()
        {
            var item = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero);
            var result = inventory.AddItemAtPosition(item);

            Assert.IsTrue(result);
            Assert.AreEqual(1, addedItems.Count);
            Assert.AreEqual(10, inventory.GetItemAtPosition(Vector2Int.zero).amount);
        }

        [Test]
        public void AddItemAtPosition_ExceedsStackSize_ReturnsFalseWithLeftover()
        {
            var item = new ItemStack<TestItemType>(testItem, 100, Vector2Int.zero);
            var result = inventory.AddItemAtPosition(item);

            Assert.IsFalse(result);
            Assert.AreEqual(64, inventory.GetItemAtPosition(Vector2Int.zero).amount);
            Assert.AreEqual(36, item.amount);
            Assert.AreEqual(1, addedItems.Count);
        }

        [Test]
        public void AddItemAtPosition_StackWithSameType_CombinesSuccessfully()
        {
            var item1 = new ItemStack<TestItemType>(testItem, 30, Vector2Int.zero);
            inventory.AddItemAtPosition(item1);
            
            var item2 = new ItemStack<TestItemType>(testItem, 20, Vector2Int.zero);
            var result = inventory.AddItemAtPosition(item2);

            Assert.IsTrue(result);
            Assert.AreEqual(50, inventory.GetItemAtPosition(Vector2Int.zero).amount);
            Assert.AreEqual(1, addedItems.Count);
            Assert.AreEqual(1, changedItems.Count);
        }

        [Test]
        public void AddItemAtPosition_StackWithDifferentType_ReturnsFalse()
        {
            var testItem2 = ScriptableObject.CreateInstance<TestItemType>();
            testItem2.SetStackSize(64);

            var item1 = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero);
            inventory.AddItemAtPosition(item1);
            
            var item2 = new ItemStack<TestItemType>(testItem2, 10, Vector2Int.zero);
            var result = inventory.AddItemAtPosition(item2);

            Assert.IsFalse(result);
            Assert.AreEqual(10, inventory.GetItemAtPosition(Vector2Int.zero).amount);
        }

        [Test]
        public void AddAnywhere_FindsEmptySlot_AddsSuccessfully()
        {
            var item = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero);
            var result = inventory.AddAnywhere(item);

            Assert.IsTrue(result);
            Assert.AreEqual(10, inventory.GetItemAtPosition(Vector2Int.zero).amount);
        }

        [Test]
        public void AddAnywhere_FillsMultipleSlots()
        {
            var item1 = new ItemStack<TestItemType>(testItem, 150, Vector2Int.zero);
            inventory.AddAnywhere(item1);

            Assert.AreEqual(64, inventory.GetItemAtPosition(new Vector2Int(0, 0)).amount);
            Assert.AreEqual(64, inventory.GetItemAtPosition(new Vector2Int(1, 0)).amount);
            Assert.AreEqual(22, inventory.GetItemAtPosition(new Vector2Int(2, 0)).amount);
            Assert.AreEqual(3, addedItems.Count);
        }

        [Test]
        public void GetItemOfType_ReturnsCorrectItem()
        {
            var item = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero);
            inventory.AddItemAtPosition(item);

            var found = inventory.GetItemOfType(testItem);
            Assert.IsNotNull(found);
            Assert.AreEqual(testItem, found.itemType);
        }

        [Test]
        public void GetItemOfType_ItemNotFound_ReturnsNull()
        {
            var testItem2 = ScriptableObject.CreateInstance<TestItemType>();
            var found = inventory.GetItemOfType(testItem2);
            Assert.IsNull(found);
        }

        [Test]
        public void RemoveAmountFromPosition_PartialRemoval_UpdatesAmount()
        {
            var item = new ItemStack<TestItemType>(testItem, 30, Vector2Int.zero);
            inventory.AddItemAtPosition(item);
            
            inventory.RemoveAmountFromPosition(Vector2Int.zero, 10);

            Assert.AreEqual(20, inventory.GetItemAtPosition(Vector2Int.zero).amount);
            Assert.AreEqual(1, changedItems.Count);
        }

        [Test]
        public void RemoveAmountFromPosition_CompleteRemoval_RemovesItem()
        {
            var item = new ItemStack<TestItemType>(testItem, 30, Vector2Int.zero);
            inventory.AddItemAtPosition(item);
            
            inventory.RemoveAmountFromPosition(Vector2Int.zero, 30);

            Assert.IsNull(inventory.GetItemAtPosition(Vector2Int.zero));
            Assert.AreEqual(1, removedItems.Count);
        }

        [Test]
        public void RemoveItem_ReturnsAndRemovesItem()
        {
            var item = new ItemStack<TestItemType>(testItem, 25, Vector2Int.zero);
            inventory.AddItemAtPosition(item);
            
            var removed = inventory.RemoveItem(Vector2Int.zero);

            Assert.IsNotNull(removed);
            Assert.AreEqual(25, removed.amount);
            Assert.IsNull(inventory.GetItemAtPosition(Vector2Int.zero));
            Assert.AreEqual(1, removedItems.Count);
        }

        [Test]
        public void RemoveItemOfType_ReturnsCorrectItem()
        {
            var item = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero);
            inventory.AddItemAtPosition(item);
            
            var removed = inventory.RemoveItemOfType(testItem);

            Assert.IsNotNull(removed);
            Assert.AreEqual(testItem, removed.itemType);
            Assert.IsNull(inventory.GetItemAtPosition(Vector2Int.zero));
        }

        [Test]
        public void ReplaceItem_ReplacesExistingItem()
        {
            var item1 = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero);
            inventory.AddItemAtPosition(item1);
            
            var testItem2 = ScriptableObject.CreateInstance<TestItemType>();
            testItem2.SetStackSize(64);
            var item2 = new ItemStack<TestItemType>(testItem2, 5, Vector2Int.zero);
            
            inventory.ReplaceItem(Vector2Int.zero, item2);

            var replaced = inventory.GetItemAtPosition(Vector2Int.zero);
            Assert.AreEqual(testItem2, replaced.itemType);
            Assert.AreEqual(5, replaced.amount);
        }
    }

    [TestFixture]
    public class TetrisInventoryTests
    {
        private TetrisInventory<TestItemType> inventory;
        private TestItemType testItem;
        private TestItemType largeItem;
        private List<ItemStack<TestItemType>> addedItems;
        private List<ItemStack<TestItemType>> changedItems;
        private List<ItemStack<TestItemType>> removedItems;

        [SetUp]
        public void Setup()
        {
            inventory = new TetrisInventory<TestItemType>(5, 5);
            
            testItem = ScriptableObject.CreateInstance<TestItemType>();
            testItem.SetStackSize(64);
            testItem.SetSize(Vector2Int.one);
            
            largeItem = ScriptableObject.CreateInstance<TestItemType>();
            largeItem.SetStackSize(1);
            largeItem.SetSize(new Vector2Int(2, 2));

            addedItems = new List<ItemStack<TestItemType>>();
            changedItems = new List<ItemStack<TestItemType>>();
            removedItems = new List<ItemStack<TestItemType>>();

            inventory.OnItemAdded += item => addedItems.Add(item);
            inventory.OnItemChanged += item => changedItems.Add(item);
            inventory.OnItemRemoved += item => removedItems.Add(item);
        }

        [Test]
        public void AddItemAtPosition_ValidPosition_AddsSuccessfully()
        {
            var item = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero);
            var result = inventory.AddItemAtPosition(item);

            Assert.IsTrue(result);
            Assert.AreEqual(1, addedItems.Count);
            Assert.AreEqual(testItem, inventory.GetItemAtPosition(Vector2Int.zero).itemType);
        }

        [Test]
        public void AddItemAtPosition_CollisionDetected_ReturnsFalse()
        {
            var item1 = new ItemStack<TestItemType>(largeItem, 1, Vector2Int.zero);
            inventory.AddItemAtPosition(item1);
            
            var item2 = new ItemStack<TestItemType>(testItem, 10, new Vector2Int(1, 1));
            var result = inventory.AddItemAtPosition(item2);

            Assert.IsFalse(result);
            Assert.AreEqual(1, addedItems.Count);
        }

        [Test]
        public void AddItemAtPosition_ExceedsStackSize_ReturnsLeftover()
        {
            var item = new ItemStack<TestItemType>(testItem, 100, Vector2Int.zero);
            var result = inventory.AddItemAtPosition(item);

            Assert.IsFalse(result);
            Assert.AreEqual(64, inventory.GetItemAtPosition(Vector2Int.zero).amount);
            Assert.AreEqual(36, item.amount);
        }

        [Test]
        public void AddAnywhere_FindsAvailableSpace_AddsSuccessfully()
        {
            var item = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero);
            var result = inventory.AddAnywhere(item);

            Assert.IsTrue(result);
            Assert.IsNotNull(inventory.GetItemAtPosition(item.position));
        }

        [Test]
        public void AddAnywhere_FollowsSearchOrder_TopLeftToBottomRight()
        {
            var item1 = new ItemStack<TestItemType>(testItem, 64, Vector2Int.zero);
            inventory.AddAnywhere(item1);
            
            var item2 = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero);
            inventory.AddAnywhere(item2);

            Assert.IsNotNull(inventory.GetItemAtPosition(new Vector2Int(1, 0)));
        }

        [Test]
        public void GetItemOfType_ReturnsCorrectItem()
        {
            var item = new ItemStack<TestItemType>(testItem, 10, new Vector2Int(2, 3));
            inventory.AddItemAtPosition(item);

            var found = inventory.GetItemOfType(testItem);
            Assert.IsNotNull(found);
            Assert.AreEqual(testItem, found.itemType);
        }

        [Test]
        public void GetItemAtPosition_DetectsItemByAnyPartOfSize()
        {
            var item = new ItemStack<TestItemType>(largeItem, 1, Vector2Int.zero);
            inventory.AddItemAtPosition(item);

            var found = inventory.GetItemAtPosition(new Vector2Int(1, 1));
            Assert.IsNotNull(found);
            Assert.AreEqual(largeItem, found.itemType);
        }

        [Test]
        public void RemoveAmountFromPosition_PartialRemoval_UpdatesAmount()
        {
            var item = new ItemStack<TestItemType>(testItem, 30, Vector2Int.zero);
            inventory.AddItemAtPosition(item);
            
            inventory.RemoveAmountFromPosition(Vector2Int.zero, 10);

            Assert.AreEqual(20, inventory.GetItemAtPosition(Vector2Int.zero).amount);
            Assert.AreEqual(1, changedItems.Count);
        }

        [Test]
        public void RemoveAmountFromPosition_CompleteRemoval_RemovesItem()
        {
            var item = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero);
            inventory.AddItemAtPosition(item);
            
            inventory.RemoveAmountFromPosition(Vector2Int.zero, 10);

            Assert.IsNull(inventory.GetItemAtPosition(Vector2Int.zero));
            Assert.AreEqual(1, removedItems.Count);
        }

        [Test]
        public void RemoveItem_RemovesItemAtPosition()
        {
            var item = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero);
            inventory.AddItemAtPosition(item);
            
            var removed = inventory.RemoveItem(Vector2Int.zero);

            Assert.IsNotNull(removed);
            Assert.IsNull(inventory.GetItemAtPosition(Vector2Int.zero));
            Assert.AreEqual(1, removedItems.Count);
        }

        [Test]
        public void RemoveItemOfType_RemovesFirstItemOfType()
        {
            var item1 = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero);
            var item2 = new ItemStack<TestItemType>(testItem, 10, new Vector2Int(1, 0));
            inventory.AddItemAtPosition(item1);
            inventory.AddItemAtPosition(item2);
            
            var removed = inventory.RemoveItemOfType(testItem);

            Assert.IsNotNull(removed);
            Assert.AreEqual(1, removedItems.Count);
        }

        [Test]
        public void ReplaceItem_ReplacesAtExactPosition()
        {
            var item1 = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero);
            inventory.AddItemAtPosition(item1);
            
            var testItem2 = ScriptableObject.CreateInstance<TestItemType>();
            testItem2.SetStackSize(64);
            testItem2.SetSize(Vector2Int.one);
            var item2 = new ItemStack<TestItemType>(testItem2, 5, Vector2Int.zero);
            
            inventory.ReplaceItem(Vector2Int.zero, item2);

            var replaced = inventory.GetItemAtPosition(Vector2Int.zero);
            Assert.AreEqual(testItem2, replaced.itemType);
        }

        [Test]
        public void ReplaceItem_CannotReplaceIfCollision()
        {
            var item1 = new ItemStack<TestItemType>(largeItem, 1, new Vector2Int(1, 0));
            var item2 = new ItemStack<TestItemType>(testItem, 10, new Vector2Int(0, 0));
            Assert.IsTrue(inventory.AddItemAtPosition(item1));
            Assert.IsTrue(inventory.AddItemAtPosition(item2));
            
            var largeItem2 = ScriptableObject.CreateInstance<TestItemType>();
            largeItem2.SetStackSize(1);
            largeItem2.SetSize(new Vector2Int(2, 2));
            var item3 = new ItemStack<TestItemType>(largeItem2, 1, Vector2Int.zero);
            
            inventory.ReplaceItem(Vector2Int.zero, item3);

            var item = inventory.GetItemAtPosition(Vector2Int.zero);
            Assert.AreEqual(testItem, item.itemType);
        }
    }

    [TestFixture]
    public class InventoryHandlerTests
    {
        private InventoryHandler<TestItemType> handler;
        private LinearInventory<TestItemType> linearInv;
        private TetrisInventory<TestItemType> tetrisInv;
        private TestItemType testItem;
        private List<ItemStack<TestItemType>> overflowItems;

        [SetUp]
        public void Setup()
        {
            handler = new InventoryHandler<TestItemType>();
            linearInv = new LinearInventory<TestItemType>(5);
            tetrisInv = new TetrisInventory<TestItemType>(3, 3);
            
            handler.AddInventory(linearInv);
            handler.AddInventory(tetrisInv);
            
            testItem = ScriptableObject.CreateInstance<TestItemType>();
            testItem.SetStackSize(64);
            testItem.SetSize(Vector2Int.one);
            
            overflowItems = new List<ItemStack<TestItemType>>();
            handler.OnItemOverflow += item => overflowItems.Add(item);
        }

        [Test]
        public void AddInventory_AddsMultipleInventories()
        {
            var newInv = new LinearInventory<TestItemType>(5);
            handler.AddInventory(newInv);

            Assert.AreEqual(3, handler.Inventories.Count);
        }

        [Test]
        public void AddItemAtPosition_AddsToCorrectInventory()
        {
            var item = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero, 0);
            var result = handler.AddItemAtPosition(item);

            Assert.IsTrue(result);
            Assert.IsNotNull(handler.GetItemAtPosition(0, Vector2Int.zero));
        }

        [Test]
        public void GetItemOfType_SpecificInventory_ReturnsFromThatInventory()
        {
            var item = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero, 0);
            linearInv.AddItemAtPosition(item);

            var found = handler.GetItemOfType(testItem, 0);
            Assert.IsNotNull(found);
        }

        [Test]
        public void GetItemOfType_AllInventories_SearchesAll()
        {
            var item = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero);
            tetrisInv.AddItemAtPosition(item);

            var found = handler.GetItemOfType(testItem, -1);
            Assert.IsNotNull(found);
        }

        [Test]
        public void AddAnywhere_SpecificInventory_AddsToThatInventory()
        {
            var item = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero, 0);
            var result = handler.AddAnywhere(item);

            Assert.IsTrue(result);
            Assert.AreEqual(0, item.inventoryIndex);
        }

        [Test]
        public void AddAnywhere_AllInventories_TriesEachInventory()
        {
            var item = new ItemStack<TestItemType>(testItem, 1000, Vector2Int.zero, -1);
            var result = handler.AddAnywhere(item);

            Assert.IsFalse(result);
            Assert.AreEqual(1, overflowItems.Count);
        }

        [Test]
        public void AddAnywhere_OverflowTriggersEvent()
        {
            var item = new ItemStack<TestItemType>(testItem, 1000, Vector2Int.zero, 0);
            handler.AddAnywhere(item);

            Assert.AreEqual(1, overflowItems.Count);
            Assert.AreEqual(testItem, overflowItems[0].itemType);
        }

        [Test]
        public void RemoveItemOfType_SpecificInventory_RemovesFromThat()
        {
            var item = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero);
            linearInv.AddItemAtPosition(item);

            var removed = handler.RemoveItemOfType(testItem, 0);
            Assert.IsNotNull(removed);
            Assert.IsNull(handler.GetItemAtPosition(0, Vector2Int.zero));
        }

        [Test]
        public void RemoveItemOfType_AllInventories_SearchesAll()
        {
            var item = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero);
            tetrisInv.AddItemAtPosition(item);

            var removed = handler.RemoveItemOfType(testItem, -1);
            Assert.IsNotNull(removed);
        }

        [Test]
        public void RemoveAmountFromPosition_RemovesFromCorrectInventory()
        {
            var item = new ItemStack<TestItemType>(testItem, 30, Vector2Int.zero);
            linearInv.AddItemAtPosition(item);

            handler.RemoveAmountFromPosition(0, Vector2Int.zero, 10);

            Assert.AreEqual(20, handler.GetItemAtPosition(0, Vector2Int.zero).amount);
        }

        [Test]
        public void RemoveItem_RemovesFromCorrectInventory()
        {
            var item = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero);
            linearInv.AddItemAtPosition(item);

            var removed = handler.RemoveItem(0, Vector2Int.zero);
            Assert.IsNotNull(removed);
            Assert.IsNull(handler.GetItemAtPosition(0, Vector2Int.zero));
        }

        [Test]
        public void ReplaceItem_ReplacesInCorrectInventory()
        {
            var item1 = new ItemStack<TestItemType>(testItem, 10, Vector2Int.zero);
            linearInv.AddItemAtPosition(item1);

            var testItem2 = ScriptableObject.CreateInstance<TestItemType>();
            testItem2.SetStackSize(64);
            testItem2.SetSize(Vector2Int.one);
            var item2 = new ItemStack<TestItemType>(testItem2, 5, Vector2Int.zero);
            
            handler.ReplaceItem(0, Vector2Int.zero, item2);

            var replaced = handler.GetItemAtPosition(0, Vector2Int.zero);
            Assert.AreEqual(testItem2, replaced.itemType);
        }
    }

    public class TestItemType : ItemType
    {
        public void SetStackSize(int size)
        {
            var field = typeof(ItemType).GetProperty("StackSize");
            if (field != null)
            {
                field.GetSetMethod(true).Invoke(this, new object[] { size });
            }
        }

        public void SetSize(Vector2Int size)
        {
            var field = typeof(ItemType).GetProperty("Size");
            if (field != null)
            {
                field.GetSetMethod(true).Invoke(this, new object[] { size });
            }
        }
    }
}