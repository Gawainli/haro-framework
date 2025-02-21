using UnityEngine;

namespace ProxFramework.Localization
{
    public abstract class LocalizedBehaviour : MonoBehaviour
    {
        [SerializeField] private bool _enableLocalization = true;
        public abstract void ApplyLocalization();
        protected virtual void Awake()
        {
        }

        protected void OnEnable()
        {
            if (_enableLocalization && LocalizationModule.initialized)
            {
                LocalizationModule.OnLanguageChanged += ApplyLocalization;
                ApplyLocalization();
            }
        }

        protected void OnDisable()
        {
            if (_enableLocalization && LocalizationModule.initialized)
            {
                LocalizationModule.OnLanguageChanged -= ApplyLocalization;
            }
        }
    }
}