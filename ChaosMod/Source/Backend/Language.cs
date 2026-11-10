using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Lego.ChaosMod.Backend;
public sealed class Language {
    public static Dictionary<string, string> _table = new Dictionary<string, string>();
    public static void Load(string langCode)
    {
        string path = Util.GetPluginDirectory($"{langCode}.json");
        if (!File.Exists(path)) {
            Logging.Error($"존제하지 않음: {path}");
            Logging.Error($"\"{langCode}\" 언어 데이터를 찾지 못하였습니다. 언어 키가 보일 수 있습니다.");
            Logging.Error($"I cannot find language data named \"{langCode}\", language keys can be shown.");
            return;
        }

        string json = File.ReadAllText(path);
        _table = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)
            ?? new Dictionary<string, string>();
    }

    public static string GetText(string key, string defaultValue = null!)
        => _table.TryGetValue(key, out var value) ? value : (string.IsNullOrEmpty(defaultValue) ? key : defaultValue);
}