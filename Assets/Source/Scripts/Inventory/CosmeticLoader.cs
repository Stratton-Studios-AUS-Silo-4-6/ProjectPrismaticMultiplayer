using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    public class CosmeticLoader : MonoBehaviour
    {
        /// <summary>
        /// Id of the item. Used to check what is the equipped cosmetic in the loadout for this item.
        /// </summary>
        [SerializeField] private string itemId;
        
        [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;

        private void Start()
        {
            if (CosmeticApi.TryGetEquippedCosmetic<GunCosmetic>(itemId, out var cosmetic))
            {
                cosmetic.Apply(skinnedMeshRenderer);
                
            }
            else
            {
                Debug.LogError($"Could not find cosmetic of item [{itemId}].");
            }
        }
    }
}