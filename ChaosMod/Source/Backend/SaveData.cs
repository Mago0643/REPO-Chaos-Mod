using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Lego.ChaosMod.Backend;
/// <summary>
/// For saving some datas in byte file.
/// </summary>
public sealed class SaveData
{
    public readonly struct DataValue
    {
        public readonly object Value;
        private DataValue(object v) => Value = v;

        public static implicit operator DataValue(bool v)   => new(v);
        public static implicit operator DataValue(int v)    => new(v);
        public static implicit operator DataValue(uint v)   => new(v);
        public static implicit operator DataValue(float v)  => new(v);
        public static implicit operator DataValue(string v) => new(v);
        public static implicit operator DataValue(double v) => new(v);
        public static implicit operator DataValue(float[] v) => new(v);
        public static implicit operator DataValue(int[] v) => new(v);
        public static implicit operator DataValue(string[] v) => new(v);
        public static implicit operator DataValue(double[] v) => new(v);
        public static implicit operator DataValue(uint[] v) => new(v);
        public static implicit operator DataValue(bool[] v) => new(v);
        public static implicit operator DataValue(UnityEngine.Color v) => new(v);
    }

    public string header = "";
    public const uint version = 1;

    private Random rnd;
    private Dictionary<string, object> data = new();
    private ReadOnlyDictionary<string, object> r_data => new(data);

    /// <summary>
    /// Makes a new SaveData instance.
    /// <param name="header">Save file header.</param>
    /// <param name="version">Save file version.</param>
    /// </summary>
    public SaveData(string header = "CHAOS_MOD_SAVE") {
        this.header = header;
        rnd = new Random();
    }

    public void Set(string key, DataValue value) => data[key] = value.Value;

    public T Get<T>(string key, T defaultValue = default!) {
        if (data.TryGetValue(key, out var v))
            return (T)v;
        else {
            Logging.Error($"Did not found data key for \"{key}\"");
            return defaultValue;
        }
    }
    public List<object> GetAllValues() => data.Values.ToList();
    public List<string> GetAllKeys() => data.Keys.ToList();
    public ReadOnlyDictionary<string, object> GetAll() => r_data;

    /// <summary>
    /// Loads binary file data from <paramref name="path"/>.
    /// </summary>
    /// <param name="path">Location to load file from.</param>
    public void Load(string path)
    {
        Load(new FileStream(path, FileMode.Open), true);
    }

    /// <summary>
    /// Loads save file from File Stream.
    /// </summary>
    /// <param name="stream">stream</param>
    /// <param name="dispose">Should dispose FileStream after finished loading?</param>
    public void Load(FileStream stream, bool dispose = false)
    {
        var dspe = () => { if (dispose) stream.Dispose(); };

        using var br = new BinaryReader(stream);

        string fHeader = UnConfuseStringBytes(br);
        if (fHeader != header) {
            dspe();
            throw new FormatException($"Header 1 mismatch (got {fHeader})");
        }

        uint fVersion = br.ReadUInt32();
        if (fVersion > version) {
            dspe();
            throw new NotSupportedException($"Save file version is high! we are at version {version}, but the file is {fVersion}.");
        }

        fHeader = UnConfuseStringBytes(br);
        if (fHeader != "Dictionary") {
            dspe();
            throw new FormatException($"Header 2 mismatch (got {fHeader})");
        }

        data.Clear();

        int dictLength = br.ReadInt32();
        for (int i = 0; i < dictLength; i++) {
            string key = UnConfuseStringBytes(br);
            string type = UnConfuseStringBytes(br);
            switch (type) {
                case "string": data.Add(key, UnConfuseStringBytes(br)); break;
                case "float": data.Add(key, br.ReadSingle()); break;
                case "int": data.Add(key, br.ReadInt32()); break;
                case "bool": data.Add(key, br.ReadBoolean()); break;
                case "uint": data.Add(key, br.ReadUInt32()); break;
                case "double": data.Add(key, br.ReadDouble()); break;
                case "float[]": {
                    List<float> array = new();
                    for (int k = 0; k < br.ReadInt32(); k++)
                        array.Add(br.ReadSingle());
                    data.Add(key, array.ToArray());
                    break;
                }
                case "int[]": {
                    List<int> array = new();
                    for (int k = 0; k < br.ReadInt32(); k++)
                        array.Add(br.ReadInt32());
                    data.Add(key, array.ToArray());
                    break;
                }
                case "uint[]": {
                    List<uint> array = new();
                    for (int k = 0; k < br.ReadInt32(); k++)
                        array.Add(br.ReadUInt32());
                    data.Add(key, array.ToArray());
                    break;
                }
                case "string[]": {
                    List<string> array = new();
                    for (int k = 0; k < br.ReadInt32(); k++)
                        array.Add(UnConfuseStringBytes(br));
                    data.Add(key, array.ToArray());
                    break;
                }
                case "double[]": {
                    List<double> array = new();
                    for (int k = 0; k < br.ReadInt32(); k++)
                        array.Add(br.ReadDouble());
                    data.Add(key, array.ToArray());
                    break;
                }
                case "bool[]": {
                    List<bool> array = new();
                    for (int k = 0; k < br.ReadInt32(); k++)
                        array.Add(br.ReadBoolean());
                    data.Add(key, array.ToArray());
                    break;
                }
                case "color": {
                    data.Add(key, new UnityEngine.Color(
                        br.ReadSingle(),
                        br.ReadSingle(),
                        br.ReadSingle(),
                        br.ReadSingle()
                    ));
                    break;
                }
            }
        }

        dspe();
    }

