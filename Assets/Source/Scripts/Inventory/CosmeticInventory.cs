using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable;
using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    /// <summary>
    /// Save data for player inventory.
    /// </summary>
    [Serializable]
    public class CosmeticInventory
    {
        [SerializeField]
        public List<ItemCosmeticPair> cosmeticEquips = new()
        {
            new() {itemType = ItemType.PhotonRifle, cosmeticId = null},
            new() {itemType = ItemType.PulseCarbine, cosmeticId = null},
            new() {itemType = ItemType.BallisticRepeater, cosmeticId = null},
            new() {itemType = ItemType.TacticalSmg, cosmeticId = null},
        };
        
        public void Equip(ItemType itemType, string cosmeticId)
        {
            var entry = cosmeticEquips.FirstOrDefault(x => x.itemType == itemType);
            
            if (entry == null)
            {
                cosmeticEquips.Add(new ItemCosmeticPair
                {
                    itemType = itemType,
                    cosmeticId = cosmeticId,
                });
            }
            else
            {
                var index = cosmeticEquips.IndexOf(entry);
                cosmeticEquips[index] = new ItemCosmeticPair
                {
                    itemType = itemType,
                    cosmeticId = cosmeticId,
                };
            }
        }

        public async Task<T> GetEquippedCosmetic<T>(ItemType itemType) where T : Cosmetic
        {
            var entry = cosmeticEquips.FirstOrDefault(x => x.itemType == itemType);

            if (string.IsNullOrWhiteSpace(entry?.cosmeticId) )
            {
                Debug.Log($"no equipped for: {itemType.ToString()}");
                return null;
            }

            Debug.Log($"getting equipped: {itemType.ToString()} [{entry.cosmeticId}]");
            var content = await BeamContext.Default.Api.ContentService.GetContent(entry.cosmeticId);
            var cosmetic = content as T;

            return cosmetic;
        }

        public bool Matches(CosmeticInventory other)
        {
            if (cosmeticEquips.Count != other.cosmeticEquips.Count)
            {
                return false;
            }
            for (var i = 0; i < cosmeticEquips.Count; i++)
            {
                var x = cosmeticEquips[i];
                var y = other.cosmeticEquips[i];
                if (!x.Matches(y))
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Entry containing an <see cref="ItemType"/> - cosmeticId pair.
    /// </summary>
    [Serializable]
    public class ItemCosmeticPair
    {
        public ItemType itemType;
        public string cosmeticId;

        public bool Matches(ItemCosmeticPair other)
        {
            if (itemType != other.itemType)
            {
                return false;
            }

            if (cosmeticId != other.cosmeticId)
            {
                return false;
            }

            return true;
        }
    }
}