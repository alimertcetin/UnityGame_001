using System.Buffers;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerSystems
{
    public static class Utils
    {
        public static T PickRandom<T>(this T[] arr, int length)
        {
            return arr[UnityEngine.Random.Range(0, length)];
        }
        
        public static T PickRandom<T>(this T[] arr)
        {
            return arr[UnityEngine.Random.Range(0, arr.Length)];
        }

        public static T GetClosest<T>(this T[] arr, Vector3 position) where T : Component
        {
            return GetClosest(arr, position, arr.Length);
        }

        public static T GetClosest<T>(this T[] arr, Vector3 position, int length) where T : Component
        {
            T closest = default;
            var dist = float.MaxValue;
            for (var i = 0; i < length; i++)
            {
                T obj = arr[i];
                var d = (obj.transform.position - position).sqrMagnitude;
                if (d < dist)
                {
                    closest = obj;
                    dist = d;
                }
            }

            return closest;
        }

        public static RaycastHit GetClosest(this RaycastHit[] arr, Vector3 position)
        {
            return GetClosest(arr, position, arr.Length);
        }

        public static RaycastHit GetClosest(this RaycastHit[] arr, Vector3 position, int length)
        {
            RaycastHit closest = default;
            var dist = float.MaxValue;
            for (var i = 0; i < length; i++)
            {
                var hit = arr[i];
                var d = (hit.point - position).sqrMagnitude;
                if (d < dist)
                {
                    closest = hit;
                    dist = d;
                }
            }

            return closest;
        }
        
        public static T GetFarthest<T>(this T[] arr, Vector3 position) where T : Component
        {
            return GetFarthest(arr, position, arr.Length);
        }

        public static T GetFarthest<T>(this T[] arr, Vector3 position, int length) where T : Component
        {
            T farthest = default;
            var dist = float.MinValue;
            for (var i = 0; i < length; i++)
            {
                T obj = arr[i];
                var d = (obj.transform.position - position).sqrMagnitude;
                if (d > dist)
                {
                    farthest = obj;
                    dist = d;
                }
            }

            return farthest;
        }

        public static T[] GetBuffer<T>(int size = 4)
        {
            return ArrayPool<T>.Shared.Rent(size);
        }

        public static void Return<T>(this T[] arr)
        {
            ArrayPool<T>.Shared.Return(arr);
        }

        /// <summary>
        /// Cache <typeparamref name="T0"/> to <typeparamref name="T1"/>
        /// </summary>
        /// <param name="from">Components to cache</param>
        /// <param name="buffer">Buffer to hold cached values</param>
        /// <exception cref="System.ArgumentOutOfRangeException">If there is no place to hold cached values in <paramref name="buffer"/></exception>
        public static void CacheType<T0, T1>(IList<T0> from, T1[] buffer)
            where T0 : Component
            where T1 : Component
        {
            int arrLength = from.Count;
            int bufferLength = buffer.Length;
            if (bufferLength < arrLength) throw new System.ArgumentOutOfRangeException(nameof(buffer));
            for (int i = 0; i < arrLength; i++)
            {
                buffer[i] = from[i].GetComponent<T1>();
            }
        }

        /// <summary>
        /// Cache <typeparamref name="T0"/> to <typeparamref name="T1"/>
        /// </summary>
        /// <param name="from">Components to cache</param>
        /// <param name="buffer">Buffer to hold cached values</param>
        /// <exception cref="System.ArgumentOutOfRangeException">If there is no place to hold cached values in <paramref name="buffer"/></exception>
        public static void CacheTypeFromChild<T0, T1>(IList<T0> from, T1[] buffer)
            where T0 : Component
            where T1 : Component
        {
            int arrLength = from.Count;
            int bufferLength = buffer.Length;
            if (bufferLength < arrLength) throw new System.ArgumentOutOfRangeException(nameof(buffer));
            for (int i = 0; i < arrLength; i++)
            {
                buffer[i] = from[i].GetComponentInChildren<T1>();
            }
        }

        public static int GetHits(RaycastHit[] buffer, int bufferLength, IList<Vector3> points, int pointsLength, int layerMask)
        {
            int count = 0;
            for (int i = 0; i < pointsLength - 1 && i < bufferLength; i++)
            {
                if (Physics.Linecast(points[i], points[i + 1], out var hitInfo, layerMask))
                {
                    buffer[count++] = hitInfo;
                }
            }

            return count;
        }
    }
}