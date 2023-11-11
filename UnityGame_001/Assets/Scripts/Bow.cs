using System.Collections.Generic;
using TheGame;
using TheGame.CameraSystems;
using TheGame.ThrowSystems;
using TheGame.UI;
using UnityEngine;
using UnityEngine.Pool;
using XIV.Core.TweenSystem;

namespace PlayerSystems
{
    public class Bow : BehaviourBase
    {
        [SerializeField] GameObject arrowPrefab;
        [SerializeField] LayerMask hitLayers;
        [SerializeField] LayerMask groundLayer;
        [SerializeField] float maxDraw = 40f;
        [SerializeField] FloatingJoystick inputHandler;
        [SerializeField] TrajectoryIndicator trajectoryIndicator;
        [SerializeField] GameSettingsChannelSO gameSettingsLoadedChannel;
        [SerializeField] bool dontAllowInvalidShots;
        [Range(0f, 1f)]
        [SerializeField] float hitIndicatorPrecision = 0.1f;
        
        Vector3 acceleration => Physics.gravity;
        
        ObjectPool<Transform> throwableObjectPool;
        Transform hand;
        GameSettings gameSettings;

        Vector2 inputFollowPosition;
        const int detail = 2048;
        const float duration = 10f;

        protected override void Awake()
        {
            base.Awake();
            inputFollowPosition = Vector2.up * 0.75f + Vector2.right * 0.5f;
            throwableObjectPool = new ObjectPool<Transform>(CreateThrowable, OnGetThrowable, OnReleaseThrowable);
        }

        protected override int[] GetStates()
        {
            return new[] { GameState.PLAYING };
        }

        void OnEnable() => gameSettingsLoadedChannel.Register(OnGameSettingsLoaded);
        void OnDisable() => gameSettingsLoadedChannel.Unregister(OnGameSettingsLoaded);
        void OnGameSettingsLoaded(GameSettings obj) => gameSettings = obj;

        public void Init(Transform hand) => this.hand = hand;
        
        public void ContinueDrawing()
        {
            var speed = IsCollidingAOT() ? gameSettings.possibleHitSensitivity : gameSettings.normalSensitivity;
            inputFollowPosition += (inputHandler.Direction * (speed * Time.deltaTime));
            inputFollowPosition.x = Mathf.Clamp01(inputFollowPosition.x);
            inputFollowPosition.y = Mathf.Clamp01(inputFollowPosition.y);
        }

        void Update()
        {
            if (dontAllowInvalidShots)
            {
                Release();
            }
        }

        public void Release()
        {
            var list = ListPool<TrajectoryCollisionData>.Get();
            bool isColliding = IsCollidingAOT(list);
            
            if (isColliding == false && dontAllowInvalidShots)
            {
                ListPool<TrajectoryCollisionData>.Release(list);
                return;
            }

            var velocity = GetVelocity();
            int mask = hitLayers | groundLayer;
            var go = throwableObjectPool.Get().gameObject;
            var throwData = new ThrowData(hand.position, velocity, acceleration, mask);
            ThrowSystem.Throw(go, throwData, OnHit);

            if (isColliding)
            {
                var trajectoryCollisionData = list[^1];
                if (Vector3.Distance(trajectoryCollisionData.colliderCenterAtTime, trajectoryCollisionData.point) < hitIndicatorPrecision)
                {
                    var instance = MovingPlatformCamera.instance;
                    instance.CancelTween(false);
                    instance.Show(trajectoryCollisionData.transform);

                    var travelTime = trajectoryCollisionData.absoluteCollisionTime - Time.time;
                    instance.XIVTween()
                        .Wait(travelTime + 1f)
                        .OnComplete(instance.Hide)
                        .Start();
                }
            }
            
            
            ListPool<TrajectoryCollisionData>.Release(list);
        }

