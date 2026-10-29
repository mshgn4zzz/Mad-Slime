using System;
using System.Collections.Generic;
using System.IO;
using MadSlime.Shop.Data;
using MadSlime.Skins.Data;
using UnityEditor;
using UnityEngine;
using YG;

namespace MadSlime.Saves.Editor
{
    public class PlayerDataResetTool
    {
        private readonly string _editorSavesPath = Path.Combine(
            InfoYG.PATCH_PC_EDITOR,
            "SavesEditorYG2.json");
        private const string ShopContentPath = "Assets/MadSlime/Scriptables/Shop/ShopContent.asset";
        private const string PendingResetKey = "MadSlime.PlayerDataResetTool.PendingReset";
        private const int EmptyBalance = 0;
        private const int TestBalance = 9999999;

        public void ResetPlayerData()
        {
            Reset(EmptyBalance);
        }

        public void ResetPlayerDataWithMoney()
        {
            Reset(TestBalance);
        }

        public void ResetPlayerDataWithMoneyAndSkins()
        {
            Reset(TestBalance, CollectSkinIds());
        }

        public void UnlockAllSkins()
        {
            UnlockSkins();
        }

        public void Subscribe()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            if (EditorApplication.isPlayingOrWillChangePlaymode == false)
            {
                OnPlayModeStateChanged(PlayModeStateChange.EnteredEditMode);
            }
        }

        public void Unsubscribe()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void Reset(int balance)
        {
            Reset(balance, null);
        }

        private void Reset(int balance, List<string> openSkinIds)
        {
            SavesYG data = new SavesYG();

            data.Balance = balance;

            if (openSkinIds != null)
            {
                data.OpenSkinIds = openSkinIds;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                SessionState.SetString(PendingResetKey, JsonUtility.ToJson(data));
                EditorApplication.isPlaying = false;
                return;
            }

            WriteResetData(data);
        }

        private void UnlockSkins()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (File.Exists(_editorSavesPath) == false)
            {
                SavesYG defaultData = new SavesYG();

                Reset(defaultData.Balance, CollectSkinIds());
                return;
            }

            SavesYG data = JsonUtility.FromJson<SavesYG>(File.ReadAllText(_editorSavesPath));

            if (data == null)
            {
                throw new InvalidOperationException(
                    $"[PlayerDataResetTool] Failed to parse saves at {_editorSavesPath}.");
            }

            data.OpenSkinIds = MergeSkinIds(data.OpenSkinIds, CollectSkinIds());

            File.WriteAllText(_editorSavesPath, JsonUtility.ToJson(data, true));
            AssetDatabase.Refresh();
        }

        private void WriteResetData(SavesYG data)
        {
            File.WriteAllText(_editorSavesPath, JsonUtility.ToJson(data, true));
            YG2.saves = data;
            AssetDatabase.Refresh();
        }

        private List<string> CollectSkinIds()
        {
            ShopContent shopContent = AssetDatabase.LoadAssetAtPath<ShopContent>(ShopContentPath);

            if (shopContent == null)
            {
                throw new InvalidOperationException($"[PlayerDataResetTool] No ShopContent at {ShopContentPath}.");
            }

            List<string> ids = new List<string>();

            for (int i = 0; i < shopContent.SkinItems.Count; i++)
            {
                SkinItem skin = shopContent.SkinItems[i];

                if (skin != null)
                {
                    ids.Add(skin.Id);
                }
            }

            return ids;
        }

        private List<string> MergeSkinIds(List<string> currentIds, List<string> catalogIds)
        {
            List<string> mergedIds = new List<string>();

            if (currentIds != null)
            {
                mergedIds.AddRange(currentIds);
            }

            for (int i = 0; i < catalogIds.Count; i++)
            {
                if (mergedIds.Contains(catalogIds[i]) == false)
                {
                    mergedIds.Add(catalogIds[i]);
                }
            }

            return mergedIds;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            string json = SessionState.GetString(PendingResetKey, string.Empty);

            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            WriteResetData(JsonUtility.FromJson<SavesYG>(json));
            SessionState.EraseString(PendingResetKey);
        }
    }
}
