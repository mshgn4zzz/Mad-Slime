using System;
using System.Collections.Generic;
using System.IO;
using Shop;
using Skins;
using UnityEditor;
using UnityEngine;
using YG;

namespace EditorTools
{
    public class PlayerDataResetTool
    {
        private const string EditorSavesPath = "Assets/PluginYourGames/Editor/SavesEditorYG2.json";
        private const string ShopContentPath = "Assets/MadSlime/Scriptables/Shop/ShopContent.asset";
        private const string PendingResetKey = "MadSlime.PlayerDataResetTool.PendingReset";
        private const int EmptyBalance = 0;
        private const int TestBalance = 9999999;

        [InitializeOnLoadMethod]
        private static void SubscribeToPlayModeChanges()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            string json = SessionState.GetString(PendingResetKey, string.Empty);

            if (string.IsNullOrEmpty(json) == true)
            {
                return;
            }

            WriteResetData(JsonUtility.FromJson<SavesYG>(json));
            SessionState.EraseString(PendingResetKey);
        }

        [MenuItem("Mad Slime/Reset Player Data/From Zero")]
        public static void ResetPlayerData()
        {
            Reset(EmptyBalance);
        }

        [MenuItem("Mad Slime/Reset Player Data/From Zero With Money")]
        public static void ResetPlayerDataWithMoney()
        {
            Reset(TestBalance);
        }

        [MenuItem("Mad Slime/Reset Player Data/From Zero With Money And Skins")]
        public static void ResetPlayerDataWithMoneyAndSkins()
        {
            Reset(TestBalance, CollectSkinIds());
        }

        [MenuItem("Mad Slime/Unlock All Skins")]
        public static void UnlockAllSkins()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode == true)
            {
                Debug.LogWarning("[PlayerDataResetTool] Exit play mode first — the running session would overwrite the changes.");
                return;
            }

            if (File.Exists(EditorSavesPath) == false)
            {
                SavesYG defaultData = new SavesYG();
                Reset(defaultData.Balance, CollectSkinIds());
                return;
            }

            SavesYG data = JsonUtility.FromJson<SavesYG>(File.ReadAllText(EditorSavesPath));

            if (data == null)
            {
                Debug.LogError($"[PlayerDataResetTool] Failed to parse saves at {EditorSavesPath}.");
                return;
            }

            data._openSkinIds = MergeSkinIds(data._openSkinIds, CollectSkinIds());

            File.WriteAllText(EditorSavesPath, JsonUtility.ToJson(data, true));
            AssetDatabase.Refresh();

            Debug.Log($"[PlayerDataResetTool] all skins unlocked, open count {data._openSkinIds.Count}");
        }

        private static void Reset(int balance)
        {
            Reset(balance, null);
        }

        private static void Reset(int balance, List<string> openSkinIds)
        {
            SavesYG data = new SavesYG();
            data.Balance = balance;

            if (openSkinIds != null)
            {
                data._openSkinIds = openSkinIds;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode == true)
            {
                SessionState.SetString(PendingResetKey, JsonUtility.ToJson(data));
                EditorApplication.isPlaying = false;
                return;
            }

            WriteResetData(data);
        }

        private static void WriteResetData(SavesYG data)
        {
            File.WriteAllText(EditorSavesPath, JsonUtility.ToJson(data, true));
            YG2.saves = data;
            AssetDatabase.Refresh();

            Debug.Log($"[PlayerDataResetTool] all player data reset, balance set to {data.Balance}, open skins {data._openSkinIds.Count}");
        }

        private static List<string> CollectSkinIds()
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

        private static List<string> MergeSkinIds(List<string> currentIds, List<string> catalogIds)
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
    }
}
