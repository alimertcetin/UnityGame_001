using UnityEngine;

namespace PlayerSystems
{
    public class TrajectoryIndicator : MonoBehaviour
    {
        [SerializeField] LineRenderer lineRenderer;
        [SerializeField] int detail = 70;
        [Tooltip("Time in seconds")]
        [SerializeField] float duration = 10f;

        public void SetCollidingState(bool isColliding)
        {
            lineRenderer.materials[0].color = isColliding ? Color.green : Color.white;
        }

        public void Display(Vector3 startPos, Vector3 velocity, Vector3 acceleration)
        {
            if (lineRenderer.positionCount != detail) lineRenderer.positionCount = detail;

            var buffer = Utils.GetBuffer<Vector3>(detail);
            TrajectoryUtils.GetPointsNonAlloc(startPos, velocity, acceleration, buffer, detail, duration);
            for (int i = 0; i < detail; i++)
            {
                lineRenderer.SetPosition(i, buffer[i]);
            }
            buffer.Return();
        }
    }
}