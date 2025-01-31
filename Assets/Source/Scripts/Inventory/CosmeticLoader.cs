using Mirror;
using MultiFPS.Gameplay;
using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    public class CosmeticLoader : NetworkBehaviour
    {
        /// <summary>
        /// Id of the item. Used to check what is the equipped cosmetic in the loadout for this item.
        /// </summary>
        [SerializeField] private string itemId;

        [SerializeField] private Item item;
        
        [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;

        [SyncVar] private string syncCosmeticId;

        private void OnEnable()
        {
            item.onTake += SetCosmetics;
        }

        private void OnDisable()
        {
            item.onTake -= SetCosmetics;
        }

        /// <summary>
        /// Invoked by a client to try to set the cosmetics of this item.
        /// </summary>
        [Client]
        private void SetCosmetics()
        {
            Debug.Log("attempting to set skin...");
            
            if (!item.MyOwner?.isOwned ?? false)
            {
                return;
            }
            if (CosmeticApi.TryGetEquippedCosmetic<GunCosmetic>(itemId, out var cosmetic))
            {
                CmdSetCosmetics(cosmetic.CosmeticId);
            }
            else
            {
                Debug.LogError($"cannot find asset for: [{cosmetic.CosmeticId}]");
            }
        }

        /// <summary>
        /// The command given by the client to a server that tells other clients to update the cosmetic of this item. 
        /// </summary>
        /// <param name="cosmeticId"></param>
        [Command]
        private void CmdSetCosmetics(string cosmeticId)
        {
            syncCosmeticId = cosmeticId;
            RpcSetCosmetics(cosmeticId);
        }

        /// <summary>
        /// The rpc received by other clients to update their own instance of the item with the applied cosmetic.
        /// </summary>
        /// <param name="cosmeticId"></param>
        [ClientRpc]
        private void RpcSetCosmetics(string cosmeticId)
        {
            SetCosmeticsInternal(cosmeticId);
        }

        /// <summary>
        /// The internal method that actually applies the cosmetics to this item.
        /// </summary>
        /// <param name="cosmeticId"></param>
        private void SetCosmeticsInternal(string cosmeticId)
        {
            if (CosmeticDatabase.Instance.TryFind<GunCosmetic>(cosmeticId, out var cosmetic))
            {
                cosmetic.Apply(skinnedMeshRenderer);
            }
            else
            {
                Debug.LogError($"No cosmetic found for {cosmeticId ?? "null"}");
            }
        }

        public override void OnStartClient()
        {
            if (!string.IsNullOrWhiteSpace(syncCosmeticId))
            {
                SetCosmeticsInternal(syncCosmeticId);
            }
        }
    }
}