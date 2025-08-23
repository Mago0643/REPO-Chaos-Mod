using System;
using System.Collections.Generic;
using System.Text;

namespace ChaosMod
{
    public class Language
    {
        public static Dictionary<string, string> KoreanToEnglish = new Dictionary<string, string>();
        public static void Init()
        {
            // 더 좋게 만들고 싶은데 귀찮음
            Dictionary<string, string> thing = new Dictionary<string, string>()
            {
                {"무효", "Nothing"},
                {"넘어지기", "Tumble"},
                {"모든 몬스터 죽이기", "Kill all monsters"},
                {"넓은 시야각", "Wide FOV"},
                {"게임 속도 x2", "Game Speed x2"},
                {"모든 미닫이문 부수기", "Break All Hinges"},
                {"환청", "Auditory Hallucination"},
                {"체력 숨기기", "Hide Health"},
                {"기력 숨기기", "Hide Energy"},
                {"모든 서랍 문을 지도에 표시", "Mark All Drawers on Map"},
                {"축지법 쓰는 오리", "Crazy Duck"},
                {"무작위 대사 말하기", "Speak random thing"},
                {"지진", "Earthquake"},
                {"귀중품 가격 두배", "Double Valuables Price"},
                {"귀중품 가격 -10%", "Valuables -10% Price"},
                {"회전회오리이이이", "SPINNNNN!!!"},
                {"친화적인 몬스터", "Friendly monsters"},
                {"높은음 목소리", "High-pitched Voice"},
                {"낮은음 목소리", "Low-pitched Voice"},
                {"화면 효과 비활성화", "Disable Screen Effects"},
                {"티타늄 오브", "Titanium Orb"},
                {"죽은 플레이어 소생", "Revive all players"},
                {"점프 & 달리기 없음", "No Sprint & Jump"},
                {"가족 친화적", "Family Friendly"},
                {"기본", "General"},
                {"레벨 이벤트", "Level Events"},
                {"어지러움", "Dizziness"},
                {"한국어", "Korean"},
                {"게이지 빨간색 차지 비중", "Gauge Color Red"},
                {"게이지 초록색 차지 비중", "Gauge Color Green"},
                {"게이지 파란색 차지 비중", "Gauge Color Blue"},
                {"동공 확장", "Pupil Dilation"},
                {"올해의 운전사", "Driver of the year"},
                {"무중력", "No Gravity"},
                {"전체 조명 비활성화", "Disable All Lighting"},
                {"몬스터 2마리 소환", "Spawn 2 monsters"},
                {"몬스터 4마리 소환", "Spawn 4 monsters"}
            };

            KoreanToEnglish.Clear();
            foreach (var keyValue in thing)
            {
                KoreanToEnglish.Add(keyValue.Key, keyValue.Value);
            }
        }

        public static string GetText(string key)
        {
            bool isKorean = false;
            if (ChaosMod.Instance.ConfigKorean != null)
                isKorean = ChaosMod.Instance.ConfigKorean.Value;
            if (!KoreanToEnglish.ContainsKey(key) || isKorean)
            {
                return key;
            }
            return KoreanToEnglish[key];
        }
    }
}
