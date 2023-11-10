using UnityEngine;

namespace TheGame.ThrowSystems
{
    public struct ThrowObjectHitData
    {
        public Transform throwObject;
        public Vector3 beforeHitPosition;
        public Vector3 afterHitPosition;
    }
}