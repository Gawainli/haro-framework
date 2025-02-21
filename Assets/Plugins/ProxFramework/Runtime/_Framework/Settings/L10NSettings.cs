using System.Collections.Generic;
using UnityEngine;

namespace ProxFramework.Runtime.Settings
{
    [System.Serializable]
    public class L10NFontSettings
    {
        public SystemLanguage language;
        public string fontAssetPath;
        public string fontMaterialPath;
        public float fontSizeScaler = 1;
    }

    [System.Serializable]
    public class L10NSettings
    {
        public SystemLanguage editorCheckLanguage;
        public SystemLanguage defaultLanguage;
        public List<SystemLanguage> supportedLanguages;
        public List<L10NFontSettings> l10NFontSettings;
        public List<string> l10NAssetsPathPrefixes;
    }
}