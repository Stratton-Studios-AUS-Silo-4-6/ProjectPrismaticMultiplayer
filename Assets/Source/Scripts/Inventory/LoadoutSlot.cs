using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StrattonStudioGames.PrisMulti
{
    /// <summary>
    /// UI component that handles displaying <see cref="ItemData"/>.
    /// </summary>
    public class LoadoutSlot : MonoBehaviour
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI label;

        #region Unity hooks

        private void Start()
        {
            label.text = itemData?.ItemName ?? string.Empty;
            button.interactable = itemData;
        }

        private void OnEnable()
        {
            button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(OnClick);
        }

        private void Reset()
        {
            button = GetComponent<Button>();
        }

        #endregion

        private void OnClick()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name != "LoadoutEquip")
            {
                return;
            }

            StartCoroutine(Wait());
            IEnumerator Wait()
            {
                yield return new WaitUntil(() => LoadoutEquipPage.Instance);
                LoadoutEquipPage.Instance.Init(itemData);
            }
            
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}