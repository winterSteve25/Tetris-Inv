using NUnit.Framework;
using TetrisInv.Runtime;
using UnityEngine;

namespace TetrisInv.Tests.Runtime
{
    public class TetrisInventoryTests
    {
        private TetrisInventory<TestItemType> _inventory;
        private TestItemType _testItemType1X1;
        private TestItemType _testItemType2X2;
        private TestItemType _testItemType1X2;
        private TestItemType _largeStackItem;

        [SetUp]
        public void Setup()
        {
            _inventory = new TetrisInventory<TestItemType>(5, 5);

            // Create test item types
            _testItemType1X1 = ScriptableObject.CreateInstance<TestItemType>();
            _testItemType1X1.Initialize("TestItem1x1", 10, new Vector2Int(1, 1));

            _testItemType2X2 = ScriptableObject.CreateInstance<TestItemType>();
            _testItemType2X2.Initialize("TestItem2x2", 5, new Vector2Int(2, 2));

            _testItemType1X2 = ScriptableObject.CreateInstance<TestItemType>();
            _testItemType1X2.Initialize("TestItem1x2", 8, new Vector2Int(1, 2));

            _largeStackItem = ScriptableObject.CreateInstance<TestItemType>();
            _largeStackItem.Initialize("LargeStackItem", 100, new Vector2Int(1, 1));
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up ScriptableObjects
            if (_testItemType1X1 != null) Object.DestroyImmediate(_testItemType1X1);
            if (_testItemType2X2 != null) Object.DestroyImmediate(_testItemType2X2);
            if (_testItemType1X2 != null) Object.DestroyImmediate(_testItemType1X2);
            if (_largeStackItem != null) Object.DestroyImmediate(_largeStackItem);
        }

        #region Constructor Tests

        [Test]
        public void Constructor_InitializesCorrectly()
        {
            var newInventory = new TetrisInventory<TestItemType>(10, 8);

            Assert.AreEqual(10, newInventory.Width);
            Assert.AreEqual(8, newInventory.Height);
            Assert.AreEqual(0, newInventory.Items.Count);
        }

        #endregion

        #region AddItemAtPosition Tests

        [Test]
        public void AddItemAtPosition_EmptySlot_AddsSuccessfully()
        {
            var item = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0));
            bool wasAdded = false;

            _inventory.OnItemAdded += (addedItem) => wasAdded = true;

            bool result = _inventory.AddItemAtPosition(item);

