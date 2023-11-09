using System.Collections.Generic;
using PlayerSystems;
using UnityEngine;

namespace TheGame
{
    public static class TargetManager
    {
        public static List<MovingPlatform> targets;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init() => targets = new List<MovingPlatform>();

        public static void AddTargetToList(MovingPlatform target)
        {
            if (targets.Contains(target)) return;
            targets.Add(target);
        }

        public static void RemoveTarget(MovingPlatform target) => targets.Remove(target);
    }
}
