using System.Linq;
using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    /// <summary>
    /// Component of the prefab singleton containing all cosmetic entries
    /// </summary>
    public class CosmeticDatabase : MonoSingleton<CosmeticDatabase>
    {
        [SerializeField] private Cosmetic[] entries;

        public bool TryFind<T>(string cosmeticId, out T cosmetic) where T : Cosmetic
        {
             cosmetic = entries.FirstOrDefault(item => item.Id == cosmeticId) as T;
             
             if (cosmetic == null)
             {
                 return false;
             }

             return true;
        }
    }
}