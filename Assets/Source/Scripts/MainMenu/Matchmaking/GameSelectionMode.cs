using MultiFPS.Gameplay.Gamemodes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StrattonStudioGames.PrisMulti
{
    public class GameSelectionMode : MonoBehaviour, IListViewEntry<GameSelectionMode.EntryData>
    {
        [Header("Scene references")]
        [SerializeField] private GameSelectionPanel gameSelectionPanel;
        
        [Header("Prefab references")]
        [SerializeField] private Toggle toggle;
        [SerializeField] private TextMeshProUGUI label;

        private int? index;

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
            gameSelectionPanel = GetComponentInParent<GameSelectionPanel>();
        }

        #endregion

        private void OnToggle(bool isOn)
        {
            if (isOn)
            {
                index ??= transform.GetSiblingIndex();
                gameSelectionPanel.SelectMode(index);
            }
            else
            {
                gameSelectionPanel.SelectMode(null);
            }
        }

        #region IListViewEntry implementations

        public void OnAdd(EntryData entryData)
        {
            label.text = entryData.gamemode.ToString();
            toggle.group = entryData.group;
            gameSelectionPanel = entryData.selectionPanel;
            index = entryData.index;
            name = $"{index} : {entryData.gamemode.ToString()}" ;
        }

        public void OnRemove()
        {
        }
        
        #endregion

        public class EntryData
        {
            public ToggleGroup group;
            public Gamemodes gamemode;
            public GameSelectionPanel selectionPanel;
            public int index;
        }
    }
}