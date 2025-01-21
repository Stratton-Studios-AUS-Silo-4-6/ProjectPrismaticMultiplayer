using UnityEngine;

namespace MultiFPS.Gameplay
{
    //Information required to render bullets and hit effects
    public struct Hitscan
    {
        public Vector3[] PenetrationPositions;
        public byte[] PenetratedObjectMaterialsIDs;
        public Quaternion FirstHitRotation;

        public static Hitscan Fire(Gun gun, int damage)
        {
            var owner = gun.MyOwner;
            Quaternion hitRotation = Quaternion.identity;
            RaycastHit[] hitScan = GameTools.HitScan(gun.FirePoint, owner.transform, GameManager.fireLayer, 250f);

            int penetratedObjects = 0;

            Vector3[] penetrationPositions;
            byte[] penetratedObjectMaterialsIDs;

            //we hit something, so we have to check what to do next
            if (hitScan.Length > 0)
            {
                hitRotation = Quaternion.FromToRotation(Vector3.forward, hitScan[0].normal);

                for (int i = 0; i < Mathf.Min(hitScan.Length, gun.Penetration); i++)
                {
                    penetratedObjects++;

                    RaycastHit currentHit = hitScan[i];
                    GameObject go = currentHit.collider.gameObject;
                    HitBox hb = go.GetComponent<HitBox>();
                    if (hb)
                    {
                        if (!owner.BOT)
                        {
                            gun.CmdDamage(hb._health.DNID, hb.part, damage, 1f / (i + 1), i == 0 ? AttackType.hitscan : AttackType.hitscanPenetrated); //the more objects we penetrated the less damage we deal
                        }
                        else
                        {
                            gun.ServerDamage(hb._health, hb.part, damage, 1f / (i + 1), i == 0 ? AttackType.hitscan : AttackType.hitscanPenetrated);
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

                    byte matID = 0;
                    switch (hitScan[i].collider.tag)
                    {
                        case "Flesh":
                            matID = 1;
                            break;
                    }

                    penetratedObjectMaterialsIDs.SetValue(matID, i);
                }
            }
            else 
            {
                penetrationPositions = new Vector3[1] { gun.FirePoint.forward * 99999f };
                penetratedObjectMaterialsIDs = new byte[0];
            }

            return new Hitscan
            {
                PenetrationPositions = penetrationPositions,
                PenetratedObjectMaterialsIDs = penetratedObjectMaterialsIDs,
                FirstHitRotation = hitRotation,
            } ;
        }
    }
}