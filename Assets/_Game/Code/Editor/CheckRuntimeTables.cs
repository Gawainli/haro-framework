using System;
using GameName.Core;
using ProxFramework;
using UnityEditor;

namespace GameName.Editor
{
    public class CheckRuntimeTables
    {
        [MenuItem("Tools/Check Runtime Tables")]
        public static async void CheckTables()
        {
            try
            {
                await DataSystem.Initialize();
                PLogger.Info($"{DataSystem.Tables.TbTextL10N.DataList.Count}");
                PLogger.Info("CheckRuntimeTables.CheckTables: Tables checked");
            }
            catch (Exception e)
            {
                PLogger.Exception($"CheckRuntimeTables.CheckTables: ", e);
            }
            // var str = "bbbb";
            // PLogger.Error($"AAAAAAAAAAAAA {new ArgumentException()}");
        }
    }
}