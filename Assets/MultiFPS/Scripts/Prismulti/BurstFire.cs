using System.Collections;
using MultiFPS.Gameplay;
using UnityEngine;

namespace MultiFPS.PrisMulti
{
    public class BurstFire : GunFire
    {
        [Tooltip("Total amount of time it takes for all shots in a single burst to be fired.")]
        [SerializeField] private float duration = .5f;
        
        [Tooltip("Delay between each burst.")]
        [SerializeField] private float delay = .1f;
        
        [Tooltip("Amount of shots fired in a single burst.")]
        [SerializeField] private int shots = 3;

        private float cooldown = 0f;
        private float Interval => duration / shots;
        private bool CanShoot => cooldown <= 0f && routine == null;

        private Coroutine routine;
        
        private void FixedUpdate()
        {
            if (!CanShoot)
            {
                cooldown -= Time.fixedDeltaTime;
            }
        }

        public override void Fire(Gun gun)
        {
            if (CanShoot)
            {
                cooldown = delay;
                routine = StartCoroutine(Routine());
            }
            
            return;
            IEnumerator Routine()
            {
                gun.Use();
                
                for (int i = 1; i < shots; i++)
                {
                    yield return new WaitForSeconds(Interval);
                    gun.Use();
                }

                routine = null;
            }
        }
    }
}