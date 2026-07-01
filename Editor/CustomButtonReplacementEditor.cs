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
        private const string MENU_ROOT = "Tools/Deucarian/XR UI/Custom Buttons/";
        private const string SETTINGS_FOLDER = "Assets/Deucarian/XR UI/Resources";
        private const string SETTINGS_PATH = SETTINGS_FOLDER + "/CustomButtonSettings.asset";
        private const string PALETTE_PATH = SETTINGS_FOLDER + "/XrUiColorPalette.asset";

        [MenuItem(MENU_ROOT + "Create or Select Global Settings", false, 1)]
        public static CustomButtonSettings CreateOrSelectGlobalSettings()
        {
            return CreateOrSelectAsset<CustomButtonSettings>(SETTINGS_PATH);
        }

        [MenuItem(MENU_ROOT + "Create or Select Global Color Palette", false, 2)]
        public static XrUiColorPalette CreateOrSelectGlobalColorPalette()
        {
            return CreateOrSelectAsset<XrUiColorPalette>(PALETTE_PATH);
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
