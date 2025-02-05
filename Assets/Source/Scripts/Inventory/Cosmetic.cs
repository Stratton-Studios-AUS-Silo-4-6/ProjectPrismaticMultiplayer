using Beamable.Common.Inventory;
using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    /// <summary>
    /// Component containing cosmetics data to be applied to object.
    /// </summary>
    public abstract class Cosmetic : ItemContent
    {
        [SerializeField] private ItemType itemType;

        public ItemType ItemType => itemType;
    } 
}