using System;
using PlayerSystems;
using UnityEngine;
using XIV.Core;
using XIV.Core.TweenSystem;
using XIV.Core.Utils;
using XIV.Core.XIVMath;
using Random = UnityEngine.Random;

namespace TheGame.UI
{
    public class HitPointDisplayer : BehaviourBase
    {
        public static HitPointDisplayer instance;
        [SerializeField] HitPointText hitPointTextPrefab;
        static readonly Vector3[] curveBuffer = new Vector3[4];
        
        Camera cam;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            cam = Camera.main;
        }

        public void Display(Vector3 position, float score01)
        {
            var hitPointText = Instantiate(hitPointTextPrefab, position, Quaternion.LookRotation(cam.transform.forward));
            hitPointText.text.text = (score01 * 100f).ToString("F1");
            
            var curve = CreateCurve(position, position + Vector3.up);
#if UNITY_EDITOR
            XIVDebug.DrawBezier(curve, Color.red, 1.5f);
#endif
            hitPointText.transform.XIVTween()
                .FollowCurve(curve, 0.75f, EasingFunction.EaseOutCubic)
                .OnComplete(() =>
                {
                    Destroy(hitPointText.gameObject);
                })
                .Start();
        }

        static Vector3[] CreateCurve(Vector3 start, Vector3 end, float midPointDistance = 1f)
        {
            var mid = Vector3.Lerp(start, end, 0.5f);
            var dirToStart = start - mid;
            var dirToEnd = end - mid;
            curveBuffer[0] = start;
            curveBuffer[1] = mid + (dirToStart * 0.5f) + Random.insideUnitSphere * midPointDistance;
            curveBuffer[2] = mid + (dirToEnd * 0.5f) + Random.insideUnitSphere * midPointDistance;
            curveBuffer[3] = end;
            return curveBuffer;
        }

        protected override int[] GetStates()
        {
            return new[] { GameState.PLAYING };
        }
    }
}