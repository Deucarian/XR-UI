using UnityEngine;
using UnityEngine.UI;
using Deucarian.XRUI.Controls;

namespace Deucarian.XRUI
{
    public static class UiButtonTint
    {
        #region Public Methods
        public static Color Tint(Color c, float factor) => new Color(Mathf.Clamp01(c.r * factor),
                                                                     Mathf.Clamp01(c.g * factor),
                                                                     Mathf.Clamp01(c.b * factor),
                                                                     c.a);

        public static void ApplyPalette(Selectable target, Color baseColor, float selectedBoost = 1f)
        {
            if (!target)
            {
                return;
            }

            if (target.TryGetComponent(out CustomSelectableFeedback feedback))
            {
                target.transition = Selectable.Transition.None;
                feedback.SetBaseInteractionColor(baseColor, selectedBoost);
                return;
            }

            if (target.GetComponent<ButtonFocusListener>() != null)
            {
                target.transition = Selectable.Transition.None;
            }

            if (target.targetGraphic != null)
            {
                target.targetGraphic.color = Color.white;
            }

            ColorBlock cb = target.colors;
            cb.colorMultiplier = 1f;
            float resolvedSelectedBoost = ColorPalette.Palette.UseInteractionStateMultipliers ? selectedBoost : 1f;

            cb.normalColor = baseColor;
            cb.highlightedColor = ColorPalette.HighlightedColor;
            cb.pressedColor = ColorPalette.PressedColor;
            cb.selectedColor = Tint(ColorPalette.SelectedColor, resolvedSelectedBoost);
            cb.disabledColor = ColorPalette.DisabledColor;

            target.colors = cb;
        }

        public static void ApplyPaletteWithInteractionMultipliers(Selectable target,
                                                                  Color baseColor,
                                                                  float selectedBoost = 1f)
        {
            if (!target)
            {
                return;
            }

            if (target.GetComponent<CustomSelectableFeedback>() != null ||
                target.GetComponent<ButtonFocusListener>() != null)
            {
                target.transition = Selectable.Transition.None;
            }

            if (target.TryGetComponent(out CustomSelectableFeedback feedback))
            {
                feedback.SetBaseInteractionColor(baseColor, selectedBoost, true);
                return;
            }

            if (target.targetGraphic != null)
            {
                target.targetGraphic.color = Color.white;
            }

            XrUiColorPalette palette = ColorPalette.Palette;
            ColorBlock cb = target.colors;
            cb.colorMultiplier = 1f;
            float resolvedSelectedBoost = palette.UseInteractionStateMultipliers ? selectedBoost : 1f;

            cb.normalColor = Tint(baseColor, palette.GetInteractionMultiplier(CustomButtonVisualState.Normal));
            cb.highlightedColor = Tint(baseColor, palette.GetInteractionMultiplier(CustomButtonVisualState.Highlighted));
            cb.pressedColor = Tint(baseColor, palette.GetInteractionMultiplier(CustomButtonVisualState.Pressed));
            cb.selectedColor = Tint(Tint(baseColor, palette.GetInteractionMultiplier(CustomButtonVisualState.Selected)),
                                    resolvedSelectedBoost);
            cb.disabledColor = ColorPalette.DisabledColor;

            target.colors = cb;
        }

        public static void ClearPaletteOverride(Selectable target)
        {
            if (!target || !target.TryGetComponent(out CustomSelectableFeedback feedback))
            {
                return;
            }

            feedback.ClearBaseInteractionColor();
        }
        #endregion
    }
}
