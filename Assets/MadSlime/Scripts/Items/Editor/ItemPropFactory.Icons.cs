using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MadSlime.Items.Editor
{
    public partial class ItemPropFactory
    {
        private int RenderIcons(List<ItemPropJob> jobs)
        {
            int rendered = 0;

            for (int i = 0; i < jobs.Count; i++)
            {
                int jobNumber = i + 1;
                float progress = (float)i / jobs.Count;

                EditorUtility.DisplayProgressBar("Prop Factory",
                    $"Icon {jobs[i].PrefabName} ({jobNumber}/{jobs.Count})", progress);

                if (TryRenderIcon(jobs[i]))
                {
                    rendered++;
                }
            }

            return rendered;
        }

        private bool TryRenderIcon(ItemPropJob job)
        {
            if (File.Exists(job.IconPath) && _isIconRegenerationForced == false)
            {
                return false;
            }

            IconRenderSettings settings = new IconRenderSettings
            {
                LightEuler = new Vector3(45f, -30f, 0f),
                IsRelightingEnabled = true,
                CameraOffset = new Vector3(1f, 0.85f, 1.6f),
                OrthographicPadding = 1.15f,
                IsProjectiveFitEnabled = true
            };
            Texture2D texture = _tools.RenderToTexture(job.Model, _iconLayer, IconSize, settings);

            File.WriteAllBytes(job.IconPath, texture.EncodeToPNG());
            DestroyImmediate(texture);

            return true;
        }
    }
}
