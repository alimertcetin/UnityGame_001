using System;
using TheGame;
using UnityEngine;
using XIV.Core.TweenSystem;
using XIV.Core.Utils;

namespace PlayerSystems
{
    public class MovingPlatform : BehaviourBase
    {
        public Collider coll;
        
        Vector3 startPos;
        Vector3 endPos;
        float normalizedTime => timePassed / duration;
        
        float duration;
        float timePassed;

        public void Init(Vector3 start, Vector3 end, float duration)
        {
            startPos = start;
            endPos = end;
            this.duration = duration;
        }

        public void TakeHit()
        {
            transform.CancelTween();
            transform.XIVTween()
                .Scale(Vector3.one, Vector3.one * 0.75f, 0.25f, EasingFunction.EaseOutBounce, true)
                .Start();
        }

        protected override void Awake()
        {
            base.Awake();
            TargetManager.AddTargetToList(this);
        }

        public override void Tick()
        {
            var pos = Vector3.Lerp(startPos, endPos, normalizedTime);
            transform.position = pos;
            timePassed += Time.deltaTime;
            if (normalizedTime > 1f)
            {
                OnReachedTargetPosition();
            }
        }

        protected override int[] GetStates()
        {
            return new[] { GameState.PLAYING };
        }

        public Vector3 GetPositionAtTime(float time)
        {
            var startTime = Time.time - timePassed;
            var passedTime = time - startTime;
            var normalized = passedTime / duration;
            return Vector3.Lerp(startPos, endPos, normalized);
        }

        void OnReachedTargetPosition()
        {
            TargetManager.RemoveTarget(this);
            Destroy(this.gameObject);
        }

        void OnValidate()
        {
            if (coll) return;
            coll = GetComponentInChildren<Collider>();
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(startPos, endPos);
        }
    }
}