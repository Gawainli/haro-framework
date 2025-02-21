using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ProxFramework.Asset;
using ProxFramework.Runtime.Settings;
using TMPro;
using UnityEngine;

namespace ProxFramework.Localization
{
    public static class LocalizationModule
    {
        public struct LangFontMapping
        {
            public TMP_FontAsset fontAsset;
            public Material material;
            public float sizeScaler;
        }

        public static bool initialized { get; private set; }
        public static SystemLanguage DefaultLanguage { get; private set; }
        public static SystemLanguage CurrentLanguage { get; private set; }
        public static event Action OnLanguageChanged;

        private static IL10NTable _table;
        private static List<SystemLanguage> _supportLanguages = new();

        public static TMP_FontAsset CurrentFont { get; private set; }
        public static Material CurrentFontMaterial { get; private set; }
        public static float CurrentFontSize { get; private set; }
        private static Dictionary<SystemLanguage, LangFontMapping> _mapCultureCode2FontMappings = new();

        public static void Initialize()
        {
            DefaultLanguage = SettingsUtil.GlobalSettings.l10NSettings.defaultLanguage;
            CurrentLanguage = DefaultLanguage;
            _supportLanguages.AddRange(SettingsUtil.GlobalSettings.l10NSettings.supportedLanguages);

            initialized = true;
        }

        public static void Shutdown()
        {
            _table = null;
            _supportLanguages.Clear();
            initialized = false;
        }

        public static void SetTable(IL10NTable table)
        {
            _table = table;
        }

        public static async UniTask PreloadFonts()
        {
            foreach (var langFontSettings in SettingsUtil.GlobalSettings.l10NSettings.l10NFontSettings)
            {
                var fontAsset = await AssetModule.LoadAssetAsync<TMP_FontAsset>(langFontSettings.fontAssetPath);
                Material material = null;
                if (!string.IsNullOrEmpty(langFontSettings.fontMaterialPath))
                {
                    material = await AssetModule.LoadAssetAsync<Material>(langFontSettings.fontMaterialPath);
                }

                if (fontAsset != null)
                {
                    _mapCultureCode2FontMappings.TryAdd(langFontSettings.language,
                        new LangFontMapping()
                        {
                            fontAsset = fontAsset, material = material, sizeScaler = langFontSettings.fontSizeScaler
                        });
                }
                else
                {
                    PLogger.Warning(
                        $"LocalizationModule.PreloadFonts: FontAsset {langFontSettings.fontAssetPath} is null");
                }
            }
        }

        public static void ChangeLanguage(SystemLanguage lang)
        {
            if (!_supportLanguages.Contains(lang))
            {
                Debug.LogError($"Language {lang} is not supported");
                return;
            }

            CurrentLanguage = lang;
            // ChangeFont(CurrentLanguage);
            OnLanguageChanged?.Invoke();
        }

        private static void ChangeFont(SystemLanguage lang)
        {
            if (_mapCultureCode2FontMappings.TryGetValue(lang, out var fontMapping))
            {
                CurrentFont = fontMapping.fontAsset;
                CurrentFontMaterial = fontMapping.material;
                CurrentFontSize = fontMapping.sizeScaler;
            }
        }

        public static string GetLocalizeAsstPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                PLogger.Warning("LocalizationModule.GetLocalizeAsstPath: Path is null or empty");
                return "";
            }
            
            var code = GetSystemLanguageCultureCode(CurrentLanguage);

            foreach (var pathPrefix in SettingsUtil.GlobalSettings.l10NSettings.l10NAssetsPathPrefixes)
            {
                if (path.StartsWith(pathPrefix))
                {
                    return path.Replace(pathPrefix, $"{pathPrefix}/localized/{code}");
                }
            }

            return "";
        }

        public static string GetLocalizeValue(string key)
        {
            if (_table == null)
            {
                PLogger.Warning("LocalizationModule.GetLocalizeValue: Table is null");
                return "";
            }
            
            var code = GetSystemLanguageCultureCode(CurrentLanguage);

            if (!_table.TryGetValue(code, key, out var value))
            {
                PLogger.Warning($"LocalizationModule.GetLocalizeValue: Key {key} not found");
                return "";
            }

            return value;
        }
        
         public static string GetSystemLanguageCultureCode(SystemLanguage lang)
        {
            switch (lang)
            {
                case SystemLanguage.Afrikaans: return "af";
                case SystemLanguage.Arabic: return "ar";
                case SystemLanguage.Basque: return "eu";
                case SystemLanguage.Belarusian: return "be";
                case SystemLanguage.Bulgarian: return "bg";
                case SystemLanguage.Catalan: return "ca";
                // case SystemLanguage.Chinese: return "zh-CN";
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified: return "zh-hans";
                case SystemLanguage.ChineseTraditional: return "zh-hant";
                case SystemLanguage.SerboCroatian: return "hr";
                case SystemLanguage.Czech: return "cs";
                case SystemLanguage.Danish: return "da";
                case SystemLanguage.Dutch: return "nl";
                case SystemLanguage.English: return "en";
                case SystemLanguage.Estonian: return "et";
                case SystemLanguage.Faroese: return "fo";
                case SystemLanguage.Finnish: return "fi";
                case SystemLanguage.French: return "fr";
                case SystemLanguage.German: return "de";
                case SystemLanguage.Greek: return "el";
                case SystemLanguage.Hebrew: return "he";
                case SystemLanguage.Hungarian: return "hu";
                case SystemLanguage.Icelandic: return "is";
                case SystemLanguage.Indonesian: return "id";
                case SystemLanguage.Italian: return "it";
                case SystemLanguage.Japanese: return "ja";
                case SystemLanguage.Korean: return "ko";
                case SystemLanguage.Latvian: return "lv";
                case SystemLanguage.Lithuanian: return "lt";
                case SystemLanguage.Norwegian: return "no";
                case SystemLanguage.Polish: return "pl";
                case SystemLanguage.Portuguese: return "pt";
                case SystemLanguage.Romanian: return "ro";
                case SystemLanguage.Russian: return "ru";
                case SystemLanguage.Slovak: return "sk";
                case SystemLanguage.Slovenian: return "sl";
                case SystemLanguage.Spanish: return "es";
                case SystemLanguage.Swedish: return "sv";
                case SystemLanguage.Thai: return "th";
                case SystemLanguage.Turkish: return "tr";
                case SystemLanguage.Ukrainian: return "uk";
                case SystemLanguage.Vietnamese: return "vi";
#if UNITY_2022_2_OR_NEWER
                case SystemLanguage.Hindi: return "hi";
#endif
                case SystemLanguage.Unknown:
                default: return "";
            }
        }
    }
}