using TMPro;
using UnityEngine;

namespace TheGame.UI
{
    public class HitPointText : MonoBehaviour
    {
        public TMP_Text text;

        void OnValidate()
        {
            if (text) return;
            text = GetComponentInChildren<TMP_Text>();
        }
    }
}