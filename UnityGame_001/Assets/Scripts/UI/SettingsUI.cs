using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XIV.Core.TweenSystem;
using XIV.Core.Utils;
using XIV.Packages.ScriptableObjects.Channels;

namespace TheGame
{
    public class SettingsUI : MonoBehaviour
    {
        [SerializeField] GameObject uiGameObject;
        [SerializeField] VoidChannelSO toggleSettingsUIChannel;
        [SerializeField] GameSettingsChannelSO gameSettingsLoadedChannel;

        [SerializeField] Slider normalSensSlider;
        [SerializeField] Slider possibleHitSensSlider;

        GameSettings gameSettings;

        void OnEnable()
        {
            gameSettingsLoadedChannel.Register(OnGameSettingsLoaded);
            
            toggleSettingsUIChannel.Register(HandleDisplay);
            normalSensSlider.onValueChanged.AddListener(OnNormalSensChange);
            possibleHitSensSlider.onValueChanged.AddListener(OnPossibleHitSensChange);
        }

        void OnDisable()
        {
            gameSettingsLoadedChannel.Unregister(OnGameSettingsLoaded);
            
            toggleSettingsUIChannel.Unregister(HandleDisplay);
            normalSensSlider.onValueChanged.RemoveListener(OnNormalSensChange);
            possibleHitSensSlider.onValueChanged.RemoveListener(OnPossibleHitSensChange);
        }

        void OnGameSettingsLoaded(GameSettings obj)
        {
            gameSettings = obj;
            normalSensSlider.value = gameSettings.normalSensitivity;
            possibleHitSensSlider.value = gameSettings.possibleHitSensitivity;
        }

        void OnNormalSensChange(float val)
        {
            normalSensSlider.GetComponentInChildren<TMP_Text>().text = val.ToString("F");
            gameSettings.normalSensitivity = val;
        }

        void OnPossibleHitSensChange(float val)
        {
            possibleHitSensSlider.GetComponentInChildren<TMP_Text>().text = val.ToString("F");
            gameSettings.possibleHitSensitivity = val;
        }

        void HandleDisplay()
        {
            if (uiGameObject.activeSelf) Hide();
            else Show();
        }

        public void Show()
        {
            GameState.ChangeState(GameState.PAUSED);
            uiGameObject.SetActive(true);
            uiGameObject.transform.XIVTween()
                .Scale(Vector3.zero, Vector3.one, 0.5f, EasingFunction.Linear)
                .Start();
        }

        public void Hide()
        {
            uiGameObject.transform.XIVTween()
                .Scale(Vector3.one, Vector3.zero, 0.5f, EasingFunction.Linear)
                .OnComplete(() =>
                {
                    GameState.ChangeState(GameState.PLAYING);
                    uiGameObject.SetActive(false);
                })
                .Start();
        }
    }
}
