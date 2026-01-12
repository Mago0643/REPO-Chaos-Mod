using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using static GameDirector;
using UnityEngine.TextCore.Text;

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

        public static void RectTransformFullscreen(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // https://github.com/openitg/openitg/blob/master/src/ArrowEffects.cpp#L296
        public static float BeatMath(float beat, float bpm, float y = 0f)
        {
            float SCALE(float x, float l1, float h1, float l2, float h2)
            {
                return (((x) - (l1)) * ((h2) - (l2)) / ((h1) - (l1)) + (l2));
            }
            float fAccelTime = 0.2f, fTotalTime = 0.5f;

            /* If the song is really fast, slow down the rate, but speed up the
             * acceleration to compensate or it'll look weird. */
            float fBPM = bpm;
            float fDiv = Mathf.Max(1.0f, (float)Math.Truncate(fBPM / 150.0f));
            fAccelTime /= fDiv;
            fTotalTime /= fDiv;

            /* offset by VisualDelayEffect seconds */
            float fBeat = beat + fAccelTime;
            fBeat /= fDiv;

            bool bEvenBeat = (Math.Truncate(fBeat) % 2) != 0;

            /* -100.2 -> -0.2 -> 0.2 */
            if (fBeat < 0)
                return 0f;

            fBeat -= (float)Math.Truncate(fBeat);
            fBeat += 1;
            fBeat -= (float)Math.Truncate(fBeat);

            if (fBeat >= fTotalTime)
                return 0f;

            float fAmount = 0f;
            if (fBeat < fAccelTime)
            {
                fAmount = SCALE(fBeat, 0.0f, fAccelTime, 0.0f, 1.0f);
                fAmount *= fAmount;
            }
            else /* fBeat < fTotalTime */
            {
                fAmount = SCALE(fBeat, fAccelTime, fTotalTime, 1.0f, 0.0f);
                fAmount = 1 - (1 - fAmount) * (1 - fAmount);
            }
            if (bEvenBeat)
                fAmount *= -1;

            return 20.0f * fAmount * Mathf.Sin( y / 15f + Mathf.PI / 2.0f );
        }
    }
}
