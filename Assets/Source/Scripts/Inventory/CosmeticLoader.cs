using Beamable.Common.Content;
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

        /// <summary>
        /// Test value for passing content data to the network.
        /// </summary>
        [SerializeField] private ContentRef<GunCosmetic> localContent;

        [SyncVar] private string syncContentId;

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
            if (!item.MyOwner?.isOwned ?? false)
            {
                return;
            }

            if (!isServer)
            {
                CmdSetCosmetics(localContent.GetId());
            }
            else
            {
                RpcSetCosmetics(localContent.GetId());
            }
        }

        /// <summary>
        /// The command given by the client to a server that tells other clients to update the cosmetic of this item. 
        /// </summary>
        /// <param name="contentId"></param>
        [Command]
        private void CmdSetCosmetics(string contentId)
        {
            syncContentId = contentId;
            RpcSetCosmetics(contentId);
        }

        /// <summary>
        /// The rpc received by other clients to update their own instance of the item with the applied cosmetic.
        /// </summary>
        /// <param name="contentId"></param>
        [ClientRpc]
        private void RpcSetCosmetics(string contentId)
        {
            SetCosmeticsInternal(contentId);
        }

        /// <summary>
        /// The internal method that actually applies the cosmetics to this item.
        /// </summary>
        /// <param name="contentId"></param>
        private async void SetCosmeticsInternal(string contentId)
        {
            var content = await Beamable.BeamContext.Default.Api.ContentService.GetContent(contentId);
            var cosmetic = content as GunCosmetic;
            cosmetic.Apply(skinnedMeshRenderer);
        }

        public override void OnStartClient()
        {
            if (!string.IsNullOrWhiteSpace(syncContentId))
            {
                SetCosmeticsInternal(syncContentId);
            }
        }
    }
}