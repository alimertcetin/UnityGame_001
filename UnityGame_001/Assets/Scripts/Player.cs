using System;
using UnityEngine;

namespace PlayerSystems
{
    enum PlayerState
    {
        Idle = 0,
        AimBow,
        Release
    }

    public class Player : MonoBehaviour
    {
        [SerializeField] Transform hand;
        [SerializeField] Bow bow;
        
        PlayerState currentState;
        
        void Awake()
        {
            currentState = PlayerState.Idle;
            bow.Init(hand);
        }

        void Update()
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
                bow.StartDraw();
                SetState(PlayerState.AimBow);
            }
        }

        void HandleAim()
        {
            bow.ContinueDrawing();
            bow.DisplayAimIndicator();
            if (Input.GetMouseButtonDown(1))
            {
                bow.StartDraw();
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