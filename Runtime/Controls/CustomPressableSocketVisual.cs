using UnityEngine;
using UnityEngine.UI;

namespace Deucarian.XRUI.Controls
{
    public static class CustomPressableSocketVisual
    {
        public static Color ResolveColor(CustomButtonSettings settings)
        {
            return (settings != null ? settings : CustomButtonSettings.Global).ResolveSocketGhostColor();
        }

        public static Color ResolveColor(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            return ResolveColor(settings);
        }

        public static Color ResolveImageColor(CustomButtonSettings settings)
        {
            return ResolveImageColor(settings, CustomPressableVisualStyle.GenericButton);
        }

        public static Color ResolveImageColor(CustomButtonSettings settings, CustomPressableVisualStyle style)
            => ResolveImageColor(settings, style, XrUiColorPalette.Global);

        public static Color ResolveImageColor(CustomButtonSettings settings, CustomPressableVisualStyle style, XrUiColorPalette palette)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            Color color = resolvedSettings.ResolveSocketGhostColor(palette);
            if (ResolveSocketSprite(resolvedSettings, style) == null)
            {
                color.a = 0f;
            }

            return color;
        }

        public static Sprite ResolveSprite(CustomButtonSettings settings)
        {
            return ResolveSocketSprite(settings, CustomPressableVisualStyle.GenericButton);
        }

        public static Sprite ResolveSocketSprite(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            return resolvedSettings.ResolveVisualStyle(style).SocketGhostSprite;
        }

        public static Sprite ResolvePressableBackgroundSprite(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            return resolvedSettings.ResolveVisualStyle(style).PressableBackgroundSprite;
        }

        public static Sprite ResolveOutlineSprite(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            return resolvedSettings.ResolveVisualStyle(style).OutlineSprite;
        }

        public static Image.Type ResolveImageType(CustomButtonSettings settings)
        {
            return ResolveSocketImageType(settings, CustomPressableVisualStyle.GenericButton);
        }

        public static Image.Type ResolveSocketImageType(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            if (ResolveSocketFillCenter(settings, style))
            {
                return Image.Type.Sliced;
            }

            return ResolveImageType(ResolveSocketSprite(settings, style));
        }

        public static Image.Type ResolvePressableBackgroundImageType(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            if (ResolvePressableBackgroundFillCenter(settings, style))
            {
                return Image.Type.Sliced;
            }

            return ResolveImageType(ResolvePressableBackgroundSprite(settings, style));
        }

        public static Image.Type ResolveOutlineImageType(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            if (ResolveOutlineFillCenter(settings, style))
            {
                return Image.Type.Sliced;
            }

            return ResolveImageType(ResolveOutlineSprite(settings, style));
        }

        public static bool ResolvePreserveAspect(CustomPressableVisualStyle style)
        {
            return false;
        }

        public static bool ResolveSocketFillCenter(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            return resolvedSettings.ResolveVisualStyle(style).SocketGhostFillCenter;
        }

        public static bool ResolvePressableBackgroundFillCenter(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            return resolvedSettings.ResolveVisualStyle(style).PressableBackgroundFillCenter;
        }

        public static bool ResolveOutlineFillCenter(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            return resolvedSettings.ResolveVisualStyle(style).OutlineFillCenter;
        }

        private static Image.Type ResolveImageType(Sprite sprite)
        {
            return sprite != null && sprite.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
        }

        public static float ResolvePixelsPerUnitMultiplier(CustomButtonSettings settings)
        {
            return ResolveSocketPixelsPerUnitMultiplier(settings, CustomPressableVisualStyle.GenericButton);
        }

        public static float ResolveSocketPixelsPerUnitMultiplier(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            CustomPressableVisualStyleProfile profile = resolvedSettings.ResolveVisualStyle(style);
            return profile.SocketGhostSprite != null ? profile.SocketGhostPixelsPerUnitMultiplier : 1f;
        }

        public static float ResolvePressableBackgroundPixelsPerUnitMultiplier(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            CustomPressableVisualStyleProfile profile = resolvedSettings.ResolveVisualStyle(style);
            return profile.PressableBackgroundSprite != null ? profile.PressableBackgroundPixelsPerUnitMultiplier : 1f;
        }

        public static float ResolveOutlinePixelsPerUnitMultiplier(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            CustomPressableVisualStyleProfile profile = resolvedSettings.ResolveVisualStyle(style);
            return profile.OutlineSprite != null ? profile.OutlinePixelsPerUnitMultiplier : 1f;
        }

