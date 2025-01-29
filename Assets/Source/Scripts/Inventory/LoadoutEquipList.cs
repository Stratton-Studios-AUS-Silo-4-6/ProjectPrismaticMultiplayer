using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace StrattonStudioGames.PrisMulti
{
    public class LoadoutEquipList : MonoBehaviour
    {
        [SerializeField] private Button equipButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Transform container;
        [SerializeField] private LoadoutEquipEntry prefab;

        private ListView<Cosmetic, LoadoutEquipEntry> listView;
        private ItemData itemData;
        private int selectedIndex;
        
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

        public void Init(ItemData itemData)
        {
            this.itemData = itemData;
            listView.Add(itemData.Cosmetics);

            foreach (var entry in listView.Entries)
            {
                entry.gameObject.SetActive(false);
            }
            
            listView.Entries[selectedIndex].gameObject.SetActive(true);
        }

        #endregion

        private void OnEquip()
        {
            var selectedCosmetic = itemData.Cosmetics[selectedIndex];
            CosmeticApi.Equip(itemData.ItemId, selectedCosmetic.CosmeticId);
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
            if (selectedIndex < itemData.Cosmetics.Length - 1)
            {
                listView.Entries[selectedIndex].gameObject.SetActive(false);
                selectedIndex++;
                listView.Entries[selectedIndex].gameObject.SetActive(true);
            }
        }
    }
}