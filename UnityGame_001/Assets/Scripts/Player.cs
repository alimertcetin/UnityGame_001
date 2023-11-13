using System;
using TheGame;
using UnityEngine;

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
        
        PlayerState currentState;
        
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
            bow.DisplayAimIndicator();
            if (Input.GetMouseButtonDown(0))
            {
                SetState(PlayerState.AimBow);
            }
        }

        void HandleAim()
        {
            bow.ContinueDrawing();
            bow.DisplayAimIndicator();
            if (Input.GetMouseButtonDown(1))
            {
                bow.DisplayAimIndicator();
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
        }
    }
}