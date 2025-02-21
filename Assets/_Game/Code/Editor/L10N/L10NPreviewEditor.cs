using cfg;
using GameName.Core;
using ProxFramework.Localization;
using ProxFramework.Runtime.Settings;
using UnityEditor;
using UnityEngine;

namespace ProxFramework.Editor.L10N
{
    public class L10NPreviewEditor : EditorWindow
    {
        private static int selectedLangIndex = 0;

        [MenuItem("Prox/L10N/Preview Tool")]
        public static async void ShowWindow()
        {
            GetWindow<L10NPreviewEditor>("Localization Preview");
            await DataSystem.Initialize();
            LocalizationModule.Initialize();
            LocalizationModule.SetTable(new L10NTable());
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            DrawLanguageSelector();
            DrawSceneObjectPreview();

            EditorGUILayout.EndVertical();
        }

        void DrawLanguageSelector()
        {
            EditorGUILayout.BeginHorizontal();
            // previewLang = (SystemLanguage) EditorGUILayout.EnumPopup("Editor Language", previewLang);
            var options =
                SettingsUtil.GlobalSettings.l10NSettings.supportedLanguages.ConvertAll(lang => lang.ToString());
            selectedLangIndex = EditorGUILayout.Popup("Preview Language", selectedLangIndex, options.ToArray());
            EditorGUILayout.EndHorizontal();
        }

        void DrawSceneObjectPreview()
        {
            var localizedBehaviours = FindObjectsOfType<LocalizedBehaviour>();
            foreach (var localizedBehaviour in localizedBehaviours)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(localizedBehaviour.name);
                var previewLang = SettingsUtil.GlobalSettings.l10NSettings.supportedLanguages[selectedLangIndex];
                if (GUILayout.Button("Refresh"))
                {
                    LocalizationModule.ChangeLanguage(previewLang);
                    PLogger.Info($"Previewing lang:{previewLang} {localizedBehaviour.name}");
                    localizedBehaviour.ApplyLocalization();
                }

                EditorGUILayout.EndHorizontal();
            }
        }
    }
}