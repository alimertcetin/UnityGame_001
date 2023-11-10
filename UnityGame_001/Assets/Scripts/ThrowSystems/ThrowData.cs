using UnityEngine;

namespace TheGame.ThrowSystems
{
    public readonly struct ThrowData
    {
        public readonly Vector3 startPos;
        public readonly Vector3 velocity;
        public readonly Vector3 acceleration;
        public readonly int collisionLayers;

        public ThrowData(Vector3 startPos, Vector3 velocity, Vector3 acceleration, int collisionLayers)
        {
            this.startPos = startPos;
            this.velocity = velocity;
            this.acceleration = acceleration;
            this.collisionLayers = collisionLayers;
        }
    }
}