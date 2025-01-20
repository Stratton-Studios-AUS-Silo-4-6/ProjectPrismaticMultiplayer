using UnityEngine;

namespace MultiFPS.Gameplay
{
    //Information required to render bullets and hit effects
    public struct Hitscan
    {
        public Vector3[] PenetrationPositions;
        public byte[] PenetratedObjectMaterialsIDs;
        public Quaternion FirstHitRotation;
    }
}