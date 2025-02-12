using MultiFPS.Gameplay;
using UnityEngine;

namespace MultiFPS.PrisMulti
{
    public class SingleHitscanAoe : GunFire
    {
        [Tooltip("Amount of time between each shot.")]
        [SerializeField] private float interval = .1f;

        [SerializeField] private float effectRadius = 1f;

        [Tooltip("How far the hitscan reaches.")]
        [SerializeField] private float range = 250f;

        [SerializeField] private int directDamage = 10;
        [SerializeField] private int areaDamage = 10;

        [SerializeField] private LayerMask layerMask;

        private float cooldown;
        private bool CanFire => cooldown <= 0f && gun.CanUse();

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
                cooldown = interval;
                Shoot();
                gun.ClientReduceAmmo();
            }
        }

        public override void ReleaseTrigger()
        {
        }

        /// <summary>
        /// Shoot without penetration. Explosion occurs immediately on a valid hit.
        /// </summary>
        private void Shoot()
        {
            var barrel = gun.FirePoint;

            var hasHit = Physics.Raycast(barrel.position, barrel.forward, out var hit, range, layerMask, QueryTriggerInteraction.Ignore);

            if (hasHit && hit.transform.TryGetComponent<HitBox>(out var hitbox))
            {
                if (gun.MyOwner.BOT)
                {
                    gun.ServerDamage(hitbox._health, hitbox.part, directDamage, 1f, AttackType.hitscan);
                }
                else
                {
                    gun.CmdDamage(hitbox._health.DNID, hitbox.part, directDamage, 1f, AttackType.hitscan);
                }
            }

            var hitscanData = new Hitscan
            {
                FirstHitRotation = Quaternion.identity,
                PenetratedObjectMaterialsIDs = new[] { hasHit && hit.collider.CompareTag("Flesh") ? (byte) 1 :(byte) 0 },
                PenetrationPositions = new[] { hasHit ? hit.point : barrel.forward * range },
            }; 
            
            gun.VisualUse(hitscanData);

            if (hasHit)
            {
                Explosion(hit.point);
            }
            
            gun.Recoil();
        }

        private void Explosion(Vector3 origin)
        {
            var areaHits = Physics.OverlapSphere(origin, effectRadius, layerMask, QueryTriggerInteraction.Ignore);

            var hitscanData = new Hitscan
            {
                FirstHitRotation = Quaternion.identity,
            };

            hitscanData.PenetratedObjectMaterialsIDs = new byte[areaHits.Length];
            hitscanData.PenetrationPositions = new Vector3[areaHits.Length];


            for (var i = 0; i < areaHits.Length; i++)
            {
                var areaHit = areaHits[i];
                if (areaHit.TryGetComponent<HitBox>(out var areaHitbox))
                {
                    if (gun.MyOwner.BOT)
                    {
                        gun.ServerDamage(areaHitbox._health, CharacterPart.legs, areaDamage, .333f,
                            AttackType.explosion);
                    }
                    else
                    {
                        gun.CmdDamage(areaHitbox._health.DNID, CharacterPart.legs, areaDamage, .333f,
                            AttackType.explosion);
                    }
                }
                
                hitscanData.PenetratedObjectMaterialsIDs[i] = areaHit.CompareTag("Flesh") ? (byte) 1 : (byte) 0;
                hitscanData.PenetrationPositions[i] = areaHit.ClosestPoint(origin);
            }
            
            gun.VisualUse(hitscanData);
        }
    }
}