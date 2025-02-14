﻿using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using ProxFramework;
using ProxFramework.Asset;
using ProxFramework.Runtime.Settings;

namespace GameName.Core
{
    public static class DataSystem
    {
        public static bool initialized = false;
        public static cfg.Tables Tables { get; private set; }

        private static readonly Dictionary<string, byte[]> _dataTableCache = new();

        public static async UniTask Initialize()
        {
            string[] tableNames = GetTableNames();
            if (tableNames == null || tableNames.Length == 0)
            {
                PLogger.Warning("DataSystem.Initialize: AllTableNames is null or length is 0");
                Tables = new cfg.Tables();
                return;
            }

            int tableTypeCode = GetTableTypeCode();
            if (tableTypeCode == -1)
            {
                PLogger.Error("DataSystem.Initialize: Invalid constructor for Tables");
                return;
            }

            switch (tableTypeCode)
            {
                case 0:
                    Tables = new cfg.Tables();
                    break;
                case 1:
                    await LoadAllBin(tableNames);
                    Tables = new cfg.Tables(LoadTableByBytes);
                    break;
                case 2:
                    // TODO: LoadTableByJson
                    break;
            }

            ReleaseAllAssets();
            initialized = true;
        }

        private static string[] GetTableNames()
        {
            var type = typeof(cfg.Tables);
            var fieldInfo = type.GetField("AllTableNames", BindingFlags.Public | BindingFlags.Static);
            return fieldInfo?.GetValue(null) as string[];
        }

        private static int GetTableTypeCode()
        {
            var constructorInfos = typeof(cfg.Tables).GetConstructors();
            //Tables should have one default constructor and one constructor with one parameter generated by luban
            if (constructorInfos.Length > 2)
            {
                return -1;
            }

            foreach (var info in constructorInfos)
            {
                var parameters = info.GetParameters();
                if (parameters.Length == 1)
                {
                    var loaderReturnType = parameters[0].ParameterType.GetGenericArguments()[1];
                    return loaderReturnType == typeof(Luban.ByteBuf) ? 1 : 2;
                }
            }

            return 0;
        }

        private static async UniTask LoadAllBin(string[] tableNames)
        {
            foreach (var tableName in tableNames)
            {
                var assetLocation =
                    $"{SettingsUtil.GlobalSettings.dataTableSettings.tableAssetsDir}/{tableName}{SettingsUtil.GlobalSettings.dataTableSettings.tableAssetExtension}";
                var bytes = await AssetModule.LoadRawDataAsync(assetLocation);
                if (bytes == null)
                {
                    PLogger.Error($"DataSystem.LoadAllBin: {tableName} load failed");
                    continue;
                }

                if (!_dataTableCache.TryAdd(tableName, bytes))
                {
                    PLogger.Error($"DataSystem.LoadAllBin: {tableName} already loaded");
                }
            }
        }

        private static Luban.ByteBuf LoadTableByBytes(string tableName)
        {
            if (_dataTableCache.TryGetValue(tableName, out var bytes))
            {
                return Luban.ByteBuf.Wrap(bytes);
            }

            PLogger.Error($"DataSystem.LoadTableByBytes: {tableName} not loaded");
            return null;
        }

        public static void ReleaseAllAssets()
        {
            foreach (var bytes in _dataTableCache.Values)
            {
                AssetModule.ReleaseAsset(bytes);
            }

            _dataTableCache.Clear();
        }
    }
}