using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StrattonStudioGames.PrisMulti
{
    /// <summary>
    /// Opens a single (non-addtive) scene when pressing a button.
    /// </summary>
    public class SceneButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private string sceneName;

        private void OnEnable()
        {
            button.onClick.AddListener(OpenScene);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(OpenScene);
        }

        private void Reset()
        {
            button = GetComponent<Button>();
        }

        private void OpenScene()
        {
            SceneManager.LoadSceneAsync(sceneName);
        }
    }
}