using UnityEngine;
using XIV.Packages.ScriptableObjects.Channels;

namespace TheGame
{
    [CreateAssetMenu(menuName = "Game/" + nameof(GameSettingsChannelSO))]
    public class GameSettingsChannelSO : XIVChannelSO<GameSettings>
    {
        
    }
}