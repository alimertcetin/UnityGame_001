using System;
using PlayerSystems;
using UnityEngine;
using XIV.Core.TweenSystem;
using XIV.Core.Utils;
using XIV.Core.XIVMath;

namespace TheGame.UI
{
    public class HitPointDisplayer : BehaviourBase
    {
        public static HitPointDisplayer instance;
        [SerializeField] HitPointText hitPointTextPrefab;
        
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
            
            var curve = BezierMath.CreateCurve(position, position + Vector3.up);
            hitPointText.transform.XIVTween()
                .FollowCurve(curve, 0.75f, EasingFunction.EaseOutCubic)
                .OnComplete(() =>
                {
                    Destroy(hitPointText.gameObject);
                })
                .Start();
        }

        protected override int[] GetStates()
        {
            return new[] { GameState.PLAYING };
        }
    }
}