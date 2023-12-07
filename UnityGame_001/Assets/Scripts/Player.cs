using System;
using TheGame;
using UnityEngine;
using XIV.Core.Extensions;

namespace PlayerSystems
{
    enum PlayerState
    {
        Idle = 0,
        AimBow,
        Release
    }

    public class Player : BehaviourBase
    {
        [SerializeField] Transform hand;
        [SerializeField] Bow bow;
        [SerializeField] Animator animator;
        
        PlayerState currentState;
        static readonly int StateParameterID = AnimationConstants.ArcherController.Parameters.ArcherController_State_IntID;
        
        protected override void Awake()
        {
            base.Awake();
            currentState = PlayerState.Idle;
            bow.Init(hand);
        }

        protected override int[] GetStates()
        {
            return new[]
            {
                GameState.PLAYING,
            };
        }

        public override void Tick()
        {
            switch (currentState)
            {
                case PlayerState.Idle:
                    HandleIdle();
                    break;
                case PlayerState.AimBow:
                    HandleAim();
                    break;
                case PlayerState.Release:
                    HandleRelease();
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        void HandleIdle()
        {
            if (Input.GetMouseButtonDown(0))
            {
                SetState(PlayerState.AimBow);
            }
        }

        void HandleAim()
        {
            if (animator.IsPlaying(AnimationConstants.ArcherController.Clips.ArcherController_Aim_IdleHash) == false)
            {
                if (Input.GetMouseButton(0) == false)
                {
                    SetState(PlayerState.Idle);
                }
                return;
            }

            bow.ContinueDrawing();
            bow.DisplayAimIndicator();
            if (Input.GetMouseButtonDown(1))
            {
                SetState(PlayerState.Idle);
            }
            if (Input.GetMouseButton(0) == false)
            {
                SetState(PlayerState.Release);
            }
        }

        void HandleRelease()
        {
            bow.Release();
            SetState(PlayerState.Idle);
        }

        void SetState(PlayerState newState)
        {
            currentState = newState;
            animator.SetInteger(StateParameterID, (int)newState);
        }
    }
}