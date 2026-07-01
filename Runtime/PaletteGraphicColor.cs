using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deucarian.XRUI
{
    [DisallowMultipleComponent]
    public sealed class PaletteGraphicColor : MonoBehaviour
    {
        [SerializeField] private Graphic _target;
        [SerializeField] private XrUiSemanticColor _semanticColor = XrUiSemanticColor.BodyText;
        [SerializeField] private bool _preserveAlpha;

        public Graphic Target => ResolveTarget();
        public XrUiSemanticColor SemanticColor => _semanticColor;

        public void Configure(Graphic target, XrUiSemanticColor semanticColor, bool preserveAlpha)
        {
            _target = target != null ? target : ResolveTarget();
            _semanticColor = semanticColor;
            _preserveAlpha = preserveAlpha;
            ApplyPaletteColor();
        }

        public void ApplyPaletteColor() => ApplyPaletteColor(XrUiColorPalette.Global);

        private void Reset()
        {
            _target = GetComponent<Graphic>();
            PaletteGraphicColorBinder.TryInferSemanticColor(_target, out _semanticColor);
        }

        private void OnEnable()
        {
            XrUiColorPalette.PaletteChanged -= ApplyPaletteColor;
            XrUiColorPalette.PaletteChanged += ApplyPaletteColor;
            ApplyPaletteColor();
        }

        private void OnDisable()
        {
            XrUiColorPalette.PaletteChanged -= ApplyPaletteColor;
        }

        private void OnValidate()
        {
            if (_target == null)
            {
                _target = GetComponent<Graphic>();
            }
        }

        private Graphic ResolveTarget()
        {
            if (_target == null)
            {
                _target = GetComponent<Graphic>();
            }

            return _target;
        }

        private void ApplyPaletteColor(XrUiColorPalette palette)
        {
            Graphic target = ResolveTarget();
            if (target == null || palette == null)
            {
                return;
            }

            if (PaletteGraphicColorBinder.IsIgnored(target))
            {
                return;
            }

            Color color = palette.GetSemanticColor(_semanticColor);
            if (_preserveAlpha)
            {
                color.a = target.color.a;
            }

            if (!PaletteGraphicColorBinder.AreColorsApproximatelyEqual(target.color, color))
            {
                target.color = color;
            }
        }
    }

    public static class PaletteGraphicColorBinder
    {
        private const float COLOR_TOLERANCE = 0.01f;

        public static bool TryInferSemanticColor(Graphic graphic, out XrUiSemanticColor semanticColor)
        {
            semanticColor = XrUiSemanticColor.Background;
            if (graphic == null)
            {
                return false;
            }

            if (graphic.color.a <= COLOR_TOLERANCE)
            {
                semanticColor = XrUiSemanticColor.Transparent;
                return true;
            }

            if (graphic is TMP_Text text)
            {
                return TryInferTextSemanticColor(text, out semanticColor);
            }

            if (graphic is Image image)
            {
                return TryInferImageSemanticColor(image, out semanticColor);
            }

            return XrUiColorPalette.TryResolveUiColor(graphic.color, out semanticColor);
        }

        internal static bool AreColorsApproximatelyEqual(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) <= COLOR_TOLERANCE &&
                   Mathf.Abs(a.g - b.g) <= COLOR_TOLERANCE &&
                   Mathf.Abs(a.b - b.b) <= COLOR_TOLERANCE &&
                   Mathf.Abs(a.a - b.a) <= COLOR_TOLERANCE;
        }

        internal static bool IsIgnored(Graphic graphic)
        {
            return graphic != null && graphic.GetComponentInParent<PaletteGraphicColorIgnore>(true) != null;
        }

        private static bool TryInferTextSemanticColor(TMP_Text text, out XrUiSemanticColor semanticColor)
        {
            TMP_InputField inputField = text.GetComponentInParent<TMP_InputField>(true);
            if (inputField != null)
            {
                if (inputField.placeholder != null &&
                    (inputField.placeholder.transform == text.transform || text.transform.IsChildOf(inputField.placeholder.transform)))
                {
                    semanticColor = XrUiSemanticColor.PlaceholderText;
                    return true;
                }

                if (inputField.textComponent == text || ContainsName(text.transform, "text"))
                {
                    semanticColor = XrUiSemanticColor.InputText;
                    return true;
                }
            }

            if (ContainsName(text.transform, "title") ||
                ContainsName(text.transform, "header"))
            {
                semanticColor = XrUiSemanticColor.TitleText;
                return true;
            }

            if (ContainsName(text.transform, "small") ||
                ContainsName(text.transform, "caption") ||
                ContainsName(text.transform, "hint"))
            {
                semanticColor = XrUiSemanticColor.SmallText;
                return true;
            }

            if (text.color.a < 0.75f)
            {
                semanticColor = XrUiSemanticColor.MutedText;
                return true;
            }

            if (XrUiColorPalette.TryResolveUiColor(text.color, out semanticColor))
            {
                if (semanticColor == XrUiSemanticColor.Image ||
                    semanticColor == XrUiSemanticColor.Icon ||
                    semanticColor == XrUiSemanticColor.Outline)
                {
                    semanticColor = XrUiSemanticColor.BodyText;
                }

                return true;
            }

            semanticColor = XrUiSemanticColor.BodyText;
            return true;
        }

        private static bool TryInferImageSemanticColor(Image image, out XrUiSemanticColor semanticColor)
        {
            if (ContainsName(image.transform, "outline") ||
                ContainsName(image.transform, "border") ||
                ContainsName(image.transform, "line") ||
                ContainsName(image.transform, "divider"))
            {
                semanticColor = image.color.a < 0.75f ? XrUiSemanticColor.ImageSubtle : XrUiSemanticColor.Outline;
                return true;
            }

            if (ContainsName(image.transform, "icon") ||
                ContainsName(image.transform, "check") ||
                ContainsName(image.transform, "arrow"))
            {
                semanticColor = XrUiSemanticColor.Icon;
                return true;
            }

            if (XrUiColorPalette.TryResolveUiColor(image.color, out semanticColor))
            {
                if (semanticColor == XrUiSemanticColor.BodyText)
                {
                    semanticColor = image.sprite != null ? XrUiSemanticColor.Image : XrUiSemanticColor.Background;
                }

                return true;
            }

            if (image.sprite != null)
            {
                semanticColor = XrUiSemanticColor.Image;
                return true;
            }

            return false;
        }

        private static bool ContainsName(Transform transform, string value)
        {
            Transform current = transform;
            while (current != null)
            {
                if (!string.IsNullOrEmpty(current.name) &&
                    current.name.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }
    }
}
