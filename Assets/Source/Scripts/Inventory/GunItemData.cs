using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable.Common.Content;
using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    /// <summary>
    /// Contains data used to display UI details in the store and inventory screen.
    /// </summary>
    [CreateAssetMenu(menuName = "PrisMulti/GunItemData")]
    public class GunItemData : ScriptableObject
    {
        [SerializeField] private ItemType itemType;
        [SerializeField] private string itemName;
        [SerializeField] private ContentLink<GunCosmetic>[] cosmeticsRef;

        public ItemType ItemType => itemType;
        public string ItemName => itemName;

        public int CosmeticsAmount => cosmeticsRef.Length;

        public async Task<GunCosmetic[]> GetCosmetics()
        {
            var cosmeticsList = new List<GunCosmetic>();
            foreach (var entry in cosmeticsRef)
            {
                var content = await Beamable.BeamContext.Default.Api.ContentService.GetContent(entry.GetId());
                var cosmetic = content as GunCosmetic;
                
                if (cosmetic != null)
                {
                    cosmeticsList.Add(cosmetic);
                }
            }

            return cosmeticsList.ToArray();
        }
    }
}