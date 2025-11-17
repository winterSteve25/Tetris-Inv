using UnityEngine;

namespace TetrisInv.Runtime
{
    public class ItemType : ScriptableObject
    {
        [field: SerializeField] public virtual int StackSize { get; protected set; }
        [field: SerializeField] public virtual Vector2Int Size { get; protected set; }
    }
}