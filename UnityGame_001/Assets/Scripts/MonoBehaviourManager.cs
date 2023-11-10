using System.Collections.Generic;
using TheGame;
using UnityEngine;

namespace PlayerSystems
{
    public static class MonoBehaviourManager
    {
        class Controller : MonoBehaviour
        {
            void Update()
            {
                int currentState = GameState.currentState;
                var behaviours = monoBehaviours[currentState];
                int count = behaviours.Count;
                for (int i = 0; i < count; i++)
                {
                    var behaviour = behaviours[i];
                    if (behaviour.isActiveAndEnabled)
                    {
                        behaviour.Tick();
                    }
                }
            }
        }

        static Dictionary<int, List<BehaviourBase>> monoBehaviours;
        static Controller controller;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            monoBehaviours = new Dictionary<int, List<BehaviourBase>>();
            var states = GameState.ALL;
            int length = states.Length;
            for (int i = 0; i < length; i++)
            {
                monoBehaviours.Add(states[i], new List<BehaviourBase>());
            }

            controller = new GameObject("--- BehaviourController ---").AddComponent<Controller>();
            Object.DontDestroyOnLoad(controller);
        }
        
        public static void Add(BehaviourBase behaviour, params int[] states)
        {
            int length = states.Length;
            for (int i = 0; i < length; i++)
            {
                monoBehaviours[states[i]].Add(behaviour);
            }
        }

        public static void Remove(BehaviourBase behaviour, params int[] states)
        {
            int length = states.Length;
            for (int i = 0; i < length; i++)
            {
                monoBehaviours[states[i]].Remove(behaviour);
            }
        }
    }
    
}