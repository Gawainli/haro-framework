using System;
using Cysharp.Threading.Tasks;
using ProxFramework;
using ProxFramework.Asset;
using ProxFramework.StateMachine;

namespace Prox.GameName.Runtime
{
    public class StateDownloadFile : State
    {
        private PatchAsyncOperation _patchAsyncOperation;

        public override void Init()
        {
        }

        public override async void Enter()
        {
            try
            {
                _patchAsyncOperation = fsm.Blackboard.GetObjectValue<PatchAsyncOperation>("patchOp");
                _patchAsyncOperation.BeginDownload();
                await _patchAsyncOperation;
                if (_patchAsyncOperation.succeed)
                {
                    ChangeState<StatePatchDone>();
                }
                else
                {
                    PLogger.Error($"Patch error: {_patchAsyncOperation.errorInfo}");
                }
            }
            catch (Exception e)
            {
                PLogger.Exception("Download File Exception:", e);
            }
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayProgressBar($"下载资源, 共{_patchAsyncOperation.totalDownloadCount}个",
                $"下载资源中... {_patchAsyncOperation.CurrentDownloadCount}/{_patchAsyncOperation.totalDownloadCount}",
                _patchAsyncOperation.Progress);
#else
            PLogger.Info(
                $"下载资源中... {_patchAsyncOperation.CurrentDownloadCount}/{_patchAsyncOperation.totalDownloadCount} progress: {_patchAsyncOperation.Progress}");
#endif
        }

        public override void Exit()
        {
        }
    }
}