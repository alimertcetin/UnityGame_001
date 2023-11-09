using System;
using TheGame;
using UnityEngine;
using UnityEngine.Pool;

namespace PlayerSystems
{
    public class ThrowObject : BehaviourBase
    {
        Vector3 startPos;
        Vector3 velocity;
        Vector3 acceleration;
        float timePassed;
        int layerMask;
        Action<ThrowObject, Vector3, Vector3> onHit;

        public void Init(Vector3 velocity, Vector3 acceleration, int layerMask, Action<ThrowObject, Vector3, Vector3> onHit)
        {
            startPos = transform.position;
            this.velocity = velocity;
            this.acceleration = acceleration;
            this.timePassed = 0f;
            this.layerMask = layerMask;
            this.onHit = onHit;
        }

        public override void Tick()
        {
            var currentPosition = transform.position;
            var nextPosition = TrajectoryUtils.GetPoint(startPos, velocity, acceleration, timePassed);
            timePassed += Time.deltaTime;

            if (Physics.Linecast(currentPosition, nextPosition, out var hitInfo, layerMask))
            {
                SetPosition(hitInfo.point);
                onHit.Invoke(this, currentPosition, nextPosition);
                return;
            }

            if (currentPosition.y < -15f)
            {
                onHit.Invoke(this, currentPosition, nextPosition);
                return;
            }

            SetPosition(nextPosition);
        }

        void SetPosition(Vector3 nextPosition)
        {
            var t = transform;
            var currentPos = t.position;
            t.forward = nextPosition - currentPos;
            t.position = nextPosition;
        }

        protected override int[] GetStates()
        {
            return new[] { GameState.PLAYING };
        }

        void OnDrawGizmos()
        {
            if (enabled == false) return;
            var list = ListPool<Vector3>.Get();

            const int DETAIL = 128;
            const float DURATION = 10f; // time in seconds
            
            for (int i = 0; i < DETAIL; i++)
            {
                var t = i / (DETAIL - 1f);
                var time = t * DURATION;
                Vector3 point = TrajectoryUtils.GetPoint(startPos, velocity, acceleration, time);
                list.Add(point);
            }

            for (int i = 0; i < DETAIL - 1; i++)
            {
                Gizmos.DrawLine(list[i], list[i + 1]);
            }
            
            ListPool<Vector3>.Release(list);
        }
    }
}