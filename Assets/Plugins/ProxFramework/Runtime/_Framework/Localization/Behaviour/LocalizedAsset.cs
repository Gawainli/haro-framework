using System;
using ProxFramework.Asset;
using UnityEditor;

namespace ProxFramework.Localization
{
    public abstract class LocalizedAsset<T> : LocalizedBehaviour where T : UnityEngine.Object
    {
        // public override void AutoSetL10NKey()
        // {
        //     var asset = GetComponent<T>();
        //     if (asset == null)
        //     {
        //         PLogger.Warning($"Localized {typeof(T)} is null. {gameObject.name}");
        //         localizationKey = string.Empty;
        //         return;
        //     }
        //
        //     localizationKey = AssetDatabase.GetAssetPath(asset);
        //     // 自动标记脏数据以保存修改
        //     EditorUtility.SetDirty(this);
        //     AssetDatabase.SaveAssets();
        // }
        
        public override async void ApplyLocalization()
        {
            try
            {
                if (string.IsNullOrEmpty(localizationKey))
                {
                    PLogger.Warning($"Asset path is empty, please set it first. {gameObject.name}");
                    return;
                }

                var localizedPath = LocalizationModule.GetLocalizeAsstPath(localizationKey);
                var asset = await AssetModule.LoadAssetAsync<T>(localizedPath);

                if (asset == null)
                {
                    PLogger.Warning($"Localized asset is null. {gameObject.name}. asset path:{localizedPath}");
                    return;
                }

                ApplyAsset(asset);
            }
            catch (Exception e)
            {
                PLogger.Error($"LocalizedAsset.LoadLocalizedAsset: {e}");
            }
        }

        protected abstract void ApplyAsset(T asset);
    }
}