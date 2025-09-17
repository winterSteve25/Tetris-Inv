using UnityEngine;

namespace TetrisInv.Runtime
{
    public class ItemType : ScriptableObject
    {
        [field: SerializeField] public int StackSize { get; private set; }
        [field: SerializeField] public Vector2Int Size { get; private set; }
    }
}