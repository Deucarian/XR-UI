using UnityEngine;
using UnityEngine.UI;
using Deucarian.XRUI.Controls;

namespace Deucarian.XRUI
{
    public static class UiButtonTint
    {
        #region Public Methods
        public static Color Tint(Color c, float factor) => Deucarian.Theming.DeucarianControlColorMath.Tint(c, factor);

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
            XrUiColorPalette palette = XrUiPaletteScope.Resolve(target);
            float resolvedSelectedBoost = palette.UseInteractionStateMultipliers ? selectedBoost : 1f;

            cb.normalColor = baseColor;
            cb.highlightedColor = palette.GetInteractionColor(CustomButtonVisualState.Highlighted);
            cb.pressedColor = palette.GetInteractionColor(CustomButtonVisualState.Pressed);
            cb.selectedColor = Tint(palette.GetInteractionColor(CustomButtonVisualState.Selected), resolvedSelectedBoost);
            cb.disabledColor = palette.Disabled;

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

            XrUiColorPalette palette = XrUiPaletteScope.Resolve(target);
            ColorBlock cb = target.colors;
            cb.colorMultiplier = 1f;
            float resolvedSelectedBoost = palette.UseInteractionStateMultipliers ? selectedBoost : 1f;

            cb.normalColor = Tint(baseColor, palette.GetInteractionMultiplier(CustomButtonVisualState.Normal));
            cb.highlightedColor = Tint(baseColor, palette.GetInteractionMultiplier(CustomButtonVisualState.Highlighted));
            cb.pressedColor = Tint(baseColor, palette.GetInteractionMultiplier(CustomButtonVisualState.Pressed));
            cb.selectedColor = Tint(Tint(baseColor, palette.GetInteractionMultiplier(CustomButtonVisualState.Selected)),
                                    resolvedSelectedBoost);
            cb.disabledColor = XrUiPaletteScope.Resolve(target).Disabled;

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
