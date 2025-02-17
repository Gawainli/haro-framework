﻿using UnityEngine;

namespace ProxFramework.Localization
{
    [RequireComponent(typeof(TMPro.TMP_Text))]
    public class LocalizedText : LocalizedBehaviour
    {
        private TMPro.TMP_Text _text;

        protected override void Awake()
        {
            base.Awake();
            _text = GetComponent<TMPro.TMP_Text>();
        }

        protected override void ApplyLocalization()
        {
            if (_text == null)
            {
                PLogger.Warning("LocalizedText: Text component is null");
                return;
            }

            _text.text = LocalizationModule.GetLocalizeValue(_text.text);
        }

        protected void ApplyFont()
        {
            _text.font = LocalizationModule.CurrentFont;
            _text.fontMaterial = LocalizationModule.CurrentFontMaterial;
            _text.fontSize *= LocalizationModule.CurrentFontSize;
        }
    }
}