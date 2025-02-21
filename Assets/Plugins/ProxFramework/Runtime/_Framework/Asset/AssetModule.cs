using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProxFramework.Base;
using ProxFramework.Runtime.Settings;
using UnityEditor;
using UnityEngine;
using YooAsset;

namespace ProxFramework.Asset
{
    internal class ResLogger : YooAsset.ILogger
    {
        public void Log(string message)
        {
            PLogger.Info(message);
        }

        public void Warning(string message)
        {
            PLogger.Warning(message);
        }

        public void Error(string message)
        {
            PLogger.Error(message);
        }

        public void Exception(Exception exception)
        {
            PLogger.Exception("Asset Module Exception:", exception);
        }
    }

    public static partial class AssetModule
    {
        private static int _downloadingMaxNum;
        private static int _failedTryAgain;
        private static Dictionary<string, ResourcePackage> _mapNameToResourcePackage = new();
        public static TaskCtsModule.CtsInfo ctsInfo;
        public static CancellationToken CtsToken => ctsInfo.cts.Token;
        public static string defaultPkgName;
        public static bool initialized = false;

        public static void Initialize()
        {
            if (!YooAssets.Initialized)
            {
                YooAssets.Initialize(new ResLogger());
            }

            ctsInfo = TaskCtsModule.GetCts();
            _downloadingMaxNum = SettingsUtil.GlobalSettings.assetSettings.maxDownloadingNum;
            _failedTryAgain = SettingsUtil.GlobalSettings.assetSettings.failedTryAgain;
            defaultPkgName = SettingsUtil.GlobalSettings.assetSettings.packages[0].name;
            initialized = true;
        }

        public static async UniTask<EOperationStatus> InitPackages()
        {
            foreach (var packageDefine in SettingsUtil.GlobalSettings.assetSettings.packages)
            {
                var status = await InitPackage(packageDefine);
                if (status != EOperationStatus.Failed) continue;
                return EOperationStatus.Failed;
            }

            return EOperationStatus.Succeed;
        }


        public static async UniTask<EOperationStatus> InitPackage(AssetPackageDefine packageDefine)
        {
            var packageName = packageDefine.name;
            var pkgPlayMode = SettingsUtil.GlobalSettings.PlayMode;
            var defaultHostServer = GetHostServerURL();
            var fallbackHostServer = GetHostServerURL();
            IDecryptionServices decryptionServices = packageDefine.encryptType switch
            {
                AssetEncryptType.Offset => new FileOffsetDecryption(),
                AssetEncryptType.Xor => new FileStreamDecryption(),
                _ => null
            };
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(packageDefine.devHostUrl) && pkgPlayMode == EPlayMode.EditorSimulateMode)
            {
                pkgPlayMode = EPlayMode.HostPlayMode;
                defaultHostServer = packageDefine.devHostUrl;
            }
#endif
            var package = YooAssets.TryGetPackage(packageName) ?? YooAssets.CreatePackage(packageName);
            _mapNameToResourcePackage.TryAdd(packageName, package);

            InitializationOperation initializationOperation = null;
            if (pkgPlayMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters =
                    FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                initializationOperation = package.InitializeAsync(createParameters);
            }
            else if (pkgPlayMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.BuildinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuildinFileSystemParameters(decryptionServices);
                initializationOperation = package.InitializeAsync(createParameters);
            }
            else if (pkgPlayMode == EPlayMode.HostPlayMode)
            {
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                var createParameters = new HostPlayModeParameters();
                createParameters.BuildinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuildinFileSystemParameters(decryptionServices);
                createParameters.CacheFileSystemParameters =
                    FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices, decryptionServices);
                initializationOperation = package.InitializeAsync(createParameters);
            }
            else if (pkgPlayMode == EPlayMode.WebPlayMode)
            {
                var createParameters = new WebPlayModeParameters();
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                createParameters.WebServerFileSystemParameters =
 WechatFileSystemCreater.CreateWechatFileSystemParameters(remoteServices);
#else
                createParameters.WebServerFileSystemParameters =
                    FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
#endif
                initializationOperation = package.InitializeAsync(createParameters);
            }

            if (initializationOperation == null)
            {
                return EOperationStatus.Failed;
            }

            await initializationOperation.ToUniTask(cancellationToken: ctsInfo.cts.Token);
            if (initializationOperation.Status == EOperationStatus.Failed)
            {
                PLogger.Error($"Init package {packageName} failed. error: {initializationOperation.Error}");
            }

            return initializationOperation.Status;
        }

        private static string GetHostServerURL()
        {
            var hostUrl = SettingsUtil.GlobalSettings.assetSettings.assetCdn;
            var appVersion = Application.version;

#if UNITY_EDITOR
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                return $"{hostUrl}/android/{appVersion}";
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
                return $"{hostUrl}/ios/{appVersion}";
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
                return $"{hostUrl}/webgl/{appVersion}";
            else
                return $"{hostUrl}/pc/{appVersion}";
#else
        if (Application.platform == RuntimePlatform.Android)
            return $"{hostUrl}/android/{appVersion}";
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
            return $"{hostUrl}/ios/{appVersion}";
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
            return $"{hostUrl}/webgl/{appVersion}";
        else
            return $"{hostUrl}/pc/{appVersion}";
#endif
        }

        public static ResourcePackage[] GetAllPackages()
        {
            return _mapNameToResourcePackage.Values.ToArray();
        }

        public static ResourcePackage GetPackage(string packageName)
        {
            if (_mapNameToResourcePackage.TryGetValue(packageName, out var package))
            {
                return package;
            }

            PLogger.Error($"Package {packageName} not found");
            return null;
        }

        public static bool TryGetPackage(string packageName, out ResourcePackage package)
        {
            return _mapNameToResourcePackage.TryGetValue(packageName, out package);
        }

        public static bool TryGetContainsPackage(string assetPath, out ResourcePackage outPackage)
        {
            outPackage = null;
            foreach (var package in _mapNameToResourcePackage.Values)
            {
                if (package.CheckLocationValid(assetPath))
                {
                    outPackage = package;
                    return true;
                }
            }

            return false;
        }

        public static void Shutdown()
        {
        }
    }
}