        public void DisplayAimIndicator()
        {
            var list = ListPool<TrajectoryCollisionData>.Get();
            bool isColliding = IsCollidingAOT(list);
            float hitPossibility = 0f;
            TrajectoryCollisionData trajectoryCollisionData = default;
            if (isColliding)
            {
                trajectoryCollisionData = list[^1];
                var distance = Vector3.Distance(trajectoryCollisionData.colliderCenterAtTime, trajectoryCollisionData.point);
                hitPossibility = 1f - Mathf.Clamp01(distance / hitIndicatorPrecision);
            }

            DisplayIndicator(hitPossibility, trajectoryCollisionData);
            ListPool<TrajectoryCollisionData>.Release(list);
        }
        
        void DisplayIndicator(float hitPossibility, TrajectoryCollisionData collisionData)
        {
            var buffer = Utils.GetBuffer<Vector3>(detail);
            TrajectoryUtils.GetPointsNonAlloc(hand.position, GetVelocity(), acceleration, buffer, detail, duration);
            trajectoryIndicator.Display(buffer, detail, hitPossibility, collisionData);
            buffer.Return();
        }

        void OnHit(ThrowObjectHitData hitData)
        {
            Transform throwObject = hitData.throwObject;
            Vector3 beforeHitPos = hitData.beforeHitPosition;
            Vector3 targetPos = hitData.afterHitPosition;

            var throwObjectPosition = throwObject.position;
            var buffer = Utils.GetBuffer<RaycastHit>(2);
            var diff = targetPos - beforeHitPos;
            int mask = hitLayers | groundLayer;
            var length = Physics.RaycastNonAlloc(beforeHitPos, diff.normalized, buffer, diff.magnitude, mask);
            var closest = buffer.GetClosest(throwObjectPosition, length);
            if (closest.transform)
            {
                throwObject.SetParent(closest.transform, true);
                var movingPlatform = closest.transform.GetComponentInParent<MovingPlatform>();
                if (movingPlatform)
                {
                    var distance = Vector3.Distance(closest.collider.bounds.center, throwObjectPosition);
                    var score = 1f - Mathf.Clamp01(distance / hitIndicatorPrecision);
                    if (score > 0.1f) HitPointDisplayer.instance.Display(hitData.throwObject.position, score);
                    movingPlatform.TakeHit();
                }
            }
            else
            {
                throwableObjectPool.Release(throwObject);
            }
            buffer.Return();
        }

        Transform CreateThrowable()
        {
            return Instantiate(arrowPrefab).transform;
        }

        void OnGetThrowable(Transform obj)
        {
            obj.transform.position = hand.position;
            obj.gameObject.SetActive(true);
        }

        void OnReleaseThrowable(Transform obj)
        {
            obj.gameObject.SetActive(false);
        }

        bool IsCollidingAOT()
        {
            var list = ListPool<TrajectoryCollisionData>.Get();
            bool isColliding = IsCollidingAOT(list);
            ListPool<TrajectoryCollisionData>.Release(list);
            return isColliding;
        }

        bool IsCollidingAOT(List<TrajectoryCollisionData> collisionDataBuffer)
        {
            var handPos = hand.position;
            var velocity = GetVelocity();
            TrajectoryUtils.GetCollisionsAtTime(handPos, velocity, acceleration, detail, duration, Time.time, collisionDataBuffer, TargetManager.targets);
            bool isColliding = collisionDataBuffer.Count > 0;
            return isColliding;
        }

        Vector3 GetVelocity()
        {
            static Vector3 GetDirection(float t)
            {
                var t0 = t > 0.5f ? 1f - (t - 0.5f) / 0.5f : t / 0.5f;
                var v0 = Vector3.Lerp(Vector3.zero, Vector3.forward, t0);
                var v1 = Vector3.Lerp(Vector3.down, Vector3.up, t);
                return (v0 + v1).normalized;
            }
            
            var inputPos = inputFollowPosition;
            var currDrawY = Mathf.Lerp(-maxDraw, maxDraw, inputPos.y);
            var currDrawX = Mathf.Lerp(-maxDraw, maxDraw, inputPos.x);
            return ((GetDirection(currDrawY / maxDraw) * currDrawY) + Vector3.right * currDrawX);
        }
    }
}