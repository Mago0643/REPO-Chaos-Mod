using Lego.ChaosMod.Backend;
using System.Collections.Generic;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class SayRandomThingsEvent: ChaosEvent {
    /*public static string[] tries =
    {
        "What the fuck did you just fucking say about me, you little bitch?",
        "git gud", "goo goo ga ga.", "AAAAAAAAAAAAAAAAAAAAAAAA",
        "OOOOOOOOOOOOOOOOOO", "I love you.", "That monster I fucking hate.", "I love this game.",
        "Hey silly!", "jaundice", "I'm the one that carrying this whole game!",
        "goo goo ga ga?", "I have nothing to say.", "오빤 강남스타일", "I LOOOVEEEE",
        "75000 DOLLARS?? OH MY GOSH", "Tung Tung Tung Tung Tung Tung Tung Tung Tung Sahur",
        "Brr Brr Patapim", "adorable", "The cake is a lie.", "This is Certified hood classic.",
        "This is the part where he kills you.", "skill issue", "CHICKEN JOCKEY", "skibidi",
        "I. AM. STEVE.", "THanks for playing my mod!", "six sevennn", "It starts with one...", "One thing, I don't know why", // In the end reference
        "it doesn't even matter how hard you try,", "ERROR!", "Look behind you!"
    };

    public static string[] tries_korean =
    {
        "방금 나한테 씨발 뭐라고 지껄였냐, 이 미친 새끼야?",
        "꼬우면 접든가", "구 구 가 가.", "아아아아아아아아아아아아아아아아아",
        "오오오오오오오오오오오오오", "사랑해.", "내가 존나 싫어하는 괴물.", "난 이 게임이 좋아.",
        "바보야!", "정으이", "나 혼자 이 게임을 캐리하고 있어!",
        "구 구 가 가?", "뭐라고 말할 게 없어.", "오빤 강남스타일", "아이 러어어어어브~",
        "세븐 밀리언 퐈이브 따우젼 딸러즈?? 오우 마이 가쉬!!", "퉁 퉁 퉁 퉁 퉁 퉁 퉁 퉁 퉁 사후르",
        "브르르 브르르 파타핌", "기엽다", "케이크는 거짓말이야.", "이게 진짜 띵작이지.",
        "그가 당신을 죽이는 순간", "실력 차이", "치킨 조키!", "스키비디",
        "아이. 앰. 스티브.", "제 모드를 플레이 해 줘서 고마워요!", "식스 세브으으은", "한 가지로부터 시작되어...", "한 가지 확실한 건, 나도 왜인지는 몰라.",
        "네가 얼마나 노력했다 해도 상관없어,", "오류!", "뒤를 봐!"
    };

    public static string[] tries_ff = [
        "git gud", "goo goo ga ga.", "AAAAAAAAAAAAAAAAAAAAAAAA",
        "OOOOOOOOOOOOOOOOOO", "I love you.", "I love this game.", "Hey silly!",
        "jaundice", "I'm the one that carrying this whole game!",
        "goo goo ga ga?", "I have nothing to say.", "오빤 강남스타일", "I LOOOVEEEE",
        "75000 DOLLARS?? OH MY GOSH", "Tung Tung Tung Tung Tung Tung Tung Tung Tung Sahur",
        "Brr Brr Patapim", "adorable", "The cake is a lie.", "This is Certified hood classic.",
        "CHICKEN JOCKEY", "skibidi", "I. AM. STEVE.", "THanks for playing my mod!",
        "six sevennn", "It starts with one...", "One thing, I don't know why", // In the end reference
        "it doesn't even matter how hard you try,", "ERROR!", "Look behind you!"
    ];*/

    public SayRandomThingsEvent(): base("evt_say_random_thing", "evt_say_random_thing_desc")
    {
        isOnce = true;
        options.multiplayerOnly = true;
        ID = 14;
    }

    List<string> ConvertStringShitWhatever(string glue) {
        // ex. glue = "\"string 1\", \"string 2\",\"string 3\""

        List<string> strings = new();
        if (glue.Trim() == "") return strings;

        string[] cock = glue.Split(","); // { "\"string 1\"", " \"string 2\"", "\"string 3\"" }
        for (int i = 0; i < cock.Length; i++) {
            string str = cock[i];

            int strStartIndex = str.IndexOf("\"");
            if (strStartIndex == -1) {
                // if did not found ", find the '.
                strStartIndex = str.IndexOf("\'");
                if (strStartIndex == -1) {
                    Logging.Error($"Format Error: String should start with \" or \'. (at position {i})");
                    break;
                }
            }

            int strEndIndex = str.IndexOf(str[strStartIndex], strStartIndex + 1);
            if (strEndIndex == -1) {
                Logging.Error($"Format Error: String must be end with {str[strStartIndex]}. (at position {i})");
                break;
            }

            strings.Add(str.Substring(strStartIndex + 1, strEndIndex - strStartIndex - 1)); // string 1
        }
        return strings;
    }

    public override void Start()
    {
        base.Start();
        ChatManager chat = ChatManager.instance;

        // also TODO: If there's an player who turned on family friendly, make all players do not say swer words

        /*string speech = tries[Random.Range(0, tries.Length)];
        if (ChaosMod.Instance.familyFriendly) {
            string[] cussWords = { "fuck", "fucking", "suck", "bitch", "idiot", "kill" };
            foreach (string word in cussWords)
            {
                if (speech.Contains(word))
                    speech = speech.Replace(word, "(REDACTED)");
            }
        }*/

        var dialogue = ConvertStringShitWhatever( Language.GetText("evt_say_random_thing_dialogues" + (ChaosMod.Instance.familyFriendly ? "_family_friendly" : "")) );
        string speech = dialogue[Random.Range(0, dialogue.Count)];

        chat.PossessChatScheduleStart(4);
        chat.PossessChat(ChatManager.PossessChatID.None, speech, UnityEngine.Random.Range(1.5f, 2f), Color.Lerp(Color.yellow, new Color(0f, 0.5f, 1f), Random.Range(0f, 1f)));
        chat.PossessChatScheduleEnd();
    }
}