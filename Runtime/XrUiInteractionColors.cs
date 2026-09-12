using Deucarian.XRUI.Controls;
using UnityEngine;

namespace Deucarian.XRUI
{
    internal readonly struct XrUiInteractionColors
    {
        private readonly Color normal, highlighted, pressed, selected, disabled;

        internal XrUiInteractionColors(Color normal, Color highlighted, Color pressed, Color selected, Color disabled)
        {
            this.normal = normal; this.highlighted = highlighted; this.pressed = pressed;
            this.selected = selected; this.disabled = disabled;
        }

        internal Color Resolve(XrUiColorPalette palette, CustomButtonVisualState state, XrUiSemanticColor semantic) => state switch
        {
            CustomButtonVisualState.Highlighted => highlighted,
            CustomButtonVisualState.Pressed => pressed,
            CustomButtonVisualState.Selected => selected,
            CustomButtonVisualState.Disabled => disabled,
            _ => semantic == XrUiSemanticColor.Background ? normal : palette.GetSemanticColor(semantic),
        };

        internal static Color ResolveLegacy(XrUiColorPalette palette, CustomButtonVisualState state, XrUiSemanticColor semantic)
        {
            if (state == CustomButtonVisualState.Disabled) return palette.Disabled;
            var role = state switch
            {
                CustomButtonVisualState.Highlighted => XrUiSemanticColor.Secondary,
                CustomButtonVisualState.Pressed => XrUiSemanticColor.Primary,
                CustomButtonVisualState.Selected => XrUiSemanticColor.Primary,
                _ => semantic,
            };
            Color color = palette.GetSemanticColor(role);
            float multiplier = palette.GetInteractionMultiplier(state);
            return Deucarian.Theming.DeucarianControlColorMath.Tint(color, multiplier);
        }
    }
}
