using System.Linq;
using MultiFPS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StrattonStudioGames.PrisMulti
{
    public class GameSelectionMap : MonoBehaviour, IListViewEntry<GameSelectionMap.EntryData>
    {
        [Header("Scene references")]
        [SerializeField] private GameSelectionPanel gameSelectionPanel;

        [Header("Prefab references")]
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Toggle toggle;
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private float minimizedHeight = 50;
        [SerializeField] private float expandedHeight = 100;
        
        [Header("Asset references")]
        [SerializeField] private MapRepresenter mapData;

        [Header("ListView settings")]
        [SerializeField] private GameSelectionMode entryPrefab;
        [SerializeField] private Transform entryContainer;

        private ListView<GameSelectionMode.EntryData, GameSelectionMode> listView;

        #region Public methods

        [ContextMenu(nameof(InitList))]
        public void InitList()
        {
            listView = new ListView<GameSelectionMode.EntryData, GameSelectionMode>(entryPrefab, entryContainer);

            var index = 0;
            var data = from gamemode in mapData.AvailableGamemodes
                select new GameSelectionMode.EntryData
                {
                    gamemode = gamemode,
                    selectionPanel = gameSelectionPanel,
                    @group = toggleGroup,
                    index = index++,
                };
            
            listView.Add(data.ToArray());
        }

        #endregion

        #region Unity hooks

        private void OnEnable()
        {
            toggle.onValueChanged.AddListener(OnToggle);
        }

        private void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(OnToggle);
        }

        private void Reset()
        {
            toggle = GetComponent<Toggle>();
            toggleGroup = GetComponent<ToggleGroup>();
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();
        }

        #endregion

        [ContextMenu(nameof(Show))]
        private void Show()
        {
            OnToggle(true);
        }

        [ContextMenu(nameof(Hide))]
        private void Hide()
        {
            OnToggle(false);
        }

        private void OnToggle(bool isOn)
        {
            if (isOn)
            {
                gameSelectionPanel.SelectMap(mapData);
                
                var rectSize = rectTransform.sizeDelta;
                rectSize.y = expandedHeight;
                rectTransform.sizeDelta = rectSize;
                
                canvasGroup.alpha = 1;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            else
            {
                gameSelectionPanel.SelectMap(null);
                
                var rectSize = rectTransform.sizeDelta;
                rectSize.y = minimizedHeight;
                rectTransform.sizeDelta = rectSize;
                
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                
                foreach (var entry in toggleGroup.ActiveToggles())
                {
                    entry.isOn = false;
                }
            }
        }

        #region IListViewEntry implementations

        public void OnAdd(EntryData data)
        {
            mapData = data.mapData;
            label.text = data.mapData.Name;
            name = data.mapData.Name;
            gameSelectionPanel = data.selectionPanel;
            toggle.group = data.group;
        }

        public void OnRemove()
        {
        }

        #endregion
        
        public class EntryData
        {
            public GameSelectionPanel selectionPanel;
            public MapRepresenter mapData;
            public ToggleGroup group;
        }
    }
}