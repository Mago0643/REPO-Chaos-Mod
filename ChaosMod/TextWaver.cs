using TMPro;
using UnityEngine;

namespace ChaosMod {
    internal class TextWaver: MonoBehaviour
    {
        internal float radius = 10f;
        private TMP_Text tmpText;
        void Start()
        {
            if (!TryGetComponent<TMP_Text>(out tmpText))
            {
                ChaosMod.Logger.LogWarning("Did not found TMP_Text on " + name + ".");
                Destroy(this);
                return;
            }
        }

        void LateUpdate()
        {
            tmpText.ForceMeshUpdate();

            var tInfo = tmpText.textInfo;
            var meshInfo = tInfo.meshInfo;

            for (int i = 0; i < tInfo.characterCount; i++)
            {
                var cInfo = tInfo.characterInfo[i];
                if (!cInfo.isVisible) continue;

                int mIdx = cInfo.materialReferenceIndex;
                int vIdx = cInfo.vertexIndex;

                // y sine-wave
                float waveOffset = (float)i / tInfo.characterCount;
                waveOffset *= 5f;
                Vector3 offset = new Vector3(0f, Mathf.Sin(Time.time * Mathf.PI + waveOffset) * radius, 0f);

                var src = tmpText.textInfo.meshInfo[mIdx].vertices;
                var dst = meshInfo[mIdx].vertices;

                Vector3 center = (src[vIdx] + src[vIdx + 2]) / 2f;

                // rotation
                float angle = Mathf.Cos(Time.time * Mathf.PI + waveOffset) * radius;
                Quaternion rot = Quaternion.Euler(0f, 0f, angle);

                // rainbow colors
                var colors = meshInfo[mIdx].colors32;
                Color32 c = Color.HSVToRGB(
                    Mathf.Repeat(Time.time / 5f + waveOffset / 5f, 1f), 0.75f, 1f
                );

                for (int k = 0; k < 4; k++)
                {
                    Vector3 pos = src[vIdx + k];
                    pos -= center;
                    pos = rot * pos;
                    pos += center;
                    pos += offset;

                    dst[vIdx + k] = pos;
                    colors[vIdx + k] = c;
                }
            }

            for (int i = 0; i < meshInfo.Length; i++)
            {
                meshInfo[i].mesh.vertices = meshInfo[i].vertices;
                meshInfo[i].mesh.colors32 = meshInfo[i].colors32;
                tmpText.UpdateGeometry(meshInfo[i].mesh, i);
            }
        }
    }
}