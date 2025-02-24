using UnityEngine;

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

        public override void ApplyLocalization()
        {
            if (_text == null)
            {
                _text = GetComponent<TMPro.TMP_Text>();
            }

            if (_text == null)
            {
                PLogger.Warning("LocalizedText: Text component not found");
                return;
            }

            ApplyFont();
            _text.text = LocalizationModule.GetLocalizeValue(localizationKey);
        }

        private void ApplyFont()
        {
            _text.font = LocalizationModule.CurrentFont;
            if (LocalizationModule.CurrentFontMaterial != null)
            {
                _text.fontMaterial = LocalizationModule.CurrentFontMaterial;
            }

            _text.fontSize *= LocalizationModule.CurrentFontSize;
        }
    }
}