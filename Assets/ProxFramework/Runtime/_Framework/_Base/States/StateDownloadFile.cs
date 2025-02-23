﻿using Cysharp.Threading.Tasks;
using ProxFramework.Asset;
using ProxFramework.Logger;
using ProxFramework.StateMachine;
using YooAsset;

namespace ProxFramework.Base
{
    public class StateDownloadFile : State
    {
        public override void Init()
        {
        }

        public override async void Enter()
        {
            LogModule.Info("start download file");
            AssetModule.downloaderOperation.OnDownloadErrorCallback = OnDownloadErrorCallback;
            AssetModule.downloaderOperation.OnDownloadProgressCallback = OnDownloadProgress;
            AssetModule.downloaderOperation.BeginDownload();
            await AssetModule.downloaderOperation.ToUniTask();
            if (AssetModule.downloaderOperation.Status == EOperationStatus.Succeed)
            {
                ChangeState<StatePatchDone>();
            }
        }

        public override void Exit()
        {
        }
        
        public void OnDownloadErrorCallback(string filename, string error)
        {
            LogModule.Error($"download error: {filename}, {error}");
        }

        public void OnDownloadProgress(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes,
            long currentDownloadBytes)
        {
            LogModule.Info($" download progress: {currentDownloadCount}/{totalDownloadCount}, {currentDownloadBytes}/{totalDownloadBytes}");
        }
            
    }
}
