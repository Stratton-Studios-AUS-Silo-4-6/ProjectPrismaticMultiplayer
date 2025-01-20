using UnityEngine;

namespace MultiFPS.PrisMulti
{
    public class SingleFire : GunFire
    {
        [Tooltip("Amount of time between each shot.")]
        [SerializeField] private float interval = .1f;

        private float cooldown = 0f;
        private bool CanFire => cooldown <= 0f;

        private void FixedUpdate()
        {
            if (cooldown > 0f)
            {
                cooldown -= Time.fixedDeltaTime;
            }
        }

        public override void PressTrigger()
        {
            if (CanFire)
            {
                gun.Use();
                cooldown = interval;
            }
        }

        public override void ReleaseTrigger()
        {
        }
    }
}