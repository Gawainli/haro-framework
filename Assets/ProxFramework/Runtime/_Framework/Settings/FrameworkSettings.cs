﻿using UnityEngine;

namespace ProxFramework.Runtime.Settings
{
    [CreateAssetMenu(fileName = "FrameworkSettings", menuName = "Prox/Framework Settings")]
    public class FrameworkSettings : ScriptableObject
    {
        public int targetFrameRate = 60;
        public AssetSettings assetSettings;
        public HCLRSettings hclrSettings;
    }
}