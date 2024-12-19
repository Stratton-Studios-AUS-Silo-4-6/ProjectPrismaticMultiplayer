using System;
using MultiFPS.Gameplay.Gamemodes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StrattonStudioGames.PrisMulti
{
    public class GameSelectionMode : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private GameSelectionPanel gameSelectionPanel;

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
                var index = transform.GetSiblingIndex();
                gameSelectionPanel.SelectMode(index);
            }
            else
            {
                gameSelectionPanel.SelectMode(null);
            }
        }
    }
}