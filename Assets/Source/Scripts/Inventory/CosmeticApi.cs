using System.Threading.Tasks;
using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    public static class CosmeticApi
    {
        public static void Equip(ItemType itemType, string cosmeticId)
        {
            var equipId = ToEquipMapId(itemType.ToString());
            PlayerPrefs.SetString(equipId, cosmeticId);
        }

        public static async Task<T> GetEquippedCosmetic<T>(ItemType itemType) where T : Cosmetic
        {
            var equipId = ToEquipMapId(itemType.ToString());
            var cosmeticId = PlayerPrefs.GetString(equipId, null);

            if (string.IsNullOrWhiteSpace(cosmeticId))
            {
                return null;
            }

            var content = await Beamable.BeamContext.Default.Api.ContentService.GetContent(cosmeticId);
            var cosmetic = content as T;
            return cosmetic;
        }

        private static string ToEquipMapId(string itemId)
        {
            return $"local.equip.{itemId}";
        }
    }
}