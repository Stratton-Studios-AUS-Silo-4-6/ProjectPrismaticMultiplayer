using System.Linq;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace StrattonStudioGames.PrisMulti
{
    /// <summary>
    /// A list of equippable items for a given <see cref="ItemType"/>.
    /// </summary>
    /// <remarks>
    /// e.g. Skins of a Photon Rifle.
    /// </remarks>
    public class LoadoutEquipList : MonoBehaviour
    {
        [SerializeField] private Button equipButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Transform container;
        [SerializeField] private LoadoutEquipEntry prefab;

        private ListView<Cosmetic, LoadoutEquipEntry> listView;
        private GunItemData gunItemData;
        private int selectedIndex;
        private Cosmetic[] cosmetics;
        
        #region Unity hooks

        private void Awake()
        {
            listView = new ListView<Cosmetic, LoadoutEquipEntry>(prefab, container);
        }

        private void OnEnable()
        {
            equipButton.onClick.AddListener(OnEquip);
            prevButton.onClick.AddListener(ToPrev);
            nextButton.onClick.AddListener(ToNext);
        }

        private void OnDisable()
        {
            equipButton.onClick.RemoveListener(OnEquip);
            prevButton.onClick.RemoveListener(ToPrev);
            nextButton.onClick.RemoveListener(ToNext);
        }

        #endregion

        #region Public methods

        public async void Init(GunItemData gunItemData)
        {
            this.gunItemData = gunItemData;
            cosmetics = await gunItemData.GetCosmetics();
            listView.Add(cosmetics);

            foreach (var entry in listView.Entries)
            {
                entry.gameObject.SetActive(false);
            }

            var equippedCosmetic = await CosmeticInventoryContainer.Instance.GetEquippedCosmetic<GunCosmetic>(gunItemData.ItemType);

            if (equippedCosmetic != null)
            {
                var equipped = listView.Entries.FirstOrDefault(x => x.Data.Id == equippedCosmetic.Id);
                selectedIndex = listView.Entries.IndexOf(equipped);
                equipped.gameObject.SetActive(true);
            }
            else
            {
                listView.Entries[selectedIndex].gameObject.SetActive(true);
            }
        }

        #endregion

        private void OnEquip()
        {
            var selectedCosmetic = cosmetics[selectedIndex];
            CosmeticInventoryContainer.Instance.Equip(gunItemData.ItemType, selectedCosmetic.Id);
            CosmeticInventoryContainer.Instance.Save();
        }

        private void ToPrev()
        {
            if (selectedIndex > 0)
            {
                listView.Entries[selectedIndex].gameObject.SetActive(false);
                selectedIndex--;
                listView.Entries[selectedIndex].gameObject.SetActive(true);
            }
        }

        private void ToNext()
        {
            if (selectedIndex < gunItemData.CosmeticsAmount - 1)
            {
                listView.Entries[selectedIndex].gameObject.SetActive(false);
                selectedIndex++;
                listView.Entries[selectedIndex].gameObject.SetActive(true);
            }
        }
    }
}