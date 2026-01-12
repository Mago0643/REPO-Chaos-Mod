using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace ChaosMod
{
    public class Language
    {
        public static Dictionary<string, string> _table = new Dictionary<string, string>();
        public static void Load(string langCode)
        {
            string path = Util.GetPluginDirectory($"{langCode}.json");
            if (!File.Exists(path)) {
                ChaosMod.Logger.LogError($"존제하지 않음: {path}");
                ChaosMod.Logger.LogError($"\"{langCode}\" 언어 데이터를 찾지 못하였습니다. 언어 키가 보일 수 있습니다.");
                ChaosMod.Logger.LogError($"I cannot find language data named \"{langCode}\", language keys can be shown.");
                return;
            }

            string json = File.ReadAllText(path);
            _table = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)
                ?? new Dictionary<string, string>();
        }

        public static string GetText(string key)
        {
            return _table.TryGetValue(key, out var value) ? value : key;
        }
    }
}
