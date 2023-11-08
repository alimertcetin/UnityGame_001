using TheGame;
using UnityEngine;
using UnityEngine.Pool;
using XIV.Core.Extensions;
using XIV.Core.TweenSystem;
using XIV.Core.Utils;

namespace PlayerSystems
{
    public class Bow : MonoBehaviour
    {
        [SerializeField] GameObject arrowPrefab;
        [SerializeField] LayerMask hitLayers;
        [SerializeField] LayerMask groundLayer;
        [SerializeField] float maxDraw = 25f;
        [SerializeField] float aimSpeed = 4f;
        [SerializeField] float aimSpeedWhenPossibleHit = 2f;
        [SerializeField] TrajectoryIndicator trajectoryIndicator;
        float currentDraw;
        
        Vector3 acceleration => Physics.gravity;
        
        ObjectPool<ThrowObject> throwableObjectPool;
        Transform hand;

        float minDraw => maxDraw * 0.35f;

        void Awake()
        {
            throwableObjectPool = new ObjectPool<ThrowObject>(CreateThrowable, OnGetThrowable, OnReleaseThrowable);
        }

        public void Init(Transform hand) => this.hand = hand;

        public void StartDraw() => ResetBow();

        public void ContinueDrawing()
        {
            var speed = IsCollidingAOT() ? aimSpeedWhenPossibleHit : aimSpeed;
            currentDraw += Time.deltaTime * speed;
            currentDraw = Clamp(currentDraw, maxDraw, minDraw);
        }

        static float Clamp(float current, float max, float min)
        {
            while (Mathf.PingPong(current, max) < min)
            {
                current += 0.1f;
            }

            return current;
        }

        public void Release()
        {
            var velocity = GetVelocity(currentDraw, maxDraw);
            int mask = hitLayers | groundLayer;
            throwableObjectPool.Get().Init(velocity, Physics.gravity, mask, OnHit);
            ResetBow();
        }

        public void DisplayAimIndicator()
        {
            trajectoryIndicator.SetCollidingState(IsCollidingAOT());
            trajectoryIndicator.Display(hand.position, GetVelocity(currentDraw, maxDraw), acceleration);
        }

        void ResetBow() => currentDraw = minDraw;

        void OnHit(ThrowObject throwObject, Vector3 beforeHitPos, Vector3 targetPos)
        {
            var p = throwObject.transform.position;
            var buffer = Utils.GetBuffer<RaycastHit>(2);
            var diff = targetPos - beforeHitPos;
            int mask = hitLayers | groundLayer;
            var length = Physics.RaycastNonAlloc(beforeHitPos, diff.normalized, buffer, diff.magnitude, mask);
            var closest = buffer.GetClosest(p, length);
            buffer.Return();
            if (closest)
            {
                var throwableTransform = throwObject.transform;
                throwableTransform.SetParent(closest.transform, true);
                throwObject.enabled = false;
                if (closest.GetComponent<MovingPlatform>())
                {
                    closest.XIVTween()
                        .RendererColor(Color.white, Color.black, 0.25f, EasingFunction.Spring, true)
                        .And()
                        .Scale(Vector3.one, Vector3.one * 0.75f, 0.25f, EasingFunction.EaseOutBounce, true)
                        .Start();
                }
            }
            else
            {
                throwableObjectPool.Release(throwObject);
            }
        }

        ThrowObject CreateThrowable() => Instantiate(arrowPrefab).AddComponent<ThrowObject>();

        void OnGetThrowable(ThrowObject obj)
        {
            obj.transform.position = hand.position;
            obj.gameObject.SetActive(true);
        }

        void OnReleaseThrowable(ThrowObject obj)
        {
            obj.gameObject.SetActive(false);
        }

        bool IsCollidingAOT()
        {
            var detail = 2048;
            var duration = 10f;
            var handPos = hand.position;
            var velocity = GetVelocity(currentDraw, maxDraw);
            var list = ListPool<Vector3>.Get();
            TrajectoryUtils.GetCollisionsAtTime(handPos, velocity, acceleration, detail, duration, Time.time, list, TargetManager.targets);
            bool isColliding = list.Count > 0;
            ListPool<Vector3>.Release(list);
            return isColliding;
        }

        static Vector3 GetVelocity(float currentDraw, float maxDraw)
        {
            var drawAmount = Mathf.PingPong(currentDraw, maxDraw);
            return GetDirection(drawAmount / maxDraw) * drawAmount;
        }

        static Vector3 GetDirection(float t)
        {
            var t0 = t > 0.5f ? 1f - (t - 0.5f) / 0.5f : t / 0.5f;
            var v0 = Vector3.Lerp(Vector3.zero, Vector3.forward, t0);
            var v1 = Vector3.Lerp(Vector3.down, Vector3.up, t);
            return (v0 + v1).normalized;
        }
    }
}