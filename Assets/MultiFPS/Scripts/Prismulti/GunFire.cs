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

        protected Hitscan FireHitscan(int damage)
        {
            var owner = gun.MyOwner;
            var firepoint = gun.FirePoint;
            var hitRotation = Quaternion.identity;
            var hitScan = GameTools.HitScan(firepoint, owner.transform, GameManager.fireLayer);

            var penetratedObjects = 0;

            Vector3[] penetrationPositions;
            byte[] penetratedObjectMaterialsIDs;

            //we hit something, so we have to check what to do next
            if (hitScan.Length > 0)
            {
                hitRotation = Quaternion.FromToRotation(Vector3.forward, hitScan[0].normal);

                for (int i = 0; i < Mathf.Min(hitScan.Length, gun.Penetration); i++)
                {
                    penetratedObjects++;

                    var currentHit = hitScan[i];
                    var go = currentHit.collider.gameObject;
                    var hb = go.GetComponent<HitBox>();
                    if (hb)
                    {
                        if (!owner.BOT)
                        {
                            gun.CmdDamage(
                                hb._health.DNID,
                                hb.part,
                                damage,
                                1f / (i + 1),
                                i == 0 ? AttackType.hitscan : AttackType.hitscanPenetrated
                                ); //the more objects we penetrated the less damage we deal
                        }
                        else
                        {
                            gun.ServerDamage(
                                hb._health,
                                hb.part,
                                damage,
                                1f / (i + 1),
                                i == 0 ? AttackType.hitscan : AttackType.hitscanPenetrated
                                );
                        }
                    }

                    if (go.layer == 0) //if we hitted solid wall, dont penetrate it further
                        break;
                }

                penetrationPositions = new Vector3[penetratedObjects];
                penetratedObjectMaterialsIDs = new byte[penetratedObjects];
                //material detection for appropriate particle impact effect
                for (int i = 0; i < penetratedObjects; i++)
                {
                    penetrationPositions.SetValue(hitScan[i].point, i);

                    byte matID = hitScan[i].collider.tag switch
                    {
                        "Flesh" => 1,
                        _ => 0
                    };

                    penetratedObjectMaterialsIDs.SetValue(matID, i);
                }
            }
            else
            {
                penetrationPositions = new Vector3[1] { firepoint.forward * 99999f };
                penetratedObjectMaterialsIDs = new byte[0];
            }
            
            return new Hitscan
            {
                PenetrationPositions = penetrationPositions,
                PenetratedObjectMaterialsIDs = penetratedObjectMaterialsIDs,
                FirstHitRotation = hitRotation,
            };
        }
    }
}