using Deucarian.Editor;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;

namespace Deucarian.XRUI.Controls.Editor
{
    [CustomEditor(typeof(CustomButton), true)]
    [CanEditMultipleObjects]
    public sealed class CustomButtonInspector : ButtonEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = DeucarianEditorInspector.CreateToolkit("XR button");
            DeucarianEditorInspector.Property(root, serializedObject, "m_Interactable", "Interactable");
            DeucarianEditorInspector.Property(root, serializedObject, "m_Transition", "Transition");
            var graphic = DeucarianEditorInspector.Property(root, serializedObject, "m_TargetGraphic", "Target graphic");
            var colors = DeucarianEditorInspector.Property(root, serializedObject, "m_Colors", "Colors");
            var sprites = DeucarianEditorInspector.Property(root, serializedObject, "m_SpriteState", "Sprites");
            var animation = DeucarianEditorInspector.Property(root, serializedObject, "m_AnimationTriggers", "Animation triggers");
            var generate = DeucarianEditorWorkspaceControls.Button("Generate animation controller", () =>
                CustomButtonAnimationAuthoring.Create((CustomButton)target)); root.Add(generate);
            var warning = new HelpBox(string.Empty, HelpBoxMessageType.Warning); root.Add(warning);
            DeucarianEditorInspector.Property(root, serializedObject, "m_Navigation", "Navigation");
            new DeucarianEditorWorkspaceForm(root).Toggle("visualize-navigation", "Visualize navigation",
                () => EditorPrefs.GetBool("SelectableEditor.ShowNavigation"), value =>
                {
                    EditorPrefs.SetBool("SelectableEditor.ShowNavigation", value);
                    base.OnDisable(); base.OnEnable(); SceneView.RepaintAll();
                });
            DeucarianEditorInspector.Property(root, serializedObject, "m_OnClick", "On click");
            DeucarianEditorInspector.Property(root, serializedObject, "_onButtonClick", "On press");
            DeucarianEditorInspector.Observe(root, serializedObject, () =>
            {
                if (!(target is CustomButton button)) return;
                var transition = serializedObject.FindProperty("m_Transition");
                bool mixed = transition.hasMultipleDifferentValues;
                var mode = (Selectable.Transition)transition.enumValueIndex;
                bool tinted = mode == Selectable.Transition.ColorTint;
                bool swapped = mode == Selectable.Transition.SpriteSwap;
                bool animated = mode == Selectable.Transition.Animation;
                DeucarianEditorWorkspaceControls.Show(graphic, mixed || tinted || swapped);
                DeucarianEditorWorkspaceControls.Show(colors, mixed || tinted);
                DeucarianEditorWorkspaceControls.Show(sprites, mixed || swapped);
                DeucarianEditorWorkspaceControls.Show(animation, mixed || animated);
                var animator = button.GetComponent<Animator>();
                DeucarianEditorWorkspaceControls.Show(generate, !mixed && animated && (animator == null || animator.runtimeAnimatorController == null));
                generate.SetEnabled(targets.Length == 1);
                var resolved = button.targetGraphic != null ? button.targetGraphic : button.GetComponent<Graphic>();
                warning.text = tinted && resolved == null ? "Assign a Graphic for color transitions." :
                    swapped && !(resolved is UnityEngine.UI.Image) ? "Assign an Image for sprite transitions." : string.Empty;
                DeucarianEditorWorkspaceControls.Show(warning, !mixed && !string.IsNullOrEmpty(warning.text));
            });
            return root;
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
