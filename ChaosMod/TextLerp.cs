using TMPro;
using UnityEngine;

namespace ChaosMod
{
    public class TextLerp: TextMeshProUGUI
    {
        public Vector2 targetPosition = Vector2.zero;

        void Update()
        {
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, 1f - Mathf.Exp(-Time.unscaledDeltaTime * Mathf.PI));
        }
    }
}