        public static bool Configure(Image image, CustomButtonSettings settings)
        {
            return Configure(image, settings, CustomPressableVisualStyle.GenericButton);
        }

        public static bool Configure(Image image, CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            if (image == null)
            {
                return false;
            }

            bool changed = false;
            Sprite targetSprite = ResolveSocketSprite(settings, style);
            if (image.sprite != targetSprite)
            {
                image.sprite = targetSprite;
                changed = true;
            }

            if (CustomPressableImageOverride.Clear(image))
            {
                changed = true;
            }

            Image.Type targetType = ResolveSocketImageType(settings, style);
            if (image.type != targetType)
            {
                image.type = targetType;
                changed = true;
            }

            bool targetPreserveAspect = ResolvePreserveAspect(style);
            if (image.preserveAspect != targetPreserveAspect)
            {
                image.preserveAspect = targetPreserveAspect;
                changed = true;
            }

            bool targetFillCenter = ResolveSocketFillCenter(settings, style);
            if (image.fillCenter != targetFillCenter)
            {
                image.fillCenter = targetFillCenter;
                changed = true;
            }

            if (image.fillMethod != Image.FillMethod.Horizontal)
            {
                image.fillMethod = Image.FillMethod.Horizontal;
                changed = true;
            }

            if (!Mathf.Approximately(image.fillAmount, 1f))
            {
                image.fillAmount = 1f;
                changed = true;
            }

            if (!image.fillClockwise)
            {
                image.fillClockwise = true;
                changed = true;
            }

            if (image.fillOrigin != 0)
            {
                image.fillOrigin = 0;
                changed = true;
            }

            if (image.useSpriteMesh)
            {
                image.useSpriteMesh = false;
                changed = true;
            }

            float targetPixelsPerUnitMultiplier = ResolveSocketPixelsPerUnitMultiplier(settings, style);
            if (!Mathf.Approximately(image.pixelsPerUnitMultiplier, targetPixelsPerUnitMultiplier))
            {
                image.pixelsPerUnitMultiplier = targetPixelsPerUnitMultiplier;
                changed = true;
            }

            Color targetColor = ResolveImageColor(settings, style, XrUiPaletteScope.Resolve(image));
            if (!Approximately(image.color, targetColor))
            {
                image.color = targetColor;
                changed = true;
            }

            if (image.raycastTarget)
            {
                image.raycastTarget = false;
                changed = true;
            }

            bool targetMaskable = ResolveManagedLayerMaskable(style);
            if (image.maskable != targetMaskable)
            {
                image.maskable = targetMaskable;
                changed = true;
            }

            return changed;
        }

        public static bool ConfigurePressableBackground(Image image,
                                                        CustomButtonSettings settings,
                                                        CustomPressableVisualStyle style,
                                                        Image sourceImage)
        {
            if (image == null)
            {
                return false;
            }

            bool changed = false;
            Sprite styleSprite = ResolvePressableBackgroundSprite(settings, style);
            Sprite targetSprite = styleSprite != null ? styleSprite : sourceImage != null ? sourceImage.sprite : null;
            if (image.sprite != targetSprite)
            {
                image.sprite = targetSprite;
                changed = true;
            }

            if (CustomPressableImageOverride.Clear(image))
            {
                changed = true;
            }

            Image.Type targetType = styleSprite != null || ResolvePressableBackgroundFillCenter(settings, style)
                                            ? ResolvePressableBackgroundImageType(settings, style)
                                            : sourceImage != null ? sourceImage.type : ResolveImageType(targetSprite);
            if (image.type != targetType)
            {
                image.type = targetType;
                changed = true;
            }

            bool targetPreserveAspect = ResolvePreserveAspect(style);
            if (image.preserveAspect != targetPreserveAspect)
            {
                image.preserveAspect = targetPreserveAspect;
                changed = true;
            }

            bool targetFillCenter = ResolvePressableBackgroundFillCenter(settings, style);
            if (image.fillCenter != targetFillCenter)
            {
                image.fillCenter = targetFillCenter;
                changed = true;
            }

            Image.FillMethod targetFillMethod = sourceImage != null ? sourceImage.fillMethod : Image.FillMethod.Horizontal;
            if (image.fillMethod != targetFillMethod)
            {
                image.fillMethod = targetFillMethod;
                changed = true;
            }

            float targetFillAmount = sourceImage != null ? sourceImage.fillAmount : 1f;
            if (!Mathf.Approximately(image.fillAmount, targetFillAmount))
            {
                image.fillAmount = targetFillAmount;
                changed = true;
            }

            bool targetFillClockwise = sourceImage == null || sourceImage.fillClockwise;
            if (image.fillClockwise != targetFillClockwise)
            {
                image.fillClockwise = targetFillClockwise;
                changed = true;
            }

            int targetFillOrigin = sourceImage != null ? sourceImage.fillOrigin : 0;
            if (image.fillOrigin != targetFillOrigin)
            {
                image.fillOrigin = targetFillOrigin;
                changed = true;
            }

            if (image.useSpriteMesh)
            {
                image.useSpriteMesh = false;
                changed = true;
            }

            float targetPixelsPerUnitMultiplier = styleSprite != null
                                                          ? ResolvePressableBackgroundPixelsPerUnitMultiplier(settings, style)
                                                          : sourceImage != null ? sourceImage.pixelsPerUnitMultiplier : 1f;
            if (!Mathf.Approximately(image.pixelsPerUnitMultiplier, targetPixelsPerUnitMultiplier))
            {
                image.pixelsPerUnitMultiplier = targetPixelsPerUnitMultiplier;
                changed = true;
            }

            Color targetColor = targetSprite != null ? Color.white : XrUiPaletteScope.Resolve(image).Transparent;
            if (!Approximately(image.color, targetColor))
            {
                image.color = targetColor;
                changed = true;
            }

            if (image.raycastTarget)
            {
                image.raycastTarget = false;
                changed = true;
            }

            return changed;
        }

