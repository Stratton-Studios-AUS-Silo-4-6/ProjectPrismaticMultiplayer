using System;
using UnityEngine;

namespace MultiFPS.PrisMulti
{
    public class ChargeFire : GunFire
    {
        [SerializeField] private bool fireOnMax;
        [SerializeField] private AnimationCurve damageScaling = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        private float elapsed;
        private float maxTime;
        private bool isCharging;

        private void Start()
        {
            maxTime = damageScaling.keys[^1].time;
        }

        private void Update()
        {
            if (elapsed > 0f)
            {
                elapsed -= Time.deltaTime;
            }
            else if (isCharging && fireOnMax)
            {
                ReleaseTrigger();
            }
        }

        public override void PressTrigger()
        {
            elapsed = maxTime;
            isCharging = true;
        }

        public override void ReleaseTrigger()
        {
            if (!isCharging)
                return;
            
            isCharging = false;
            var charge = maxTime - elapsed;
            var damage = (int)damageScaling.Evaluate(charge);
            var hitscan = FireHitscan(damage);
            gun.Use(hitscan);
            
            elapsed = 0f;
        }
    }
}