using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        [Tooltip("If true, sets the object to DontDestroyOnLoad.")]
        [SerializeField] private bool isPersistent;
        
        private static T instance;

        public static T Instance => instance;

        protected virtual void Awake()
        {
            if (instance)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this as T;

            if (isPersistent)
            {
                transform.parent = null;
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}