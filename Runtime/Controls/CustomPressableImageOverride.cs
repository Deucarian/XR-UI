using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Deucarian.XRUI.Controls
{
    internal static class CustomPressableImageOverride
    {
        internal static bool Clear(Image image)
        {
            if (image == null)
            {
                return false;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                SerializedObject serializedImage = new SerializedObject(image);
                SerializedProperty overrideSprite = serializedImage.FindProperty("m_OverrideSprite");
                if (overrideSprite != null)
                {
                    bool hadOverride = overrideSprite.objectReferenceValue != null;
                    if (hadOverride)
                    {
                        overrideSprite.objectReferenceValue = null;
                        serializedImage.ApplyModifiedPropertiesWithoutUndo();
                    }

                    return hadOverride;
                }
            }
#endif

            image.overrideSprite = null;
            return false;
        }
    }
}