            Assert.IsTrue(result);
            Assert.AreEqual(1, _inventory.Items.Count);
            Assert.IsTrue(wasAdded);
            Assert.AreEqual(item, _inventory.Items[0]);
        }

        [Test]
        public void AddItemAtPosition_ExceedsStackSize_PartiallyAdds()
        {
            var item = new ItemStack<TestItemType>(_testItemType1X1, 15, new Vector2Int(0, 0));
            bool wasAdded = false;

            _inventory.OnItemAdded += (addedItem) => wasAdded = true;

            bool result = _inventory.AddItemAtPosition(item);

            Assert.IsFalse(result);
            Assert.AreEqual(1, _inventory.Items.Count);
            Assert.IsTrue(wasAdded);
            Assert.AreEqual(10, _inventory.Items[0].amount); // Max stack size
            Assert.AreEqual(5, item.amount); // Remaining amount
        }

        [Test]
        public void AddItemAtPosition_SameItemTypeExists_StacksCorrectly()
        {
            var existingItem = new ItemStack<TestItemType>(_testItemType1X1, 3, new Vector2Int(0, 0));
            var newItem = new ItemStack<TestItemType>(_testItemType1X1, 4, new Vector2Int(0, 0));
            bool wasChanged = false;

            _inventory.AddItemAtPosition(existingItem);
            _inventory.OnItemChanged += (changedItem) => wasChanged = true;

            bool result = _inventory.AddItemAtPosition(newItem);

            Assert.IsTrue(result);
            Assert.AreEqual(1, _inventory.Items.Count);
            Assert.IsTrue(wasChanged);
            Assert.AreEqual(7, _inventory.Items[0].amount);
        }

        [Test]
        public void AddItemAtPosition_SameItemTypeExceedsStack_PartiallyStacks()
        {
            var existingItem = new ItemStack<TestItemType>(_testItemType1X1, 8, new Vector2Int(0, 0));
            var newItem = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0));

            _inventory.AddItemAtPosition(existingItem);

            bool result = _inventory.AddItemAtPosition(newItem);

            Assert.IsFalse(result);
            Assert.AreEqual(10, _inventory.Items[0].amount); // Max stack
            Assert.AreEqual(3, newItem.amount); // Remaining
        }

        [Test]
        public void AddItemAtPosition_DifferentItemTypeExists_ReturnsFalse()
        {
            var existingItem = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0));
            var newItem = new ItemStack<TestItemType>(_testItemType2X2, 2, new Vector2Int(0, 0));

            _inventory.AddItemAtPosition(existingItem);

            bool result = _inventory.AddItemAtPosition(newItem);

            Assert.IsFalse(result);
            Assert.AreEqual(1, _inventory.Items.Count);
        }

        [Test]
        public void AddItemAtPosition_CollidesWithExistingItem_ReturnsFalse()
        {
            var existingItem = new ItemStack<TestItemType>(_testItemType2X2, 1, new Vector2Int(0, 0));
            var newItem = new ItemStack<TestItemType>(_testItemType1X1, 3, new Vector2Int(1, 1));

            _inventory.AddItemAtPosition(existingItem);

            bool result = _inventory.AddItemAtPosition(newItem);

            Assert.IsFalse(result);
            Assert.AreEqual(1, _inventory.Items.Count);
        }

        #endregion

        #region AddAnywhere Tests

        [Test]
        public void AddAnywhere_EmptyInventory_AddsAtFirstPosition()
        {
            var item = new ItemStack<TestItemType>(_testItemType1X1, 5, Vector2Int.zero);

            _inventory.AddAnywhere(item);

            Assert.AreEqual(1, _inventory.Items.Count);
            Assert.AreEqual(new Vector2Int(0, 0), _inventory.Items[0].position);
        }

        [Test]
        public void AddAnywhere_SameItemExists_StacksFirst()
        {
            var existingItem = new ItemStack<TestItemType>(_testItemType1X1, 3, new Vector2Int(2, 2));
            var newItem = new ItemStack<TestItemType>(_testItemType1X1, 4, Vector2Int.zero);

            _inventory.AddItemAtPosition(existingItem);
            _inventory.AddAnywhere(newItem);

            Assert.AreEqual(1, _inventory.Items.Count);
            Assert.AreEqual(7, _inventory.Items[0].amount);
            Assert.AreEqual(new Vector2Int(2, 2), _inventory.Items[0].position);
        }

        [Test]
        public void AddAnywhere_InventoryFull_TriggersOverflow()
        {
            // Fill inventory
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    var item = new ItemStack<TestItemType>(_testItemType1X1, _testItemType1X1.StackSize,
                        new Vector2Int(x, y));
                    _inventory.AddItemAtPosition(item);
                }
            }

            var overflowItem = new ItemStack<TestItemType>(_testItemType1X1, 1, Vector2Int.zero);
            bool overflowTriggered = false;

            _inventory.OnItemOverflow += (item) => overflowTriggered = true;
            _inventory.AddAnywhere(overflowItem);

            Assert.IsTrue(overflowTriggered);
        }

        #endregion

        #region GetItemAtPosition Tests

        [Test]
        public void GetItemAtPosition_ItemExists_ReturnsItem()
        {
            var item = new ItemStack<TestItemType>(_testItemType2X2, 1, new Vector2Int(1, 1));
            _inventory.AddItemAtPosition(item);

            var foundItem = _inventory.GetItemAtPosition(new Vector2Int(1, 1));
            var foundItemCollision = _inventory.GetItemAtPosition(new Vector2Int(2, 2));

            Assert.AreEqual(item, foundItem);
            Assert.AreEqual(item, foundItemCollision);
        }

        [Test]
        public void GetItemAtPosition_NoItemExists_ReturnsNull()
        {
            var foundItem = _inventory.GetItemAtPosition(new Vector2Int(0, 0));

            Assert.IsNull(foundItem);
        }

        #endregion

        #region GetItemOfType Tests

        [Test]
        public void GetItemOfType_ItemExists_ReturnsFirstItem()
        {
            var item1 = new ItemStack<TestItemType>(_testItemType1X1, 3, new Vector2Int(0, 0));
            var item2 = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(1, 0));
            var item3 = new ItemStack<TestItemType>(_testItemType2X2, 2, new Vector2Int(2, 0));

            _inventory.AddItemAtPosition(item1);
            _inventory.AddItemAtPosition(item2);
            _inventory.AddItemAtPosition(item3);

            var foundItem = _inventory.GetItemOfType(_testItemType1X1);

            Assert.AreEqual(item1, foundItem);
        }

        [Test]
        public void GetItemOfType_NoItemExists_ReturnsNull()
        {
            var foundItem = _inventory.GetItemOfType(_testItemType1X1);

            Assert.IsNull(foundItem);
        }

        #endregion

        #region RemoveAmountFromPosition Tests

        [Test]
        public void RemoveAmountFromPosition_PartialRemoval_DecreasesAmount()
        {
            var item = new ItemStack<TestItemType>(_testItemType1X1, 8, new Vector2Int(0, 0));
            bool wasChanged = false;

            _inventory.AddItemAtPosition(item);
            _inventory.OnItemChanged += (changedItem) => wasChanged = true;

            _inventory.RemoveAmountFromPosition(new Vector2Int(0, 0), 3);

            Assert.AreEqual(1, _inventory.Items.Count);
            Assert.AreEqual(5, _inventory.Items[0].amount);
            Assert.IsTrue(wasChanged);
        }

        [Test]
        public void RemoveAmountFromPosition_CompleteRemoval_RemovesItem()
        {
            var item = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0));
            bool wasRemoved = false;

            _inventory.AddItemAtPosition(item);
            _inventory.OnItemRemoved += (removedItem) => wasRemoved = true;

            _inventory.RemoveAmountFromPosition(new Vector2Int(0, 0), 7);

            Assert.AreEqual(0, _inventory.Items.Count);
            Assert.IsTrue(wasRemoved);
        }

        [Test]
        public void RemoveAmountFromPosition_NoItemAtPosition_DoesNothing()
        {
            _inventory.RemoveAmountFromPosition(new Vector2Int(0, 0), 5);

            Assert.AreEqual(0, _inventory.Items.Count);
        }

        #endregion

        #region RemoveItem Tests

        [Test]
        public void RemoveItem_ItemExists_RemovesAndReturnsItem()
        {
            var item = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0));
            bool wasRemoved = false;

            _inventory.AddItemAtPosition(item);
            _inventory.OnItemRemoved += (removedItem) => wasRemoved = true;

            var removedItem = _inventory.RemoveItem(new Vector2Int(0, 0));

            Assert.AreEqual(item, removedItem);
            Assert.AreEqual(0, _inventory.Items.Count);
            Assert.IsTrue(wasRemoved);
        }

        [Test]
        public void RemoveItem_NoItemExists_ReturnsNull()
        {
            var removedItem = _inventory.RemoveItem(new Vector2Int(0, 0));

            Assert.IsNull(removedItem);
        }

        #endregion

        #region RemoveItemOfType Tests

        [Test]
        public void RemoveItemOfType_ItemExists_RemovesAndReturnsFirstItem()
        {
            var item1 = new ItemStack<TestItemType>(_testItemType1X1, 3, new Vector2Int(0, 0));
            var item2 = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(1, 0));
            bool wasRemoved = false;

            _inventory.AddItemAtPosition(item1);
            _inventory.AddItemAtPosition(item2);
            _inventory.OnItemRemoved += (removedItem) => wasRemoved = true;

            var removedItem = _inventory.RemoveItemOfType(_testItemType1X1);

            Assert.AreEqual(item1, removedItem);
            Assert.AreEqual(1, _inventory.Items.Count);
            Assert.AreEqual(item2, _inventory.Items[0]);
            Assert.IsTrue(wasRemoved);
        }

        [Test]
        public void RemoveItemOfType_NoItemExists_ReturnsNull()
        {
            var removedItem = _inventory.RemoveItemOfType(_testItemType1X1);

            Assert.IsNull(removedItem);
        }

        #endregion

        #region ReplaceItem Tests

        [Test]
        public void ReplaceItem_ItemExistsNoCollision_ReplacesItem()
        {
            var originalItem = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0));
            var replacementItem = new ItemStack<TestItemType>(_testItemType1X1, 3, new Vector2Int(0, 0));
            bool wasReplaced = false;

            _inventory.AddItemAtPosition(originalItem);
            _inventory.OnItemReplaced += (original, replacement) => wasReplaced = true;

            _inventory.ReplaceItem(new Vector2Int(0, 0), replacementItem);

            Assert.AreEqual(1, _inventory.Items.Count);
            Assert.AreEqual(replacementItem, _inventory.Items[0]);
            Assert.IsTrue(wasReplaced);
        }

        [Test]
        public void ReplaceItem_ItemExistsWouldCollide_DoesNotReplace()
        {
            var originalItem = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0));
            var otherItem = new ItemStack<TestItemType>(_testItemType1X1, 2, new Vector2Int(1, 0));
            var replacementItem = new ItemStack<TestItemType>(_testItemType2X2, 1, new Vector2Int(0, 0));
            bool wasReplaced = false;

            _inventory.AddItemAtPosition(originalItem);
            _inventory.AddItemAtPosition(otherItem);
            _inventory.OnItemReplaced += (original, replacement) => wasReplaced = true;

            _inventory.ReplaceItem(new Vector2Int(0, 0), replacementItem);

            Assert.AreEqual(2, _inventory.Items.Count);
            Assert.AreEqual(originalItem, _inventory.Items[0]); // Original item should still be there
            Assert.IsFalse(wasReplaced);
        }

        [Test]
        public void ReplaceItem_NoItemExists_DoesNothing()
        {
            var replacementItem = new ItemStack<TestItemType>(_testItemType2X2, 2, new Vector2Int(0, 0));
            bool wasReplaced = false;

            _inventory.OnItemReplaced += (original, replacement) => wasReplaced = true;

            _inventory.ReplaceItem(new Vector2Int(0, 0), replacementItem);

            Assert.AreEqual(0, _inventory.Items.Count);
            Assert.IsFalse(wasReplaced);
        }

        #endregion

        #region Event Tests

        [Test]
        public void Events_AddItem_TriggersOnItemAdded()
        {
            var item = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0));
            ItemStack<TestItemType> addedItem = null;

            _inventory.OnItemAdded += (i) => addedItem = i;

            _inventory.AddItemAtPosition(item);

            Assert.AreEqual(item, addedItem);
        }

        [Test]
        public void Events_ChangeItemAmount_TriggersOnItemChanged()
        {
            var item = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0));
            ItemStack<TestItemType> changedItem = null;

            _inventory.AddItemAtPosition(item);
            _inventory.OnItemChanged += (i) => changedItem = i;

            var additionalItem = new ItemStack<TestItemType>(_testItemType1X1, 2, new Vector2Int(0, 0));
            _inventory.AddItemAtPosition(additionalItem);

            Assert.AreEqual(item, changedItem);
        }

        [Test]
        public void Events_RemoveItem_TriggersOnItemRemoved()
        {
            var item = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(0, 0));
            ItemStack<TestItemType> removedItem = null;

            _inventory.AddItemAtPosition(item);
            _inventory.OnItemRemoved += (i) => removedItem = i;

            _inventory.RemoveItem(new Vector2Int(0, 0));

            Assert.AreEqual(item, removedItem);
        }

        #endregion

        #region Edge Cases and Boundary Tests

        [Test]
        public void AddItemAtPosition_OutOfBounds_CanStillAdd()
        {
            // The current implementation doesn't check bounds, so this should work
            var item = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(10, 10));

            bool result = _inventory.AddItemAtPosition(item);

            Assert.IsTrue(result);
            Assert.AreEqual(1, _inventory.Items.Count);
        }

        [Test]
        public void AddItemAtPosition_NegativePosition_CanStillAdd()
        {
            var item = new ItemStack<TestItemType>(_testItemType1X1, 5, new Vector2Int(-1, -1));

            bool result = _inventory.AddItemAtPosition(item);

            Assert.IsTrue(result);
            Assert.AreEqual(1, _inventory.Items.Count);
        }

        [Test]
        public void AddItemAtPosition_ZeroAmount_StillAdds()
        {
            var item = new ItemStack<TestItemType>(_testItemType1X1, 0, new Vector2Int(0, 0));

            bool result = _inventory.AddItemAtPosition(item);

            Assert.IsTrue(result);
            Assert.AreEqual(1, _inventory.Items.Count);
        }

        #endregion
    }

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