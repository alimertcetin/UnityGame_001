using TheGame;
using UnityEngine;
using XIV.Core.TweenSystem;
using XIV.Core.Utils;

namespace PlayerSystems
{
    public class MovingPlatformSpawner : BehaviourBase
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

        public override void Tick()
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
            movingPlatform.transform.rotation = Quaternion.AngleAxis(90f, Vector3.right);
            movingPlatform.enabled = false;
            var duration = 0.75f;
            var durationHalf = duration * 0.5f;
            movingPlatform.XIVTween()
                .Move(pos, pos + Vector3.up, durationHalf, EasingFunction.EaseOutExpo)
                .And()
                .RotateX(90f, 0f, 0.75f, EasingFunction.EaseOutExpo)
                .Move(pos + Vector3.up, pos, durationHalf, EasingFunction.EaseOutExpo)
                .OnComplete(() =>
                {
                    movingPlatform.enabled = true;
                    movingPlatform.Init(pos, spawnPositions.GetFarthest(pos).position, Mathf.Clamp(Random.value, 0.5f, 1f) * movementDuration);
                })
                .Start();
        }

        protected override int[] GetStates()
        {
            return new[] { GameState.PLAYING };
        }
    }
}