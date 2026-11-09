#if false
using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// IGNORE THIS
namespace Lego.ChaosMod.Backend;
internal sealed class SaveHandler: MonoBehaviour
{
    private Toggle makeBackup;
    private TMP_Text errorTxt;

    void Start() {
        makeBackup = transform.GetChild(4).GetComponent<Toggle>();
        transform.GetChild(5).GetComponent<Button>().onClick.AddListener(OnPressUpgrade);
        transform.GetChild(6).GetComponent<Button>().onClick.AddListener(OnPressReset);
        errorTxt = transform.GetChild(7).GetComponent<TMP_Text>();
    }

    void OnPressUpgrade() {
        try {
            string path = Util.GetPluginDirectory("settings.bytes");
            if (!File.Exists(path))
            {
                Logging.Error($"Not found legacy save file at \"{path}\"");
                errorTxt.text = $"ERROR while upgrading: Legacy config file not found.";
                Destroy(gameObject);
                return;
            }

            Logging.Log($"Found legacy config file at \"{path}\"");
            if (makeBackup)
                if (!BackupThing(path, Util.GetPluginDirectory("settings-backup.bytes")))
                    return;

            string tempPath = Util.GetPluginDirectory("settings.bytes.temp");
            File.Move(path, tempPath);

            using var legacy_fs = new FileStream(tempPath, FileMode.Open);
            using var br = new BinaryReader(legacy_fs);
            using var fs = new FileStream(path, FileMode.Create);
            using var bw = new BinaryWriter(fs);

            Logging.Log($"Starting...");

            try
            {
                if (br.ReadString() != ChaosMod.CONFIG_HEADER)
                {
                    Logging.Error(Language.GetText("exception_config_invaild_header"));
                    errorTxt.text = "ERROR while upgrading: Invalid header found in legacy file.";
                    return;
                }
            }
            catch (Exception e)
            {
                Logging.Error(e.GetBaseException() + ": " + e.Message + "\n" + e.StackTrace);
                errorTxt.text = $"ERROR while upgrading: {e.Message} (Check console)";
                if (e.GetBaseException() is EndOfStreamException)
                {
                    Logging.Log(Language.GetText("exception_endofstream_config_1"));
                    Logging.Log(Language.GetText("exception_endofstream_config_2"));
                    errorTxt.text = $"ERROR while upgrading: End Of Stream. Check console.";
                }
                return;
            }

            int version = br.ReadInt32();
            if (version > ChaosMod.CONFIG_VERSION) {
                Logging.Log(Language.GetText("exception_config_version_match"));
                errorTxt.text = $"ERROR while upgrading: Legacy config has high version???? how???????";
                return;
            }

            var options = ChaosMod.Instance.options;
            bw.Write(ChaosMod.CONFIG_HEADER);
            bw.Write(2);
            bw.Write(options.datas.Count);

            int count = br.ReadInt32();
            Logging.Log($"Config count: {count}");
            for (int i = 0; i < count; i++) {
                string key = br.ReadString();
                string type = br.ReadString();

                if (options.IsTextDescType(key)) continue; // do not contain if it's description.

                bw.Write(key);
                bw.Write(type);

                switch (type) {
                    case "bool":
                        {
                            bw.Write(br.ReadBoolean());
                            break;
                        }
                    case "float":
                        {
                            bw.Write((float)br.ReadSingle());
                            break;
                        }
                    case "range":
                        {
                            bw.Write(br.ReadSingle());
                            bw.Write(br.ReadSingle());
                            break;
                        }
                    case "color":
                        {
                            bw.Write(br.ReadSingle());
                            bw.Write(br.ReadSingle());
                            bw.Write(br.ReadSingle());
                            break;
                        }
                }
            }

            bw.Flush();

            br.Close();
            legacy_fs.Close();
            File.Delete(tempPath);
            Logging.Log("Done and done!");
        } catch (Exception e) {
            Logging.Error($"An Error has occured while upgrading save file.\n{e.Message}\n{e.StackTrace}");
            errorTxt.text = $"ERROR while upgrading: {e.Message} (Check Console)";
        }

        Destroy(gameObject);
    }

    void OnPressReset() {
        string path = Util.GetPluginDirectory("settings.bytes");
        if (makeBackup) BackupThing(path, Util.GetPluginDirectory("settings-backup.bytes"));
        try {
            if (File.Exists(path)) File.Delete(path);
        } catch (Exception e) {
            errorTxt.text = $"ERROR while doing reset: {e.Message} (Check console)";
            return;
        }

        ChaosMod.Instance.SaveSettings();
        Destroy(gameObject);
    }

    bool BackupThing(string path, string to) {
        Logging.Log($"Starting backup.");

        try {
            if (File.Exists(path) && !File.Exists(to)) {
                File.Copy(path, to);
            } else {
                Logging.Error($"");
                return false;
            }
        } catch (Exception e) {
            Logging.Error($"An Error has occured while doing backup.\n{e.Message}\n{e.StackTrace}");
            errorTxt.text = $"ERROR while doing backup: {e.Message} (Check console)";
            return false;
        }
        errorTxt.text = "";
        return true;
    }
}
#endif