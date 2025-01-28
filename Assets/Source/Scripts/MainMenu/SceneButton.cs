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
        [SerializeField] private Mode mode;
        [SerializeField] private LoadSceneMode loadSceneMode = LoadSceneMode.Single; // todo: hide from inspector when Mode is unload
            
        public enum Mode
        {
            Load,
            Unload,
        }

        private void OnEnable()
        {
            button.onClick.AddListener(TransitionScene);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(TransitionScene);
        }

        private void Reset()
        {
            button = GetComponent<Button>();
        }

        private void TransitionScene()
        {
            if (mode == Mode.Load)
            {
                SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            }
            else if (mode == Mode.Unload)
            {
                SceneManager.UnloadSceneAsync(sceneName);
            }
        }
    }
}