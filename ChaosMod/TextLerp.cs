using TMPro;
using UnityEngine;

namespace ChaosMod
{
    public class TextLerp: TextMeshProUGUI
    {
        public Vector2 targetPosition = Vector2.zero;
        public Vector2 posOffset = Vector2.zero;
        private Vector2 lerpedPosition = Vector2.zero;

        void Start()
        {
            UpdateLerpPos();
        }

        public void UpdateLerpPos()
        {
            lerpedPosition = rectTransform.anchoredPosition;
        }

        void Update()
        {
            lerpedPosition = Vector2.Lerp(lerpedPosition, targetPosition, 1f - Mathf.Exp(-Time.unscaledDeltaTime * Mathf.PI));
            rectTransform.anchoredPosition = lerpedPosition + posOffset;
        }
    }
}
