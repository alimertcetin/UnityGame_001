using TheGame;
using UnityEngine;
using UnityEngine.Pool;
using XIV.Core.TweenSystem;
using XIV.Core.Utils;

namespace PlayerSystems
{
    public class Bow : MonoBehaviour
    {
        [SerializeField] GameObject arrowPrefab;
        [SerializeField] LayerMask hitLayers;
        [SerializeField] LayerMask groundLayer;
        [SerializeField] float maxDraw = 40f;
        [SerializeField] TrajectoryIndicator trajectoryIndicator;
        
        Vector3 acceleration => Physics.gravity;
        
        ObjectPool<ThrowObject> throwableObjectPool;
        Transform hand;
        
        [Range(0, 1)]
        [SerializeField] float followInputSpeed = 1f;
        [Range(0, 1)]
        [SerializeField] float followInputOnPossibleHitSpeed = 0.5f;

        ScreenSpaceInputHandler inputHandler;
        Vector3 inputFollowPosition;

        void Awake()
        {
            inputFollowPosition = Vector3.one * 0.5f;
            throwableObjectPool = new ObjectPool<ThrowObject>(CreateThrowable, OnGetThrowable, OnReleaseThrowable);
        }

        public void Init(Transform hand) => this.hand = hand;
        
        public void ContinueDrawing()
        {
            inputHandler.Update();
            var speed = IsCollidingAOT() ? followInputOnPossibleHitSpeed : followInputSpeed;
            inputFollowPosition += inputHandler.GetDeltaNormalized() * (speed * Time.deltaTime);
            inputFollowPosition.x = Mathf.Clamp01(inputFollowPosition.x);
            inputFollowPosition.y = Mathf.Clamp01(inputFollowPosition.y);
        }

        public void Release()
        {
            var velocity = GetVelocity();
            int mask = hitLayers | groundLayer;
            throwableObjectPool.Get().Init(velocity, acceleration, mask, OnHit);
        }

        public void DisplayAimIndicator()
        {
            trajectoryIndicator.SetCollidingState(IsCollidingAOT());
            trajectoryIndicator.Display(hand.position, GetVelocity(), acceleration);
        }

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
            var velocity = GetVelocity();
            var list = ListPool<Vector3>.Get();
            TrajectoryUtils.GetCollisionsAtTime(handPos, velocity, acceleration, detail, duration, Time.time, list, TargetManager.targets);
            bool isColliding = list.Count > 0;
            ListPool<Vector3>.Release(list);
            return isColliding;
        }

        Vector3 GetVelocity()
        {
            var inputPos = inputFollowPosition;
            var currDrawY = Mathf.Lerp(-maxDraw, maxDraw, inputPos.y);
            var currDrawX = Mathf.Lerp(-maxDraw, maxDraw, inputPos.x);
            return ((GetDirection(currDrawY / maxDraw) * currDrawY) + Vector3.right * currDrawX);
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