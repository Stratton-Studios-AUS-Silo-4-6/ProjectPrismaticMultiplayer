using MultiFPS.Gameplay;
using UnityEngine;

namespace MultiFPS.PrisMulti
{
    public class AutoFire : GunFire
    {
        [Tooltip("Amount of time between each shot.")]
        [SerializeField] private float interval = .1f;

        private float cooldown = 0f;
        private bool CanShoot => cooldown <= 0f;

        private void FixedUpdate()
        {
            if (!CanShoot)
            {
                cooldown -= Time.fixedDeltaTime;
            }
        }

        public override void Fire(Gun gun)
        {
            if (!CanShoot)
            {
                return;
            }
            
            cooldown = interval;
            
            gun.Use();
        }
    }
}