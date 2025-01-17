using MultiFPS.Gameplay;
using UnityEngine;

namespace MultiFPS.PrisMulti
{
    public abstract class GunFire : MonoBehaviour
    {
        [SerializeField] protected Gun gun;
        
        public abstract void ReleaseTrigger();
        public abstract void PressTrigger();

        protected virtual void Reset()
        {
            gun = GetComponent<Gun>();
        }
    }
}