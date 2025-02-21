using System;
using ProxFramework.Asset;
using UnityEditor;
using UnityEngine;

namespace ProxFramework.Localization
{
    public abstract class LocalizedAsset<T> : LocalizedBehaviour where T : UnityEngine.Object
    {
        [SerializeField] protected string assetPath;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            UpdateAssetPath();
        }

        protected virtual void UpdateAssetPath()
        {
            var asset = GetComponent<T>();
            if (asset == null)
            {
                PLogger.Warning($"Localized {typeof(T)} is null. {gameObject.name}");
                assetPath = string.Empty;
                return;
            }

            assetPath = AssetDatabase.GetAssetPath(asset);
            // 自动标记脏数据以保存修改
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif
        public override async void ApplyLocalization()
        {
            try
            {
                if (string.IsNullOrEmpty(assetPath))
                {
                    PLogger.Warning($"Asset path is empty, please set it first. {gameObject.name}");
                    return;
                }

                var localizedPath = LocalizationModule.GetLocalizeAsstPath(assetPath);
                T asset = null;
                asset = await AssetModule.LoadAssetAsync<T>(localizedPath);

                if (asset == null)
                {
                    PLogger.Warning($"Localized asset is null. {gameObject.name}");
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