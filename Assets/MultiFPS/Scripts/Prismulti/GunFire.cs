using MultiFPS.Gameplay;
using UnityEngine;

namespace MultiFPS.PrisMulti
{
    public abstract class GunFire : MonoBehaviour
    {
        [SerializeField] protected Gun gun;

        public abstract void PressTrigger();
        public abstract void ReleaseTrigger();

        protected virtual void Reset()
        {
            gun = GetComponent<Gun>();
        }
    }
}