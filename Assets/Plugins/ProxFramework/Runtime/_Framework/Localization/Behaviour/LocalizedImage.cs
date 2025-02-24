using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ProxFramework.Localization
{
    public class LocalizedImage : LocalizedAsset<Sprite>
    {
        private Image _image;

        public override void AutoSetL10NKey()
        {
            var asset = GetComponent<Image>();
            if (asset == null)
            {
                PLogger.Warning($"Localized {typeof(Image)} is null. {gameObject.name}");
                localizationKey = string.Empty;
                return;
            }

            var path = AssetDatabase.GetAssetPath(asset.sprite);
            localizationKey = path;
        }

        protected override void ApplyAsset(Sprite asset)
        {
            if (_image == null)
            {
                _image = GetComponent<Image>();
            }

            _image.sprite = asset;
        }
    }
}