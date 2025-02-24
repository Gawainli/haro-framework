#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProxFramework.Localization
{
    [CustomEditor(typeof(LocalizedBehaviour), true)]
    public class LocalizedBehaviourEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Auto Set L10N Key"))
            {
                if (target is LocalizedBehaviour localizedBehaviour)
                {
                    localizedBehaviour.AutoSetL10NKey();
                }
            }
        }
    }
}
#endif