using System.Collections.Generic;
using UnityEngine;
using XIV.Core;

namespace PlayerSystems
{
    public struct TrajectoryCollisionData
    {
        public Vector3 point;
        public Vector3 colliderCenterAtTime;
        public Transform transform;
        public Collider collider;
        public float absoluteCollisionTime;
    }
    
    public static class TrajectoryUtils
    {
        public static void GetPointsNonAlloc(Vector3 startPos, Vector3 velocity, Vector3 accelearion, Vector3[] buffer, int detail, float duration)
        {
            int bufferSize = buffer.Length;
            for (int i = 0; i < detail && i < bufferSize; i++)
            {
                var t = i / (detail - 1f);
                buffer[i] = GetPoint(startPos, velocity, accelearion, t * duration);
            }
        }

        public static void GetCollisionsAtTime(Vector3 startPos, Vector3 velocity, Vector3 acceleration, int detail, float duration, float absoluteTime, List<TrajectoryCollisionData> collisionPoints, List<MovingPlatform> platforms)
        {
            var dt = Time.deltaTime; // for precision
            int platformsLength = platforms.Count;
            for (var i = 0; i < platformsLength; i++)
            {
                var platform = platforms[i];
                var platformCollider = platform.coll;
                
                for (int j = 0; j < detail; j++)
                {
                    var time = (j / (detail - 1f)) * duration;
                    var position = GetPoint(startPos, velocity, acceleration, time);
                    var collisionTime = absoluteTime + time + dt;
                    
                    var platformCenterAtTime = platform.GetPositionAtTime(collisionTime);
                    var bounds = platformCollider.bounds;
                    bounds.center = platformCenterAtTime;
                    var closestPoint = bounds.ClosestPoint(position);
                    
                    var dirToCenter = (platformCenterAtTime - closestPoint).normalized;
                    var dirToPositionAtTime = (position - closestPoint).normalized;
                    var dot = Vector3.Dot(dirToCenter, dirToPositionAtTime);
                    if (dot < 0) continue;
                    
                    if ((closestPoint - position).sqrMagnitude < 0.01f)
                    {
                        collisionPoints.Add(new TrajectoryCollisionData
                        {
                            point = closestPoint,
                            colliderCenterAtTime = platformCenterAtTime,
                            transform = platform.transform,
                            collider = platformCollider,
                            absoluteCollisionTime = collisionTime,
                        });
#if UNITY_EDITOR
                        XIVDebug.DrawCircle(closestPoint, 0.1f, Color.blue, 1f);
                        XIVDebug.DrawBounds(bounds, 1f);
#endif
                    }
                }
                
            }
        }
        
        public static Vector3 GetPoint(Vector3 startPos, Vector3 velocity, Vector3 acceleration, float t)
        {
            return startPos + velocity * t + (acceleration / 2f) * (t * t);
        }

        public static bool IsColliding(Vector3 startPos, Vector3 velocity, Vector3 accelearion, int detail, float duration, int layerMask)
        {
            var buffer = Utils.GetBuffer<Vector3>(detail);
            GetPointsNonAlloc(startPos, velocity, accelearion, buffer, detail, duration);
            for (int i = 0; i < detail - 1; i++)
            {
                if (Physics.Linecast(buffer[i], buffer[i + 1], layerMask))
                {
                    return true;
                }
            }

            buffer.Return();
            return false;
        }
    }
}