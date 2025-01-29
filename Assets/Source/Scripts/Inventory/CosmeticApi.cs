using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    public static class CosmeticApi
    {
        public static void Equip(string itemId, string cosmeticId)
        {
            PlayerPrefs.SetString(itemId, cosmeticId);
        }

        public static bool TryGetEquippedCosmetic<T>(string itemId, out T cosmetic) where T : Cosmetic
        {
            var cosmeticId = PlayerPrefs.GetString(itemId, null);

            if (string.IsNullOrWhiteSpace(cosmeticId))
            {
                cosmetic = null;
                return false;
            }

            return CosmeticDatabase.Instance.TryFind(cosmeticId, out cosmetic);
        }
    }
}