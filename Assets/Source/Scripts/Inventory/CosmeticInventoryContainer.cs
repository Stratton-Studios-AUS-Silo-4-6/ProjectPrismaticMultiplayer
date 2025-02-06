using System.Linq;
using System.Threading.Tasks;
using Beamable;
using Beamable.Player;
using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    public class CosmeticInventoryContainer : MonoSingleton<CosmeticInventoryContainer>
    {
        private CosmeticInventory inventory;
        private readonly string fileName = "cosmetic-inventory.json";

        #region Public methods
        public async Task Load()
        {
            await CloudSave.Instance.Refresh();
            inventory = await CloudSave.Instance.LoadData<CosmeticInventory>(fileName);
        }

        public void Save()
        {
            CloudSave.Instance.SaveData(fileName, inventory);
        }

        public void Equip(ItemType itemType, string cosmeticId)
        {
            inventory.Equip(itemType, cosmeticId);
        }

        public async Task<T> GetEquippedCosmetic<T>(ItemType itemType) where T : Cosmetic
        {
            return await inventory.GetEquippedCosmetic<T>(itemType);
        }

        public void Log()
        {
            foreach (var entry in inventory.cosmeticEquips)
            {
                Debug.Log($"[{entry.itemType.ToString()}] ({entry.cosmeticId})");
            }   
        }

        #endregion

        #region Unity hooks

        private async void Start()
        {
            await Load();
            if (inventory == null)
            {
                inventory = new CosmeticInventory();
            }
        }

        #endregion

        private async Task<PlayerItemGroup> RefreshItems(string contentType = null)
        {
            var context = await BeamContext.Default.Instance;
            var items = string.IsNullOrEmpty(contentType) ?
                context.Inventory.GetItems() :
                context.Inventory.GetItems(contentType);
            await items.Refresh();
            return items;
        }

        /// <summary>
        /// Gets a Player's owned cosmetics of a given <see cref="ItemType"/>.
        /// </summary>
        /// <param name="itemType"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private async Task<T[]> GetCosmetics<T>(ItemType itemType) where T : Cosmetic
        {
            var items = await RefreshItems("items.cosmetic");
            var cosmetics = from item in items
                let cosmetic = item.Content as T
                where cosmetic.ItemType == itemType
                select cosmetic;
            return cosmetics.ToArray();
        }
    }
}
