using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Items.Editor
{
    public partial class ItemPropFactory
    {
        private int RenderIcons(List<ItemPropJob> jobs)
        {
            int rendered = 0;

            for (int i = 0; i < jobs.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Prop Factory",
                        $"Icon {jobs[i].PrefabName} ({i + 1}/{jobs.Count})", (float)i / jobs.Count);

                if (RenderIcon(jobs[i]) == true)
                {
                    rendered++;
                }
            }

            return rendered;
        }

        private bool RenderIcon(ItemPropJob job)
        {
            if (File.Exists(job.IconPath) == true && _forceIcons == false)
            {
                return false;
            }

            IconRenderSettings settings = new IconRenderSettings
            {
                LightEuler = new Vector3(45f, -30f, 0f),
                RelightToCamera = true,
                CameraOffset = new Vector3(1f, 0.85f, 1.6f),
                OrthographicPadding = 1.15f,
                FitByProjectiveBounds = true
            };
            Texture2D texture = ItemFactoryTools.RenderToTexture(job.Model, _iconLayer, IconSize, settings);

            File.WriteAllBytes(job.IconPath, texture.EncodeToPNG());
            DestroyImmediate(texture);

            return true;
        }
    }
}
