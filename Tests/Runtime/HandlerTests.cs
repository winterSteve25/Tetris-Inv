using NUnit.Framework;
using UnityEngine;
using TetrisInv.Runtime;

namespace TetrisInv.Tests.Runtime
{
    public class TetrisInventoryHandlerTests
    {
        private TetrisInventoryHandler<TestItemType> _handler;
        private TetrisInventory<TestItemType> _inventory1;
        private TetrisInventory<TestItemType> _inventory2;
        private TestItemType _testItemType1X1;
        private TestItemType _testItemType2X2;
        private TestItemType _testItemType1X2;

        [SetUp]
        public void Setup()
        {
            _handler = new TetrisInventoryHandler<TestItemType>();
            _inventory1 = new TetrisInventory<TestItemType>(5, 5);
            _inventory2 = new TetrisInventory<TestItemType>(3, 3);

            // Create test item types
            _testItemType1X1 = ScriptableObject.CreateInstance<TestItemType>();
            _testItemType1X1.Initialize("TestItem1x1", 10, new Vector2Int(1, 1));

            _testItemType2X2 = ScriptableObject.CreateInstance<TestItemType>();
            _testItemType2X2.Initialize("TestItem2x2", 5, new Vector2Int(2, 2));

            _testItemType1X2 = ScriptableObject.CreateInstance<TestItemType>();
            _testItemType1X2.Initialize("TestItem1x2", 8, new Vector2Int(1, 2));

            _handler.AddInventory(_inventory1);
            _handler.AddInventory(_inventory2);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up ScriptableObjects
            if (_testItemType1X1 != null) Object.DestroyImmediate(_testItemType1X1);
            if (_testItemType2X2 != null) Object.DestroyImmediate(_testItemType2X2);
            if (_testItemType1X2 != null) Object.DestroyImmediate(_testItemType1X2);
        }

        #region AddInventory Tests

        [Test]
        public void AddInventory_AddsInventoryToCollection()
        {
            var newHandler = new TetrisInventoryHandler<TestItemType>();
            var newInventory = new TetrisInventory<TestItemType>(4, 4);

            newHandler.AddInventory(newInventory);

            // Verify by trying to add an item to index 0
            var item = new ItemStack<TestItemType>(_testItemType1X1, 1, new Vector2Int(0, 0), 0);
            bool result = newHandler.AddItemAtPosition(item);

            Assert.IsTrue(result);
        }

        #endregion

        #region AddItemAtPosition Tests

        [Test]
        public void AddItemAtPosition_ValidIndex_AddsToSpecificInventory()
        {
            var item = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0), 0);

            bool result = _handler.AddItemAtPosition(item);

