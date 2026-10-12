using System.IO;
using UnityEditor;
using UnityEngine;
using YG;

namespace EditorTools
{
    public class PlayerDataResetTool
    {
        private const string EditorSavesPath = "Assets/PluginYourGames/Editor/SavesEditorYG2.json";
        private const int EmptyBalance = 0;
        private const int TestBalance = 9999999;

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

        private static void Reset(int balance)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode == true)
            {
                Debug.LogWarning("[PlayerDataResetTool] Exit play mode first — the running session would overwrite the reset.");
                return;
            }

            SavesYG data = new SavesYG();
            data.Balance = balance;

            string json = JsonUtility.ToJson(data, true);

            File.WriteAllText(EditorSavesPath, json);
            AssetDatabase.Refresh();

            Debug.Log($"[PlayerDataResetTool] all player data reset, balance set to {balance}");
        }
    }
}
