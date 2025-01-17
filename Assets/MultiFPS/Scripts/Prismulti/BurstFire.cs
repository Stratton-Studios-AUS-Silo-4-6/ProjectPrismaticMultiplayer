using System;
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

        private float interval;
        private float cooldown = 0f;
        private bool isTriggering;

        private void FixedUpdate()
        {
            if (cooldown > 0f)
            {
                cooldown -= Time.fixedDeltaTime;
            }
            else if (!isTriggering)
            {
                // do nothing
            }
            else if (shots++ < 3)
            {
                gun.Use();
                cooldown = interval;
            }
            else
            {
                cooldown = delay;
                shots = 0;
            }
        }

        public override void PressTrigger()
        {
            isTriggering = true;
        }

        public override void ReleaseTrigger()
        {
            isTriggering = false;
        }

        private void Start()
        {
            interval = duration / shots;
        }
    }
}