            Assert.IsTrue(result);
            Assert.AreEqual(1, _inventory1.Items.Count);
            Assert.AreEqual(0, _inventory2.Items.Count);
        }

        [Test]
        public void AddItemAtPosition_InvalidIndex_ThrowsException()
        {
            var item = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0), 5);

            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                _handler.AddItemAtPosition(item));
        }

        [Test]
        public void AddItemAtPosition_ItemCannotBeAdded_ReturnsFalse()
        {
            // Fill the position first
            var blockingItem = new ItemStack<TestItemType>(_testItemType2X2, 1, new Vector2Int(0, 0), 0);
            _handler.AddItemAtPosition(blockingItem);

            var item = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0), 0);

            bool result = _handler.AddItemAtPosition(item);

            Assert.IsFalse(result);
        }

        #endregion

        #region GetItemOfType Tests

        [Test]
        public void GetItemOfType_SpecificInventory_ReturnsItemFromThatInventory()
        {
            var item1 = new ItemStack<TestItemType>(_testItemType1X1, 3, new Vector2Int(0, 0));
            var item2 = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0));

            item1.inventoryIndex = 0;
            item2.inventoryIndex = 1;

            _handler.AddItemAtPosition(item1);
            _handler.AddItemAtPosition(item2);

            var foundItem = _handler.GetItemOfType(_testItemType1X1, 1);

            Assert.AreEqual(item2, foundItem);
            Assert.AreEqual(5, foundItem.amount);
        }

        [Test]
        public void GetItemOfType_AllInventories_ReturnsFirstFound()
        {
            var item1 = new ItemStack<TestItemType>(_testItemType1X1, 3, new Vector2Int(0, 0));
            var item2 = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0));

            item1.inventoryIndex = 0;
            item2.inventoryIndex = 1;

            _handler.AddItemAtPosition(item1);
            _handler.AddItemAtPosition(item2);

            var foundItem = _handler.GetItemOfType(_testItemType1X1, -1);

            Assert.AreEqual(item1, foundItem);
            Assert.AreEqual(3, foundItem.amount);
        }

        [Test]
        public void GetItemOfType_ItemNotFound_ReturnsNull()
        {
            var foundItem = _handler.GetItemOfType(_testItemType1X1, -1);

            Assert.IsNull(foundItem);
        }

        [Test]
        public void GetItemOfType_InvalidIndex_ThrowsException()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                _handler.GetItemOfType(_testItemType1X1, 5));
        }

        #endregion

        #region AddAnywhere Tests

        [Test]
        public void AddAnywhere_SpecificInventorySuccess_ReturnsTrue()
        {
            var item = new ItemStack<TestItemType>(_testItemType1X1, 5, Vector2Int.zero, 0);

            bool result = _handler.AddAnywhere(item);

            Assert.IsTrue(result);
            Assert.AreEqual(1, _inventory1.Items.Count);
            Assert.AreEqual(0, _inventory2.Items.Count);
        }

        [Test]
        public void AddAnywhere_SpecificInventoryFull_ReturnsFalseAndTriggersOverflow()
        {
            // Fill inventory1 completely
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    var fillItem = new ItemStack<TestItemType>(_testItemType1X1, _testItemType1X1.StackSize,
                        new Vector2Int(x, y), 0);
                    _inventory1.AddItemAtPosition(fillItem);
                }
            }

            var item = new ItemStack<TestItemType>(_testItemType1X1, 1, Vector2Int.zero, 0);
            bool overflowTriggered = false;

            _handler.OnItemOverflow += (overflowItem) => overflowTriggered = true;

            bool result = _handler.AddAnywhere(item);

            Assert.IsFalse(result);
            Assert.IsTrue(overflowTriggered);
        }

        [Test]
        public void AddAnywhere_AllInventories_AddsToFirstAvailableAndSetsIndex()
        {
            // Fill inventory1 partially to make it less attractive
            var blockItem = new ItemStack<TestItemType>(_testItemType2X2, 1, new Vector2Int(0, 0), 0);
            _handler.AddItemAtPosition(blockItem);

            var item = new ItemStack<TestItemType>(_testItemType1X1, 5, Vector2Int.zero, -1);

            bool result = _handler.AddAnywhere(item);

            Assert.IsTrue(result);
            Assert.AreEqual(0, item.inventoryIndex); // Should be set to the inventory it was added to
            // Should be added to inventory1 at first available position
            Assert.AreEqual(2, _inventory1.Items.Count);
        }

        [Test]
        public void AddAnywhere_FirstInventoryFull_AddsToSecondAndSetsIndex()
        {
            // Fill inventory1 completely
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    var fillItem = new ItemStack<TestItemType>(_testItemType1X1, _testItemType1X1.StackSize,
                        new Vector2Int(x, y), 0);
                    _inventory1.AddItemAtPosition(fillItem);
                }
            }

            var item = new ItemStack<TestItemType>(_testItemType1X1, 1, Vector2Int.zero, -1);

            bool result = _handler.AddAnywhere(item);

            Assert.IsTrue(result);
            Assert.AreEqual(1, item.inventoryIndex); // Should be set to inventory2
            Assert.AreEqual(1, _inventory2.Items.Count);
        }

        [Test]
        public void AddAnywhere_AllInventoriesFull_ReturnsFalseAndTriggersOverflow()
        {
            // Fill both inventories
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    var fillItem1 = new ItemStack<TestItemType>(_testItemType1X1, _testItemType1X1.StackSize,
                        new Vector2Int(x, y), 0);
                    _inventory1.AddItemAtPosition(fillItem1);
                }
            }

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    var fillItem2 = new ItemStack<TestItemType>(_testItemType1X1, _testItemType1X1.StackSize,
                        new Vector2Int(x, y), 1);
                    _inventory2.AddItemAtPosition(fillItem2);
                }
            }

            var item = new ItemStack<TestItemType>(_testItemType1X1, 1, Vector2Int.zero, -1);
            bool overflowTriggered = false;

            _handler.OnItemOverflow += (overflowItem) => overflowTriggered = true;

            bool result = _handler.AddAnywhere(item);

            Assert.IsFalse(result);
            Assert.IsTrue(overflowTriggered);
            Assert.AreEqual(-1, item.inventoryIndex); // Should remain -1 if not added
        }

        [Test]
        public void AddAnywhere_InvalidIndex_ThrowsException()
        {
            var item = new ItemStack<TestItemType>(_testItemType1X1, 5, Vector2Int.zero, 5);

            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                _handler.AddAnywhere(item));
        }

        #endregion

        #region RemoveItemOfType Tests

        [Test]
        public void RemoveItemOfType_SpecificInventory_RemovesFromThatInventory()
        {
            var item1 = new ItemStack<TestItemType>(_testItemType1X1, 3, new Vector2Int(0, 0));
            var item2 = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0));

            item1.inventoryIndex = 0;
            item2.inventoryIndex = 1;

            _handler.AddItemAtPosition(item1);
            _handler.AddItemAtPosition(item2);

            var removedItem = _handler.RemoveItemOfType(_testItemType1X1, 1);

            Assert.AreEqual(item2, removedItem);
            Assert.AreEqual(1, _inventory1.Items.Count);
            Assert.AreEqual(0, _inventory2.Items.Count);
        }

        [Test]
        public void RemoveItemOfType_AllInventories_RemovesFromFirstFound()
        {
            var item1 = new ItemStack<TestItemType>(_testItemType1X1, 3, new Vector2Int(0, 0));
            var item2 = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0));

            item1.inventoryIndex = 0;
            item2.inventoryIndex = 1;

            _handler.AddItemAtPosition(item1);
            _handler.AddItemAtPosition(item2);

            var removedItem = _handler.RemoveItemOfType(_testItemType1X1, -1);

            Assert.AreEqual(item1, removedItem);
            Assert.AreEqual(0, _inventory1.Items.Count);
            Assert.AreEqual(1, _inventory2.Items.Count);
        }

        [Test]
        public void RemoveItemOfType_ItemNotFound_ReturnsNull()
        {
            var removedItem = _handler.RemoveItemOfType(_testItemType1X1, -1);

            Assert.IsNull(removedItem);
        }

        [Test]
        public void RemoveItemOfType_InvalidIndex_ThrowsException()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                _handler.RemoveItemOfType(_testItemType1X1, 5));
        }

        #endregion

        #region RemoveAmountFromPosition Tests

        [Test]
        public void RemoveAmountFromPosition_ValidParameters_RemovesAmount()
        {
            var item = new ItemStack<TestItemType>(_testItemType1X1, 8, new Vector2Int(0, 0));
            item.inventoryIndex = 0;

            _handler.AddItemAtPosition(item);
            _handler.RemoveAmountFromPosition(0, new Vector2Int(0, 0), 3);

            Assert.AreEqual(1, _inventory1.Items.Count);
            Assert.AreEqual(5, _inventory1.Items[0].amount);
        }

        [Test]
        public void RemoveAmountFromPosition_RemoveAll_RemovesItem()
        {
            var item = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0));
            item.inventoryIndex = 0;

            _handler.AddItemAtPosition(item);
            _handler.RemoveAmountFromPosition(0, new Vector2Int(0, 0), 7);

            Assert.AreEqual(0, _inventory1.Items.Count);
        }

        [Test]
        public void RemoveAmountFromPosition_InvalidIndex_ThrowsException()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                _handler.RemoveAmountFromPosition(5, new Vector2Int(0, 0), 1));
        }

        #endregion

        #region RemoveItem Tests

        [Test]
        public void RemoveItem_ItemExists_RemovesAndReturnsItem()
        {
            var item = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0));
            item.inventoryIndex = 0;

            _handler.AddItemAtPosition(item);
            var removedItem = _handler.RemoveItem(0, new Vector2Int(0, 0));

            Assert.AreEqual(item, removedItem);
            Assert.AreEqual(0, _inventory1.Items.Count);
        }

        [Test]
        public void RemoveItem_NoItemExists_ReturnsNull()
        {
            var removedItem = _handler.RemoveItem(0, new Vector2Int(0, 0));

            Assert.IsNull(removedItem);
        }

        [Test]
        public void RemoveItem_InvalidIndex_ThrowsException()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                _handler.RemoveItem(5, new Vector2Int(0, 0)));
        }

        #endregion

        #region ReplaceItem Tests

        [Test]
        public void ReplaceItem_ValidParameters_ReplacesItem()
        {
            var originalItem = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0));
            originalItem.inventoryIndex = 0;
            var replacementItem = new ItemStack<TestItemType>(_testItemType1X1, 3, new Vector2Int(0, 0));

            _handler.AddItemAtPosition(originalItem);
            _handler.ReplaceItem(0, new Vector2Int(0, 0), replacementItem);

            Assert.AreEqual(1, _inventory1.Items.Count);
            Assert.AreEqual(replacementItem, _inventory1.Items[0]);
        }

        [Test]
        public void ReplaceItem_InvalidIndex_ThrowsException()
        {
            var replacementItem = new ItemStack<TestItemType>(_testItemType1X1, 3, new Vector2Int(0, 0));

            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                _handler.ReplaceItem(5, new Vector2Int(0, 0), replacementItem));
        }

        #endregion

        #region GetItemAtPosition Tests

        [Test]
        public void GetItemAtPosition_ItemExists_ReturnsItem()
        {
            var item = new ItemStack<TestItemType>(_testItemType2X2, 1, new Vector2Int(1, 1));
            item.inventoryIndex = 0;

            _handler.AddItemAtPosition(item);
            var foundItem = _handler.GetItemAtPosition(0, new Vector2Int(1, 1));
            var foundItemCollision = _handler.GetItemAtPosition(0, new Vector2Int(2, 2));

            Assert.AreEqual(item, foundItem);
            Assert.AreEqual(item, foundItemCollision);
        }

        [Test]
        public void GetItemAtPosition_NoItemExists_ReturnsNull()
        {
            var foundItem = _handler.GetItemAtPosition(0, new Vector2Int(0, 0));

            Assert.IsNull(foundItem);
        }

        [Test]
        public void GetItemAtPosition_InvalidIndex_ThrowsException()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                _handler.GetItemAtPosition(5, new Vector2Int(0, 0)));
        }

        #endregion

        #region Event Tests

        [Test]
        public void OnItemOverflow_TriggeredWhenExpected()
        {
            ItemStack<TestItemType> overflowItem = null;
            bool overflowTriggered = false;

            _handler.OnItemOverflow += (item) =>
            {
                overflowItem = item;
                overflowTriggered = true;
            };

            // Fill inventory1
            foreach (var inv in _handler.Inventories)
            {
                for (int y = 0; y < inv.Height; y++)
                {
                    for (int x = 0; x < inv.Width; x++)
                    {
                        var fillItem = new ItemStack<TestItemType>(_testItemType1X1, _testItemType1X1.StackSize,
                            new Vector2Int(x, y));
                        inv.AddItemAtPosition(fillItem);
                    }
                }
            }

            var testItem = new ItemStack<TestItemType>(_testItemType1X1, 1, Vector2Int.zero);
            _handler.AddAnywhere(testItem);

            Assert.IsTrue(overflowTriggered);
            Assert.AreEqual(testItem, overflowItem);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void MultipleInventories_WorkIndependently()
        {
            var item1 = new ItemStack<TestItemType>(_testItemType1X1, 3, new Vector2Int(0, 0), 0);
            var item2 = new ItemStack<TestItemType>(_testItemType2X2, 1, new Vector2Int(0, 0), 1);

            _handler.AddItemAtPosition(item1);
            _handler.AddItemAtPosition(item2);

            Assert.AreEqual(1, _inventory1.Items.Count);
            Assert.AreEqual(1, _inventory2.Items.Count);
            Assert.AreEqual(_testItemType1X1, _inventory1.Items[0].itemType);
            Assert.AreEqual(_testItemType2X2, _inventory2.Items[0].itemType);
        }

        [Test]
        public void SearchOperations_CheckCorrectOrder()
        {
            // Add different items to different inventories
            var item1 = new ItemStack<TestItemType>(_testItemType1X1, 3, new Vector2Int(0, 0), 0);
            var item2 = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0), 1);

            _handler.AddItemAtPosition(item2); // Add to second inventory first
            _handler.AddItemAtPosition(item1); // Add to first inventory second

            // Search should return from first inventory (index 0) first
            var foundItem = _handler.GetItemOfType(_testItemType1X1, -1);
            Assert.AreEqual(item1, foundItem);

            // Remove should also remove from first inventory first
            var removedItem = _handler.RemoveItemOfType(_testItemType1X1, -1);
            Assert.AreEqual(item1, removedItem);
            Assert.AreEqual(0, _inventory1.Items.Count);
            Assert.AreEqual(1, _inventory2.Items.Count);
        }

        #endregion
    }
}