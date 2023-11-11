using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerSystems
{
    [Serializable]
    public class TrajectoryIndicator
    {
        [SerializeField] LineRenderer lineRenderer;
        [SerializeField] GameObject hitIndicator;
        [SerializeField] int detail = 70;
        [Tooltip("Time in seconds")]
        [SerializeField] float duration = 10f;

        Ghost rentedGhost;

        bool active;

        public void Show() => SetState(true);
        public void Hide() => SetState(false);

        /// <summary>
        /// Displays the trajectory
        /// </summary>
        /// <param name="points">Points to display</param>
        /// <param name="pointsLength">Length of the <paramref name="points"/></param>
        /// <param name="hitPossibility">How likely is it to hit</param>
        /// <param name="collisionData"></param>
        public void Display(IList<Vector3> points, int pointsLength, float hitPossibility, TrajectoryCollisionData collisionData)
        {
            SetState(true);
            SetLineRenderer(hitPossibility);
            SetHitIndicator(collisionData, hitPossibility);
            DisplayGhost(collisionData);
            SetPositions(points, pointsLength);
        }

        void SetState(bool v)
        {
            if (active == v) return;
            active = v;
            lineRenderer.enabled = active;
            hitIndicator.SetActive(active);
        }

        void SetLineRenderer(float t)
        {
            lineRenderer.materials[0].color = lineRenderer.colorGradient.Evaluate(t);
        }

        void SetHitIndicator(TrajectoryCollisionData collisionData, float t)
        {
            if (collisionData.transform == false)
            {
                hitIndicator.gameObject.SetActive(false);
                return;
            }

            hitIndicator.SetActive(t > 0.1f);
            hitIndicator.transform.position = collisionData.point;
            hitIndicator.transform.forward = -collisionData.normal;
        }

        void SetPositions(IList<Vector3> points, int count)
        {
            if (lineRenderer.positionCount != count) lineRenderer.positionCount = count;

            for (int i = 0; i < count; i++)
            {
                lineRenderer.SetPosition(i, points[i]);
            }
        }

        void DisplayGhost(TrajectoryCollisionData collisionData)
        {
            if (collisionData.transform == false || collisionData.transform.TryGetComponent<GhostOwner>(out var ghostOwner) == false)
            {
                HandleRent(default);
                return;
            }

            HandleRent(ghostOwner);
            rentedGhost.gameObject.SetActive(true);
            rentedGhost.transform.position = collisionData.colliderCenterAtTime;
        }

        void HandleRent(GhostOwner ghostOwner)
        {
            if (ghostOwner == false)
            {
                if (rentedGhost) rentedGhost.ReturnToOwner();
                
                rentedGhost = default;
                return;
            }
            
            if (rentedGhost == false)
            {
                rentedGhost = ghostOwner.Rent();
                return;
            }

            if (ghostOwner.IsOwnerOf(rentedGhost))
            {
                return;
            }

            rentedGhost.ReturnToOwner();
            rentedGhost = ghostOwner.Rent();
        }
    }
}