using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Lego.ChaosMod.Backend;

internal class Logging {
  static readonly string color_log  = "\x1B[1;36m"; // Bold + Cyan
  static readonly string color_warn = "\x1B[1;33m"; // Bold + Yellow
  static readonly string color_err  = "\x1B[1;31m"; // Bold + Red
  static readonly string reset_color = "\x1B[0m";
  
  static FileStream stream;
  static StreamWriter writer;

  public static void Log(params object[] data) {
    var frame = new StackFrame(1, true);
    string str = string.Join(", ", data);
    ChaosMod.Logger.LogInfo($"{color_log}{str}{reset_color}");

    writer?.WriteLine($"[LOG | {Path.GetFileName(frame.GetFileName())}:{frame.GetFileLineNumber()} | {DateTime.Now:HH:mm:ss.fff}] {str}");
    writer?.Flush();
  }

  public static void Warn(params object[] data) {
    var frame = new StackFrame(1, true);
    string str = string.Join(", ", data);
    ChaosMod.Logger.LogWarning($"{color_warn}{str}{reset_color}");

    writer?.WriteLine($"[WARN | {Path.GetFileName(frame.GetFileName())}:{frame.GetFileLineNumber()} | {DateTime.Now:HH:mm:ss.fff}] {str}");
    writer?.Flush();
  }

  public static void Error(params object[] data) {
    var frame = new StackFrame(1, true);
    string str = string.Join(", ", data);
    ChaosMod.Logger.LogError($"{color_err}{str}{reset_color}");

    writer?.WriteLine($"[ERROR | {Path.GetFileName(frame.GetFileName())}:{frame.GetFileLineNumber()} | {DateTime.Now:HH:mm:ss.fff}] {str}");
    writer?.Flush();
  }

  internal static void Init()
  {
    string logPath = Util.GetPluginDirectory("latest.log");
    if (File.Exists(logPath)) {
      try {
        string oldLog = Util.GetPluginDirectory("latest-old.log");
        if (File.Exists(oldLog)) File.Delete(oldLog);
        File.Move(logPath, oldLog);
      } catch (IOException e) {
        Error("You dun fucked up: " + e.StackTrace);
      }
    }

    Application.logMessageReceived += OnUnityLogRecived;

    stream = File.OpenWrite(logPath);
    writer = new StreamWriter(stream, Encoding.UTF8);
  }

  // just in case...
  static void OnUnityLogRecived(string condition, string stackTrace, LogType type) {
    if (type == LogType.Log || type == LogType.Assert) return;

    string typeStr = "Log";
    string data = "";
    switch (type) {
      case LogType.Error: {
        typeStr = "Error";
        data = $"\nStack Trace:\n{stackTrace}";
        break;
      }
      case LogType.Warning: { typeStr = "Warning"; break; }
      case LogType.Exception: {
        typeStr = "Exception";
        data = $"\nStack Trace:\n{stackTrace}";
        break;
      }
    }

    writer?.WriteLine($"[Unity {typeStr} | {DateTime.Now:HH:mm:ss.fff}] {condition}{data}");
    writer?.Flush();
  }

  internal static void CleanUp()
  {
    writer?.Flush();
    writer?.Close();
    writer = null;
  }

  public static void Assert(bool condition, string msg) {
    UnityEngine.Debug.Assert(condition, msg);
  }
}