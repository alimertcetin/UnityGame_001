using System;
using PlayerSystems;
using UnityEngine;
using UnityEngine.Pool;
using XIV.Core.Collections;
using Object = UnityEngine.Object;

namespace TheGame.ThrowSystems
{
    public static class ThrowSystem
    {
        class ThrowHelper : BehaviourBase
        {
            static DynamicArray<Transform> throwObjects = new();
            static DynamicArray<ThrowData> throwDatas = new();
            static DynamicArray<float> passedTimes = new();
            static DynamicArray<Action<ThrowObjectHitData>> callbackDatas = new();

            void Update()
            {
                int count = throwObjects.Count;
                for (int i = 0; i < count; i++)
                {
                    var throwObject = throwObjects[i];
                    var throwData = throwDatas[i];
                    ref var timePassed = ref passedTimes[i];
                    
                    var currentPosition = throwObject.position;
                    var nextPosition = TrajectoryUtils.GetPoint(throwData.startPos, throwData.velocity, throwData.acceleration, timePassed);
                    timePassed += Time.deltaTime;

                    if (Physics.Linecast(currentPosition, nextPosition, out var hitInfo, throwData.collisionLayers))
                    {
                        SetPosition(throwObject, hitInfo.point);
                        HandleRemove(i, currentPosition, nextPosition);
                        return;
                    }

                    if (currentPosition.y < -15f)
                    {
                        HandleRemove(i, currentPosition, nextPosition);
                        return;
                    }

                    SetPosition(throwObject, nextPosition);
                }
            }
            
            public void Add(Transform throwObject, ThrowData throwData, Action<ThrowObjectHitData> onHit)
            {
                throwObjects.Add() = throwObject;
                throwDatas.Add() = throwData;
                passedTimes.Add() = 0f;
                callbackDatas.Add() = onHit;
            }

            void RemoveAt(int index)
            {
                throwObjects.RemoveAt(index);
                throwDatas.RemoveAt(index);
                passedTimes.RemoveAt(index);
                callbackDatas.RemoveAt(index);
            }

            void SetPosition(Transform t, Vector3 nextPosition)
            {
                var currentPos = t.position;
                t.forward = nextPosition - currentPos;
                t.position = nextPosition;
            }

            void HandleRemove(int index, Vector3 beforePos, Vector3 afterPos)
            {
                var throwObject = throwObjects[index];
                if (callbackDatas[index] == null)
                {
                    Destroy(throwObject.gameObject);
                    RemoveAt(index);
                    return;
                }
                callbackDatas[index].Invoke(GetData(throwObject, beforePos, afterPos));
                RemoveAt(index);
            }

            protected override int[] GetStates()
            {
                return new[]
                {
                    GameState.PLAYING,
                    GameState.ARROW_RELEASED,
                };
            }

            void OnDrawGizmos()
            {
                int count = throwObjects.Count;
                for (int i = 0; i < count; i++)
                {
                    var throwData = throwDatas[i];
                    var list = ListPool<Vector3>.Get();

                    const int DETAIL = 128;
                    const float DURATION = 10f; // time in seconds
            
                    for (int j = 0; j < DETAIL; j++)
                    {
                        var t = j / (DETAIL - 1f);
                        var time = t * DURATION;
                        Vector3 point = TrajectoryUtils.GetPoint(throwData.startPos, throwData.velocity, throwData.acceleration, time);
                        list.Add(point);
                    }

                    for (int j = 0; j < DETAIL - 1; j++)
                    {
                        Gizmos.DrawLine(list[j], list[j + 1]);
                    }
            
                    ListPool<Vector3>.Release(list);
                }
            }

            static ThrowObjectHitData GetData(Transform throwObject, Vector3 beforePos, Vector3 afterPos)
            {
                return new ThrowObjectHitData { throwObject = throwObject, beforeHitPosition = beforePos, afterHitPosition = afterPos };
            }
        }

        static ThrowHelper throwHelper;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            throwHelper = new GameObject("--- ThrowHelper ---").AddComponent<ThrowHelper>();
            Object.DontDestroyOnLoad(throwHelper);
        }
        
        public static void Throw(GameObject go, ThrowData throwData, Action<ThrowObjectHitData> onHit)
        {
            throwHelper.Add(go.transform, throwData, onHit);
        }
    }
}