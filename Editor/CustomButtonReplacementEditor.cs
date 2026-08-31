#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Deucarian.XRUI.Controls.Editor
{
    [CustomEditor(typeof(CustomButton), true)]
    [CanEditMultipleObjects]
    public sealed class CustomButtonInspector : ButtonEditor
    {
        private SerializedProperty _onButtonClick;

        protected override void OnEnable()
        {
            base.OnEnable();
            _onButtonClick = serializedObject.FindProperty("_onButtonClick");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Custom Button", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_onButtonClick);
            serializedObject.ApplyModifiedProperties();
        }
    }

    public static class CustomButtonReplacementEditor
    {
        private const string SETTINGS_FOLDER = "Assets/Deucarian/XR UI/Resources";
        internal const string SettingsPath = SETTINGS_FOLDER + "/CustomButtonSettings.asset";
        internal const string PalettePath = SETTINGS_FOLDER + "/XrUiColorPalette.asset";

        public static CustomButtonSettings CreateOrSelectGlobalSettings()
        {
            return CreateOrSelectAsset<CustomButtonSettings>(SettingsPath);
        }

        public static XrUiColorPalette CreateOrSelectGlobalColorPalette()
        {
            return CreateOrSelectAsset<XrUiColorPalette>(PalettePath);
        }

        private static T CreateOrSelectAsset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                Directory.CreateDirectory(SETTINGS_FOLDER);
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            UnityEditor.Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            return asset;
        }
    }
}
#endif
