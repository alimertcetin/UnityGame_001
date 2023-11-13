using UnityEngine;

namespace TheGame
{
    public static class GameState
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            currentState = -1;
            ChangeState(PLAYING);
        }
        
        public static int currentState { get; private set; }
        public static int previousState { get; private set; }

        public const int PAUSED = 0;
        public const int PLAYING = 1;
        public const int ARROW_RELEASED = 2;

        public static int[] ALL =
        {
            PAUSED,
            PLAYING,
            ARROW_RELEASED,
        };

        public static void ChangeState(int newState)
        {
            previousState = currentState;
            currentState = newState;
        }
    }
}