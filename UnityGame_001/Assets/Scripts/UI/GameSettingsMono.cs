using System;
using UnityEngine;

namespace TheGame
{
    [Serializable]
    public class GameSettings
    {
        [Range(0, 1)]
        public float normalSensitivity = 1f;
        [Range(0, 1)]
        public float possibleHitSensitivity = 0.5f;
    }
    
    public class GameSettingsMono : MonoBehaviour
    {
        [SerializeField] GameSettingsChannelSO gameSettingsLoadedChannel;
        [SerializeField] GameSettings gameSettings;
        
        void Start()
        {
            gameSettingsLoadedChannel.RaiseEvent(gameSettings);
        }
    }
}