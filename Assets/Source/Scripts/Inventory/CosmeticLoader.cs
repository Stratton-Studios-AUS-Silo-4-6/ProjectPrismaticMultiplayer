using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    public class CosmeticLoader : MonoBehaviour
    {
        /// <summary>
        /// Id of the item. Used to check what is the equipped cosmetic in the loadout for this item.
        /// </summary>
        [SerializeField] private string itemId;
        
        /// <summary>
        /// Placeholder field. Direct reference to the cosmetic id which loads the cosmetic for the item.
        /// </summary>
        [SerializeField] private string cosmeticId;
        
        [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;

        private void Start()
        {
            if (CosmeticDatabase.Instance.TryFind<GunCosmetic>(cosmeticId, out var gunCosmetic))
            {
                gunCosmetic.Apply(skinnedMeshRenderer);
            }
            else
            {
                Debug.LogError($"Could not find cosmetic of id [{cosmeticId}].");
            }
        }
    }
}