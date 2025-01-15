﻿using System;
using UnityEngine;

namespace ProxFramework.Runtime.Settings
{
    public static class SettingsUtil
    {
        private const string SettingsPath = "FrameworkSettings";

#if UNITY_EDITOR
        private static EditorDevSettings _editorDevSettings;

        public static EditorDevSettings EditorDevSettings
        {
            get
            {
                if (_editorDevSettings == null)
                {
                    var assetType = typeof(EditorDevSettings);
                    string[] assetGuids = UnityEditor.AssetDatabase.FindAssets($"t:{assetType}");
                    if (assetGuids.Length == 0)
                    {
                        _editorDevSettings = ScriptableObject.CreateInstance<EditorDevSettings>();
                        UnityEditor.AssetDatabase.CreateAsset(_editorDevSettings, "Assets/EditorDevSettings.asset");
                        UnityEditor.AssetDatabase.SaveAssets();
                        UnityEditor.AssetDatabase.Refresh();
                        PLogger.Warning(
                            $"No Editor Dev Settings found in project. Created new one at Assets/EditorDevSettings.asset");
                    }

                    if (assetGuids.Length > 1)
                    {
                        PLogger.Warning(
                            $"More than one Editor Dev Settings found in project. We use first one. Path:{UnityEditor.AssetDatabase.GUIDToAssetPath(assetGuids[0])}");
                    }

                    _editorDevSettings =
                        UnityEditor.AssetDatabase.LoadAssetAtPath<EditorDevSettings>(
                            UnityEditor.AssetDatabase.GUIDToAssetPath(assetGuids[0]));
                }

                return _editorDevSettings;
            }
        }

#endif

        private static FrameworkSettings _frameworkSettings;

        public static FrameworkSettings GlobalSettings
        {
            get
            {
                if (_frameworkSettings == null)
                {
                    _frameworkSettings = LoadSettingsFromResources();
                }

                return _frameworkSettings;
            }
        }

        private static FrameworkSettings LoadSettingsFromResources()
        {
#if UNITY_EDITOR
            var assetType = typeof(FrameworkSettings);
            string[] assetGuids = UnityEditor.AssetDatabase.FindAssets($"t:{assetType}");
            if (assetGuids.Length > 1)
            {
                foreach (var guid in assetGuids)
                {
                    PLogger.Error(
                        $"Could not had Multiple {assetType}. Repeated Path: {UnityEditor.AssetDatabase.GUIDToAssetPath(guid)}");
                }

                throw new Exception($"Could not had Multiple {assetType}");
            }
#endif
            var settings = Resources.Load<FrameworkSettings>(SettingsPath);
            if (settings != null) return settings;
            Debug.LogError($"Failed to load {typeof(FrameworkSettings)} from Resources at path: {SettingsPath}");
            throw new Exception(
                $"Failed to load {typeof(FrameworkSettings)} from Resources at path: {SettingsPath}");
        }
    }
}