using TMPro;
using UnityEngine;

namespace ChaosMod
{
    internal class EventTimerBar: TextMeshProUGUI
    {
        float timeLeft;
        float ogTimeLeft = 0f;

        public Color noTimeColor = Color.red;
        public Color enoughTimeColor = Color.green;
        public Vector2 targetPosition = Vector2.zero;

        public void SetTime(float time)
        {
            timeLeft = time;
            ogTimeLeft = time;
        }

        void Update()
        {
            if (timeLeft > 0f)
                timeLeft -= Time.unscaledDeltaTime;

            int seconds = Mathf.FloorToInt(timeLeft % 60f);
            int minutes = Mathf.FloorToInt((timeLeft / 60) % 60f);
            int hour = seconds / 3600;

            string timeString = "";
            if (hour > 0)
                timeString += $"{hour}:{minutes:D2}:{seconds:D2}";
            else
                timeString += $"{minutes:D2}:{seconds:D2}";

            text = $"({timeString})";
            color = Color.Lerp(noTimeColor, enoughTimeColor, timeLeft / ogTimeLeft);

            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, 1f - Mathf.Exp(-Time.unscaledDeltaTime * Mathf.PI));
        }
    }
}