        public static bool ConfigureOutline(Image image, CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            if (image == null)
            {
                return false;
            }

            bool changed = false;
            Sprite targetSprite = ResolveOutlineSprite(settings, style);
            bool shouldBeActive = targetSprite != null;
            if (image.gameObject.activeSelf != shouldBeActive)
            {
                image.gameObject.SetActive(shouldBeActive);
                changed = true;
            }

            if (image.sprite != targetSprite)
            {
                image.sprite = targetSprite;
                changed = true;
            }

            if (CustomPressableImageOverride.Clear(image))
            {
                changed = true;
            }

            Image.Type targetType = ResolveOutlineImageType(settings, style);
            if (image.type != targetType)
            {
                image.type = targetType;
                changed = true;
            }

            bool targetPreserveAspect = ResolvePreserveAspect(style);
            if (image.preserveAspect != targetPreserveAspect)
            {
                image.preserveAspect = targetPreserveAspect;
                changed = true;
            }

            bool targetFillCenter = ResolveOutlineFillCenter(settings, style);
            if (image.fillCenter != targetFillCenter)
            {
                image.fillCenter = targetFillCenter;
                changed = true;
            }

            if (image.fillMethod != Image.FillMethod.Horizontal)
            {
                image.fillMethod = Image.FillMethod.Horizontal;
                changed = true;
            }

            if (!Mathf.Approximately(image.fillAmount, 1f))
            {
                image.fillAmount = 1f;
                changed = true;
            }

            if (!image.fillClockwise)
            {
                image.fillClockwise = true;
                changed = true;
            }

            if (image.fillOrigin != 0)
            {
                image.fillOrigin = 0;
                changed = true;
            }

            if (image.useSpriteMesh)
            {
                image.useSpriteMesh = false;
                changed = true;
            }

            float targetPixelsPerUnitMultiplier = ResolveOutlinePixelsPerUnitMultiplier(settings, style);
            if (!Mathf.Approximately(image.pixelsPerUnitMultiplier, targetPixelsPerUnitMultiplier))
            {
                image.pixelsPerUnitMultiplier = targetPixelsPerUnitMultiplier;
                changed = true;
            }

            XrUiColorPalette palette = XrUiPaletteScope.Resolve(image);
            Color targetColor = shouldBeActive ? palette.Outline : palette.Transparent;
            if (!Approximately(image.color, targetColor))
            {
                image.color = targetColor;
                changed = true;
            }

            if (image.raycastTarget)
            {
                image.raycastTarget = false;
                changed = true;
            }

            bool targetMaskable = ResolveManagedLayerMaskable(style);
            if (image.maskable != targetMaskable)
            {
                image.maskable = targetMaskable;
                changed = true;
            }

            return changed;
        }

        private static bool ResolveManagedLayerMaskable(CustomPressableVisualStyle style)
        {
            return style != CustomPressableVisualStyle.ScrollbarSlider;
        }

        private static bool Approximately(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.001f &&
                   Mathf.Abs(a.g - b.g) < 0.001f &&
                   Mathf.Abs(a.b - b.b) < 0.001f &&
                   Mathf.Abs(a.a - b.a) < 0.001f;
        }
    }
}
