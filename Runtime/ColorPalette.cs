using Deucarian.XRUI.Controls;
using UnityEngine;

namespace Deucarian.XRUI
{
    /// <summary>
    ///     Convenience accessors for the global Deucarian XR UI color palette asset.
    /// </summary>
    public static class ColorPalette
    {
        #region Public Properties
        public static XrUiColorPalette Palette => XrUiColorPalette.Global;

        public static Color SuccessColor
        {
            get => Palette.Success;
            set => Palette.Success = value;
        }

        public static Color DangerColor
        {
            get => Palette.Danger;
            set => Palette.Danger = value;
        }

        public static Color Green
        {
            get => Palette.Success;
            set => Palette.Success = value;
        }

        public static Color Red
        {
            get => Palette.Danger;
            set => Palette.Danger = value;
        }

        public static Color WarningColor
        {
            get => Palette.Warning;
            set => Palette.Warning = value;
        }

        public static Color InfoColor
        {
            get => Palette.Info;
            set => Palette.Info = value;
        }

        public static Color PrimaryColor
        {
            get => Palette.Primary;
            set => Palette.Primary = value;
        }

        public static Color SecondaryColor
        {
            get => Palette.Secondary;
            set => Palette.Secondary = value;
        }

        public static Color BackgroundColor
        {
            get => Palette.Background;
            set => Palette.Background = value;
        }

        public static Color NormalColor
        {
            get => Palette.GetInteractionColor(CustomButtonVisualState.Normal);
            set => Palette.Normal = value;
        }

        public static Color HighlightedColor
        {
            get => Palette.GetInteractionColor(CustomButtonVisualState.Highlighted);
            set => Palette.Highlighted = value;
        }

        public static Color PressedColor
        {
            get => Palette.GetInteractionColor(CustomButtonVisualState.Pressed);
            set => Palette.Pressed = value;
        }

        public static Color SelectedColor
        {
            get => Palette.GetInteractionColor(CustomButtonVisualState.Selected);
            set => Palette.Selected = value;
        }

        public static Color DisabledColor
        {
            get => Palette.Disabled;
            set => Palette.Disabled = value;
        }

        public static Color SocketGhostColor
        {
            get => Palette.SocketGhost;
            set => Palette.SocketGhost = value;
        }

        public static Color TitleTextColor
        {
            get => Palette.TitleText;
            set => Palette.TitleText = value;
        }

        public static Color BodyTextColor
        {
            get => Palette.BodyText;
            set => Palette.BodyText = value;
        }

        public static Color SmallTextColor
        {
            get => Palette.SmallText;
            set => Palette.SmallText = value;
        }

        public static Color MutedTextColor
        {
            get => Palette.MutedText;
            set => Palette.MutedText = value;
        }

        public static Color InputTextColor
        {
            get => Palette.InputText;
            set => Palette.InputText = value;
        }

        public static Color PlaceholderTextColor
        {
            get => Palette.PlaceholderText;
            set => Palette.PlaceholderText = value;
        }

        public static Color IconColor
        {
            get => Palette.Icon;
            set => Palette.Icon = value;
        }

        public static Color ImageColor
        {
            get => Palette.Image;
            set => Palette.Image = value;
        }

        public static Color ImageMutedColor
        {
            get => Palette.ImageMuted;
            set => Palette.ImageMuted = value;
        }

        public static Color ImageSubtleColor
        {
            get => Palette.ImageSubtle;
            set => Palette.ImageSubtle = value;
        }

        public static Color SliderTrackColor
        {
            get => Palette.SliderTrack;
            set => Palette.SliderTrack = value;
        }

        public static Color OutlineColor
        {
            get => Palette.Outline;
            set => Palette.Outline = value;
        }

        public static Color TransparentColor
        {
            get => Palette.Transparent;
            set => Palette.Transparent = value;
        }

        #endregion
    }
}
