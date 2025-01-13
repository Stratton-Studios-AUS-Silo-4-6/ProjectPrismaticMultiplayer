using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    /// <summary>
    /// Configures the cursor behaviour on Start.
    /// </summary>
    public class CursorSettings : MonoBehaviour
    {
        [SerializeField] private bool isVisible;
        [SerializeField] private CursorLockMode lockMode;
        private void Start()
        {
            Cursor.visible = isVisible;
            Cursor.lockState = lockMode;
        }
    }
}