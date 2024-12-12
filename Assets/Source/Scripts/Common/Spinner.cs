using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    /// <summary>
    /// Place this on a UI object to make it spin while enabled.
    /// </summary>
    public class Spinner : MonoBehaviour
    {
        [SerializeField] private float angle = 60;
        
        private void LateUpdate()
        {
            transform.Rotate(Vector3.forward, angle * Time.deltaTime);
        }
    }
}