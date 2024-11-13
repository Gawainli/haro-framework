﻿#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.UI;

namespace ProxFramework.UI.Editor
{
    [CustomEditor(typeof(ButtonEx), true)]
    public class ButtonExEditor : ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("normalStateObjects"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("highlightedStateObjects"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pressedStateObjects"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("disabledStateObjects"), true);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif