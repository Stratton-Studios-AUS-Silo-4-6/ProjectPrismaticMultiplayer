using MultiFPS;
using UnityEngine;
using UnityEngine.UI;

namespace StrattonStudioGames.PrisMulti
{
    public class GameSelectionMap : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private Toggle toggle;
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private GameSelectionPanel gameSelectionPanel;
        [SerializeField] private float minimizedHeight = 50;
        [SerializeField] private float expandedHeight = 100;
        
        [Header("Asset references")]
        [SerializeField] private MapRepresenter mapData;

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
                var index = transform.GetSiblingIndex();
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
    }
}