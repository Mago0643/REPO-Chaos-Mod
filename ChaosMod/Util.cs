using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ChaosMod
{
    internal class Util
    {
        public static FieldInfo GetInternalVar(object target, string varName)
        {
            Type type = target.GetType();
            return type.GetField(varName, BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static T GetInternalVar<T>(object target, string varName)
        {
            return (T)GetInternalVar(target, varName).GetValue(target);
        }

        public static string GetPluginDirectory(string file = "")
        {
            string pluginPath = typeof(ChaosMod).Assembly.Location;
            string pluginDir = System.IO.Path.GetDirectoryName(pluginPath);
            return System.IO.Path.Combine(pluginDir, file);
        }

        // Stolen and Edited from EnemyVision.cs
    }
}
