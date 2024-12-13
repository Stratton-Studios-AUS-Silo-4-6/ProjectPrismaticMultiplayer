using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace StrattonStudioGames.PrisMulti
{
    public class PopupDialog : MonoBehaviour
    {
        #region Fields

        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI messageLabel;
        [SerializeField] private Transform dialogButtonContainer;
        [SerializeField] private Button dialogButtonPrototype;
        [SerializeField] private Button closeButton;

        #endregion

        #region Unity hooks

        private void Start()
        {
            dialogButtonPrototype.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            closeButton.onClick.AddListener(OnClose);
        }

        private void OnDisable()
        {
            closeButton.onClick.RemoveListener(OnClose);
        }

        #endregion

        private void OnClose()
        {
            Destroy(gameObject);
        }

        #region Static methods

        public static void Show(string title, string message, params (string, UnityAction)[] responses)
        {
            var prefab = Resources.Load<PopupDialog>("PopupDialog");
            var instance = Instantiate(prefab);
            instance.titleLabel.text = title;
            instance.messageLabel.text = message;

            foreach (var response in responses)
            {
                var dialogButtonInstance = Instantiate(instance.dialogButtonPrototype, instance.dialogButtonContainer);
                var dialogLabel = dialogButtonInstance.GetComponentInChildren<TextMeshProUGUI>();
                dialogLabel.text = response.Item1;
                dialogButtonInstance.onClick.AddListener(response.Item2);
                dialogButtonInstance.onClick.AddListener(() => Destroy(instance.gameObject));
            }
        }

        #endregion
    }
}