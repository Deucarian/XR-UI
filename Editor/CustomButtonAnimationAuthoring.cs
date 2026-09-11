using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Deucarian.XRUI.Controls.Editor
{
    internal static class CustomButtonAnimationAuthoring
    {
        internal static void Create(CustomButton button)
        {
            if (button == null) return;
            string path = EditorUtility.SaveFilePanelInProject("Button animation controller", button.name, "controller", "Choose a project asset location.");
            if (string.IsNullOrEmpty(path)) return;
            if (AssetDatabase.LoadMainAssetAtPath(path) != null)
            {
                EditorUtility.DisplayDialog("Choose a new asset", "An asset already exists at this location. Choose a new filename to preserve it.", "OK");
                return;
            }
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            var triggers = button.animationTriggers;
            string[] names = { triggers.normalTrigger, triggers.highlightedTrigger, triggers.pressedTrigger, triggers.selectedTrigger, triggers.disabledTrigger };
            string[] defaults = { "Normal", "Highlighted", "Pressed", "Selected", "Disabled" };
            var machine = controller.layers[0].stateMachine;
            for (int i = 0; i < names.Length; i++)
            {
                string name = string.IsNullOrEmpty(names[i]) ? defaults[i] : names[i];
                var clip = new AnimationClip { name = name }; AssetDatabase.AddObjectToAsset(clip, controller);
                var state = controller.AddMotion(clip);
                controller.AddParameter(name, AnimatorControllerParameterType.Trigger);
                machine.AddAnyStateTransition(state).AddCondition(AnimatorConditionMode.If, 0, name);
            }
            var animator = button.GetComponent<Animator>();
            if (animator == null) animator = Undo.AddComponent<Animator>(button.gameObject);
            Undo.RecordObject(animator, "Assign Button Animation Controller");
            AnimatorController.SetAnimatorController(animator, controller);
            EditorUtility.SetDirty(animator); AssetDatabase.SaveAssets();
        }
    }
}
