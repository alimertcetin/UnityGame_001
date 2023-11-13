using System;
using PlayerSystems;
using UnityEngine;

namespace TheGame.CameraSystems
{
    [RequireComponent(typeof(Camera))]
    public class MovingPlatformCamera : BehaviourBase
    {
        public static MovingPlatformCamera instance;
        [SerializeField] Vector3 offset;
        bool hasTarget;
        Transform target;
        Camera cam;
        float camDefaultDepth;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            cam = GetComponent<Camera>();
            camDefaultDepth = cam.depth;
            gameObject.SetActive(false);
        }

        protected override int[] GetStates()
        {
            return new[]
            {
                GameState.PLAYING,
                GameState.ARROW_RELEASED,
            };
        }

        public override void Tick()
        {
            if (hasTarget == false) return;
            if (target == false)
            {
                Hide();
                return;
            }

            var pos = target.position;
            var targetPos = pos + offset;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 10f * Time.deltaTime);
            transform.LookAt(pos);
        }

        public void Show(Transform movingPlatform)
        {
            hasTarget = true;
            target = movingPlatform;
            gameObject.SetActive(true);
            transform.position = target.position + offset;
            cam.depth = 10f;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            cam.depth = camDefaultDepth;
        }
    }
}
