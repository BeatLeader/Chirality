using BeatmapSaveDataVersion3;
using Chirality.Configuration;
using HarmonyLib;
using SongCore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Chirality
{
    [HarmonyPatch (typeof(StandardLevelDetailView))]
    internal class StandardLevelDetailViewPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(StandardLevelDetailView.SetContent), typeof(BeatmapLevel), typeof(BeatmapDifficultyMask), typeof(HashSet<BeatmapCharacteristicSO>), typeof(BeatmapDifficulty), typeof(BeatmapCharacteristicSO), typeof(PlayerData))]
        static void Prefix(BeatmapLevel level)
        {
            //Plugin.Log.Debug("SetContent");

            if (PluginConfig.Instance.enabled == false || BS_Utils.Plugin.LevelData.Mode == BS_Utils.Gameplay.Mode.Multiplayer || BS_Utils.Plugin.LevelData.Mode == BS_Utils.Gameplay.Mode.Mission)
            {
                // This prevents prevents new diffs from being generated in mp but will not remove the ones generated recently while playing in solo then going into mp
                return;
            }

            // BS 1.20.0 Not supporting OST / DLC anymore
            if (level.levelID.StartsWith("custom_level") == false)
            {
                return;
            }

            if (level.GetBeatmapKeys().FirstOrDefault() == null)
            {
                return;
            }

            if (level.GetCharacteristics().Any(i => i.serializedName.StartsWith(Plugin.prefix_list[0])))
            {
                // This is needed to keep the diffs from multiplying like rabbits
                // however it also means modes cant be switched on the fly for maps with recently generated diffs (unless we remove the generated diffs)
                return;
            }

            if (level.GetBeatmapKeys().Any(key => {
                return SongCore.Collections.RetrieveDifficultyData(level, key).additionalDifficultyData._requirements.Contains("Mapping Extensions") ||
                        SongCore.Collections.RetrieveDifficultyData(level, key).additionalDifficultyData._requirements.Contains("Noodle Extensions");
            })) {
                return;
            }

            var newKeys = new Dictionary<(BeatmapCharacteristicSO characteristic, BeatmapDifficulty difficulty), BeatmapBasicData>();
            foreach (var key in level.beatmapBasicData.Keys) {
                newKeys[key] = level.beatmapBasicData[key];
            }

            foreach (var chara in level.GetCharacteristics()) {
                foreach (var prefix in Plugin.prefix_list)
                {
                    var newChara = SongCore.Collections.customCharacteristics.FirstOrDefault(x => x.serializedName.StartsWith(prefix));
	                if (newChara == null) continue;

	                foreach (var diff in level.GetDifficulties(chara)) {
                        var newKey = (newChara, diff);
                        if (newKeys.ContainsKey(newKey)) continue;

                        var data = level.GetDifficultyBeatmapData(chara, diff);
                        if (data == null) continue;
		                newKeys.Add(newKey, data);
	                }
                }
            }

            level.GetType().GetField("beatmapBasicData", BindingFlags.Instance | BindingFlags.Public)
	            ?.SetValue(level, newKeys);
            level.GetType().GetField("_beatmapKeysCache", BindingFlags.Instance | BindingFlags.NonPublic)
	            ?.SetValue(level, null);
            level.GetType().GetField("_characteristicsCache", BindingFlags.Instance | BindingFlags.NonPublic)
	            ?.SetValue(level, null);
        }
    }

    //[HarmonyPatch(typeof(BeatmapDataLoader))]
    //class BeatmapDataLoaderPatch {
	   // [HarmonyPrefix]
	   // [HarmonyPatch("LoadBasicBeatmapDataAsync")]
	   // static void ResetCharacteristic(ref BeatmapKey beatmapKey) {
    //        var mode = beatmapKey.beatmapCharacteristic;
    //        var modeName = mode.serializedName;
    //        Plugin.Log.Error($"LoadBasicBeatmapDataAsync {modeName}");

    //        var prefix = Plugin.prefix_list.FirstOrDefault(p => modeName.StartsWith(p));
		  //  if (prefix == null) return;

    //        var beatmapCharacteristicSO = BeatmapCharacteristicSO.CreateInstance<BeatmapCharacteristicSO>();
    //        beatmapCharacteristicSO._icon = mode._icon;
    //        beatmapCharacteristicSO._descriptionLocalizationKey = mode.descriptionLocalizationKey;
    //        beatmapCharacteristicSO._characteristicNameLocalizationKey = mode.characteristicNameLocalizationKey;
    //        beatmapCharacteristicSO._serializedName = modeName.Replace(prefix, "");
    //        beatmapCharacteristicSO._compoundIdPartName = mode.compoundIdPartName;
    //        beatmapCharacteristicSO._sortingOrder = mode.sortingOrder;
    //        beatmapCharacteristicSO._containsRotationEvents = mode.containsRotationEvents;
    //        beatmapCharacteristicSO._requires360Movement = mode.requires360Movement;
    //        beatmapCharacteristicSO._numberOfColors = mode.numberOfColors;
		    
		  //  beatmapKey = new BeatmapKey(
    //            beatmapKey.levelId, 
    //            beatmapCharacteristicSO, 
    //            beatmapKey.difficulty);
	   // }
    //}

    [HarmonyPatch(typeof(BeatmapDataLoader))]
    class BeatmapDataLoaderPatch {
        public static BeatmapCharacteristicSO MakeDefault(BeatmapCharacteristicSO mode, string prefix) {
            var beatmapCharacteristicSO = BeatmapCharacteristicSO.CreateInstance<BeatmapCharacteristicSO>();
            beatmapCharacteristicSO._icon = mode._icon;
            beatmapCharacteristicSO._descriptionLocalizationKey = mode.descriptionLocalizationKey;
            beatmapCharacteristicSO._characteristicNameLocalizationKey = mode.characteristicNameLocalizationKey;
            beatmapCharacteristicSO._serializedName = mode.serializedName.Replace(prefix, "");
            beatmapCharacteristicSO._compoundIdPartName = mode.compoundIdPartName.Replace(prefix, "");
            beatmapCharacteristicSO._sortingOrder = mode.sortingOrder;
            beatmapCharacteristicSO._containsRotationEvents = mode.containsRotationEvents;
            beatmapCharacteristicSO._requires360Movement = mode.requires360Movement;
            beatmapCharacteristicSO._numberOfColors = mode.numberOfColors;

            return beatmapCharacteristicSO;
        }

        static MethodInfo TargetMethod() => AccessTools.FirstMethod(typeof(BeatmapDataLoader),
            m => m.Name == nameof(BeatmapDataLoader.LoadBasicBeatmapDataAsync) &&
                 m.GetParameters().Any(p => p.ParameterType == typeof(IBeatmapLevelData)));

        [HarmonyPrefix]
	    static void ResetCharacteristic(ref BeatmapKey beatmapKey, BeatmapDataLoader __instance) {
            var mode = beatmapKey.beatmapCharacteristic;
            var modeName = mode.serializedName;

            var prefix = Plugin.prefix_list.FirstOrDefault(p => modeName.StartsWith(p));
            if (prefix == null) return;

            beatmapKey = new BeatmapKey(beatmapKey.levelId, MakeDefault(mode, prefix), beatmapKey.difficulty);
        }
    }

    [HarmonyPatch(typeof(BeatmapDataLoader))]
    class BeatmapDataLoaderPatch2 {
        static MethodInfo TargetMethod() => AccessTools.FirstMethod(typeof(BeatmapDataLoader),
            m => m.Name == nameof(BeatmapDataLoader.LoadBeatmapDataAsync) &&
                 m.GetParameters().Any(p => p.ParameterType == typeof(IBeatmapLevelData)));

        public static string? invertMode = null; 

        [HarmonyPrefix]
	    static void LoadBeatmapDataAsyncPrefix(ref BeatmapKey beatmapKey, BeatmapDataLoader __instance, ref bool enableBeatmapDataCaching) {
            var mode = beatmapKey.beatmapCharacteristic;
            var modeName = mode.serializedName;

            var prefix = Plugin.prefix_list.FirstOrDefault(p => modeName.StartsWith(p));
            if (prefix == null) return;

            invertMode = prefix;
            enableBeatmapDataCaching = false;
        }
    }

    [HarmonyPatch(typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader))]
    class BeatmapDataLoaderV3Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("GetBeatmapDataFromSaveData")]
        static void ResetCharacteristic(ref BeatmapSaveDataVersion3.BeatmapSaveData beatmapSaveData)
        { 
            if (BeatmapDataLoaderPatch2.invertMode != null) {

                bool is_ME = false; // Chaos generator
                bool is_ME_or_NE = false; // Yeets walls

                //// Check for OST now done uptop. Removed OST support for 1.20.0
                //if (SongCore.Collections.RetrieveDifficultyData(i, key).additionalDifficultyData._requirements.Contains("Mapping Extensions"))
                //{
                //    is_ME = true;

                //    Plugin.Log.Debug("ME map: raising hell");
                //}

                //if (is_ME || SongCore.Collections.RetrieveDifficultyData(i, key).additionalDifficultyData._requirements.Contains("Noodle Extensions"))
                //{
                //    is_ME_or_NE = true;

                //    Plugin.Log.Debug("ME-NE map: yeeting walls");
                //}

                int numberOfLines = 4;
                switch (BeatmapDataLoaderPatch2.invertMode)
                {
                    case "Vertical": 
                        beatmapSaveData = V3.MirrorTransforms.Mirror_Vertical(beatmapSaveData, false, is_ME_or_NE, is_ME); 
                        break;
                    case "Horizontal": 
                        beatmapSaveData = V3.MirrorTransforms.Mirror_Horizontal(beatmapSaveData, numberOfLines, false, is_ME_or_NE, is_ME); 
                        break;
                    case "Inverse": 
                        beatmapSaveData = V3.MirrorTransforms.Mirror_Inverse(beatmapSaveData, numberOfLines, true, true, is_ME_or_NE, is_ME); 
                        break;
                    case "Inverted": 
                        beatmapSaveData = V3.MirrorTransforms.Mirror_Inverse(beatmapSaveData, numberOfLines, false, false, is_ME_or_NE, is_ME); 
                        break;
                    
                }
                BeatmapDataLoaderPatch2.invertMode = null;
            }
        }
    }

    [HarmonyPatch(typeof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader))]
    class BeatmapDataLoaderV2Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("GetBeatmapDataFromSaveData")]
        static void ResetCharacteristic(ref BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData beatmapSaveData)
        { 
            if (BeatmapDataLoaderPatch2.invertMode != null) {

                bool is_ME = false; // Chaos generator
                bool is_ME_or_NE = false; // Yeets walls

                //// Check for OST now done uptop. Removed OST support for 1.20.0
                //if (SongCore.Collections.RetrieveDifficultyData(i, key).additionalDifficultyData._requirements.Contains("Mapping Extensions"))
                //{
                //    is_ME = true;

                //    Plugin.Log.Debug("ME map: raising hell");
                //}

                //if (is_ME || SongCore.Collections.RetrieveDifficultyData(i, key).additionalDifficultyData._requirements.Contains("Noodle Extensions"))
                //{
                //    is_ME_or_NE = true;

                //    Plugin.Log.Debug("ME-NE map: yeeting walls");
                //}

                int numberOfLines = 4;
                switch (BeatmapDataLoaderPatch2.invertMode)
                {
                    case "Vertical": 
                        beatmapSaveData = V2.MirrorTransforms.Mirror_Vertical(beatmapSaveData, false, is_ME_or_NE, is_ME); 
                        break;
                    case "Horizontal": 
                        beatmapSaveData = V2.MirrorTransforms.Mirror_Horizontal(beatmapSaveData, numberOfLines, false, is_ME_or_NE, is_ME); 
                        break;
                    case "Inverse": 
                        beatmapSaveData = V2.MirrorTransforms.Mirror_Inverse(beatmapSaveData, numberOfLines, true, true, is_ME_or_NE, is_ME); 
                        break;
                    case "Inverted": 
                        beatmapSaveData = V2.MirrorTransforms.Mirror_Inverse(beatmapSaveData, numberOfLines, false, false, is_ME_or_NE, is_ME); 
                        break;
                    
                }
                BeatmapDataLoaderPatch2.invertMode = null;
            }
        }
    }

    [HarmonyPatch(typeof(FileSystemBeatmapLevelData))]
    class PatchBeatmapFile
    {
        [HarmonyPrefix]
        [HarmonyPatch("GetDifficultyBeatmap")]
        static void ResetCharacteristic(ref BeatmapKey beatmapKey, FileSystemBeatmapLevelData __instance)
        {
            var mode = beatmapKey.beatmapCharacteristic;
            var modeName = mode.serializedName;

            var prefix = Plugin.prefix_list.FirstOrDefault(p => modeName.StartsWith(p));
            if (prefix == null) return;

            Type expectedType = typeof(FileSystemBeatmapLevelData);
            Type type = __instance.GetType() == expectedType ? expectedType : __instance.GetType().BaseType;
            if (type != expectedType) Plugin.Log.Error($"Unrecognized filesystem data type {__instance.GetType()} {__instance.GetType().Assembly.FullName}");

            var difficultyBeatmaps =
                (Dictionary<(BeatmapCharacteristicSO, BeatmapDifficulty), FileDifficultyBeatmap>)type
                    .GetField("_difficultyBeatmaps", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(__instance);

            string normalCharName = beatmapKey.beatmapCharacteristic.serializedName.Replace(prefix, "");
            var diff = beatmapKey.difficulty;
            var entryForNormalCharacteristic = difficultyBeatmaps.FirstOrDefault(x => x.Key.Item1.serializedName == normalCharName && x.Key.Item2 == diff);

            beatmapKey = new BeatmapKey(beatmapKey.levelId, entryForNormalCharacteristic.Key.Item1, beatmapKey.difficulty);
        }
    }

    [HarmonyPatch(typeof(PlayerData))]
    class PatchPlayerData1
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerData.GetOrCreatePlayerLevelStatsData), new Type[] { typeof(BeatmapKey) }, new ArgumentType[] { ArgumentType.Ref })]
        static void UseDefaultCharacteristic(ref BeatmapKey beatmapKey)
        {
            var modeName = beatmapKey.beatmapCharacteristic.serializedName;
            var prefix = Plugin.prefix_list.FirstOrDefault(p => modeName.StartsWith(p));
            if (prefix != null)
            {
                beatmapKey = new BeatmapKey(beatmapKey.levelId, BeatmapDataLoaderPatch.MakeDefault(beatmapKey.beatmapCharacteristic, prefix), beatmapKey.difficulty);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerData))]
    class PatchPlayerData2 {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerData.GetOrCreatePlayerLevelStatsData), new Type[] { typeof(string), typeof(BeatmapDifficulty), typeof(BeatmapCharacteristicSO) })]
        static void UseDefaultCharacteristic(ref BeatmapCharacteristicSO beatmapCharacteristic)
        {
            var modeName = beatmapCharacteristic.serializedName;
            var prefix = Plugin.prefix_list.FirstOrDefault(p => modeName.StartsWith(p));
            if (prefix != null)
            {
                beatmapCharacteristic = BeatmapDataLoaderPatch.MakeDefault(beatmapCharacteristic, prefix);
            }
        }
    }
}