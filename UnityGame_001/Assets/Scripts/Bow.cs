using System;
using TheGame;
using TheGame.ThrowSystems;
using UnityEngine;
using UnityEngine.Pool;
using XIV.Core.TweenSystem;
using XIV.Core.Utils;

namespace PlayerSystems
{
    public class Bow : BehaviourBase
    {
        [SerializeField] GameObject arrowPrefab;
        [SerializeField] LayerMask hitLayers;
        [SerializeField] LayerMask groundLayer;
        [SerializeField] float maxDraw = 40f;
        [SerializeField] TrajectoryIndicator trajectoryIndicator;
        [SerializeField] GameSettingsChannelSO gameSettingsLoadedChannel;
        [SerializeField] bool dontAllowInvalidShots;
        
        Vector3 acceleration => Physics.gravity;
        
        ObjectPool<Transform> throwableObjectPool;
        Transform hand;
        GameSettings gameSettings;

        ScreenSpaceInputHandler inputHandler;
        Vector3 inputFollowPosition;

        protected override void Awake()
        {
            base.Awake();
            inputFollowPosition = Vector3.up * 0.75f + Vector3.right * 0.5f;
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
            inputHandler.Update();
            var speed = IsCollidingAOT() ? gameSettings.possibleHitSensitivity : gameSettings.normalSensitivity;
            inputFollowPosition += inputHandler.GetDeltaNormalized() * (speed * Time.deltaTime);
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
            if (IsCollidingAOT() == false && dontAllowInvalidShots) return;
            var velocity = GetVelocity();
            int mask = hitLayers | groundLayer;
            ThrowSystem.Throw(throwableObjectPool.Get().gameObject, new ThrowData(hand.position, velocity, acceleration, mask), OnHit);
        }

        public void DisplayAimIndicator()
        {
            trajectoryIndicator.SetCollidingState(IsCollidingAOT());
            trajectoryIndicator.Display(hand.position, GetVelocity(), acceleration);
        }

        void OnHit(ThrowObjectHitData hitData)
        {
            Transform throwObject = hitData.throwObject;
            Vector3 beforeHitPos = hitData.beforeHitPosition;
            Vector3 targetPos = hitData.afterHitPosition;

            var p = throwObject.position;
            var buffer = Utils.GetBuffer<RaycastHit>(2);
            var diff = targetPos - beforeHitPos;
            int mask = hitLayers | groundLayer;
            var length = Physics.RaycastNonAlloc(beforeHitPos, diff.normalized, buffer, diff.magnitude, mask);
            var closest = buffer.GetClosest(p, length);
            buffer.Return();
            if (closest)
            {
                throwObject.SetParent(closest.transform, true);
                var movingPlatform = closest.GetComponentInParent<MovingPlatform>();
                if (movingPlatform)
                {
                    movingPlatform.TakeHit();
                }
            }
            else
            {
                throwableObjectPool.Release(throwObject);
            }
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
            var detail = 2048;
            var duration = 10f;
            var handPos = hand.position;
            var velocity = GetVelocity();
            var list = ListPool<Vector3>.Get();
            TrajectoryUtils.GetCollisionsAtTime(handPos, velocity, acceleration, detail, duration, Time.time, list, TargetManager.targets);
            bool isColliding = list.Count > 0;
            ListPool<Vector3>.Release(list);
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