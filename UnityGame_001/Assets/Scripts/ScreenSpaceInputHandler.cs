using UnityEngine;

namespace PlayerSystems
{
    struct ScreenSpaceInputHandler
    {
        Vector3 inputPosition;
        Vector3 inputPreviousPosition;

        public void Update()
        {
            inputPreviousPosition = inputPosition;
#if UNITY_ANDROID
            if (Input.touchCount > 0) inputPosition = Input.GetTouch(0).position;
            else inputPosition = Vector3.one * 0.5f; // screen center
#else
            inputPosition = Input.mousePosition;
#endif
        }
        
        public Vector3 GetDelta()
        {
            return inputPosition - inputPreviousPosition;
        }
        
        public Vector3 GetDeltaNormalized()
        {
            return (inputPosition - inputPreviousPosition).normalized;
        }

        public Vector3 GetInputInPixelCoord()
        {
            return inputPosition;
        }

        public Vector3 GetInputNormalized()
        {
            var input = inputPosition;
            input.x /= Screen.width;
            input.y /= Screen.height;
            return input;
        }
    }
}