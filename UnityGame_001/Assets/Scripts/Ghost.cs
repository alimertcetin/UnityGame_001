using UnityEngine;

namespace PlayerSystems
{
    public class Ghost : MonoBehaviour
    {
        GhostOwner owner;

        public void Init(GhostOwner owner)
        {
            this.owner = owner;
        }

        public void ReturnToOwner()
        {
            owner.ReleaseGhost(this);
        }
    }
}