using UnityEngine;

namespace PlayerSystems
{
    public class GhostOwner : MonoBehaviour
    {
        [SerializeField] Ghost ghost;
        Transform ghostParent;
        bool isRented;

        void Awake()
        {
            ghostParent = ghost.transform.parent;
            ghost.Init(this);
        }

        public Ghost Rent()
        {
            if (isRented)
            {
                Debug.LogError("Ghost is already rented", this);
                return null;
            }
            ghost.transform.SetParent(null);
            isRented = true;
            return ghost;
        }

        public void ReleaseGhost(Ghost ghost)
        {
            if (IsOwnerOf(ghost) == false)
            {
                Debug.LogError("You should release the rented ghost.", this);
                return;
            }

            isRented = false;
            ghost.transform.SetParent(ghostParent);
            ghost.gameObject.SetActive(false);
        }

        public bool IsOwnerOf(Ghost ghost)
        {
            return ghost == this.ghost;
        }
    }
}