    public void Save(string path)
    {
        Save(new FileStream(path, FileMode.Create), true);
    }

    /// <summary>
    /// Saves the binary file using File Stream.
    /// </summary>
    /// <param name="stream">File Stream</param>
    /// <param name="dispose">Should dispose FileStream after finished saving?</param>
    public void Save(FileStream stream, bool dispose = false)
    {
        var dspe = () => { if (dispose) stream.Dispose(); };

        using var bw = new BinaryWriter(stream);

        ConfuseStringBytes(bw, header);
        bw.Write(version);
        ConfuseStringBytes(bw, "Dictionary");

        bw.Write(data.Count);
        foreach (var item in data) {
            string type = TypeToString(item.Value);
            ConfuseStringBytes(bw, item.Key);
            ConfuseStringBytes(bw, type);

            if (type == "string") {
                ConfuseStringBytes(bw, (string)item.Value);
            } else {
                switch (type) {
                    case "bool": bw.Write((bool)item.Value); break;
                    case "float": bw.Write((float)item.Value); break;
                    case "double": bw.Write((double)item.Value); break;
                    case "uint": bw.Write((uint)item.Value); break;
                    case "int": bw.Write((int)item.Value); break;
                    case "float[]": {
                        var array = (float[])item.Value;
                        bw.Write(array.Length);
                        for (int i = 0; i < array.Length; i++)
                            bw.Write(array[i]);
                        break;
                    }
                    case "int[]": {
                        var array = (int[])item.Value;
                        bw.Write(array.Length);
                        for (int i = 0; i < array.Length; i++)
                            bw.Write(array[i]);
                        break;
                    }
                    case "uint[]": {
                        var array = (uint[])item.Value;
                        bw.Write(array.Length);
                        for (int i = 0; i < array.Length; i++)
                            bw.Write(array[i]);
                        break;
                    }
                    case "string[]": {
                        var array = (string[])item.Value;
                        bw.Write(array.Length);
                        for (int i = 0; i < array.Length; i++)
                            ConfuseStringBytes(bw, array[i]);
                        break;
                    }
                    case "double[]": {
                        var array = (double[])item.Value;
                        bw.Write(array.Length);
                        for (int i = 0; i < array.Length; i++)
                            bw.Write(array[i]);
                        break;
                    }
                    case "bool[]": {
                        var array = (bool[])item.Value;
                        bw.Write(array.Length);
                        for (int i = 0; i < array.Length; i++)
                            bw.Write(array[i]);
                        break;
                    }
                    case "color": {
                        var c = (UnityEngine.Color)item.Value;
                        bw.Write(c.r);
                        bw.Write(c.g);
                        bw.Write(c.b);
                        bw.Write(c.a);
                        break;
                    }
                }
            }
        }

        dspe();
    }

    string TypeToString(object value)
    {
        return value.GetType().Name switch {
            "Int32" => "int",
            "Single" => "float",
            "Double" => "double",
            "Boolean" => "bool",
            "String" => "string",
            "UInt32" => "uint",
            "Color" => "color",
            _ => value.GetType().Name.ToLower()
        };
    }

    void ConfuseStringBytes(BinaryWriter bw, string text = "")
    {
        bw.Write(text.Length);

        if (string.IsNullOrEmpty(text)) return;
        foreach (char c in text) {
            int charNum = c;
            int offset = rnd.Next(-5, 6);
            bw.Write(offset);
            bw.Write(charNum + offset);
        }

        bw.Write("END");
    }

    string UnConfuseStringBytes(BinaryReader br)
    {
        int textLength = br.ReadInt32();
        if (textLength <= 0) return "";

        StringBuilder builder = new();

        for (int i = 0; i < textLength; i++) {
            int offset = -br.ReadInt32();
            builder.Append((char)(br.ReadInt32() + offset));
        }

        if (br.ReadString() != "END") {
            throw new FormatException($"String footer mismatch");
        }

        return builder.ToString();
    }
}