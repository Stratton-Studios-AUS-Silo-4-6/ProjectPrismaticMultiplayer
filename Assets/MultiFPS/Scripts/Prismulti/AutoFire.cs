using UnityEngine;

namespace MultiFPS.PrisMulti
{
    public class AutoFire : GunFire
    {
        [Tooltip("Amount of time between each shot.")]
        [SerializeField] private float interval = .1f;

        private float cooldown = 0f;
        private bool CanFire => cooldown <= 0f;

        private bool isTriggering;

        private void FixedUpdate()
        {
            if (cooldown > 0f)
            {
                cooldown -= Time.fixedDeltaTime;
            }
            else if (isTriggering)
            {
                gun.Use();
                cooldown = interval;
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
    }
}