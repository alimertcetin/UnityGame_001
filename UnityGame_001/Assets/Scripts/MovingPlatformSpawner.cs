using UnityEngine;

namespace PlayerSystems
{
    public class MovingPlatformSpawner : MonoBehaviour
    {
        [SerializeField] Transform[] spawnPositions;
        [SerializeField] MovingPlatform prefab;
        [SerializeField] float spawnInterval = 3f;
        [SerializeField] float movementDuration = 5f;
        float current;

        void Start()
        {
            Spawn();
        }

        void Update()
        {
            current += Time.deltaTime;
            if (current > spawnInterval)
            {
                current = 0f;
                Spawn();
            }
        }

        void Spawn()
        {
            var pos = spawnPositions.PickRandom().position;
            var movingPlatform = Instantiate(prefab, pos, Quaternion.identity);
            movingPlatform.Init(pos, spawnPositions.GetFarthest(pos).position, Mathf.Clamp(Random.value, 0.5f, 1f) * movementDuration);
        }
    }
}