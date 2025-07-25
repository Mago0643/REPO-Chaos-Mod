using ExitGames.Client.Photon.StructWrapping;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace ChaosMod
{
    [PostProcess(typeof(RainbowEffect), PostProcessEvent.AfterStack, "Hidden/PostProcessing/Rainbow"), Serializable]
    public sealed class RainbowEffect: PostProcessEffectSettings
    {
        [Range(0f, 1f), Tooltip("Rainbow intensity")]
        public FloatParameter intensity = new FloatParameter { value = 0.25f };
    }

    public sealed class RainbowRenderer : PostProcessEffectRenderer<RainbowEffect>
    {
        static Shader _shader;

        public override void Init()
        {
            if (_shader == null)
            {
                // 이미 로드된 경우 캐시됨
                LoadShaderFromBundle();
            }
        }

        void LoadShaderFromBundle()
        {
            _shader = ChaosMod.Instance.assets.LoadAsset<Shader>("Hidden/PostProcessing/Rainbow");
            if (_shader == null)
                Debug.LogError("Shader not found in AssetBundle!");
        }

        public override void Render(PostProcessRenderContext context)
        {
            if (_shader == null)
            {
                Debug.LogWarning("Rainbow shader not loaded.");
                return;
            }

            var sheet = context.propertySheets.Get(_shader);

            sheet.properties.SetFloat("_Intensity", settings.intensity.value);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}