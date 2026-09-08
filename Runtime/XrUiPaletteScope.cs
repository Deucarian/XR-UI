using UnityEngine;

namespace Deucarian.XRUI
{
    [ExecuteAlways, DisallowMultipleComponent]
    [AddComponentMenu("Deucarian/XR UI/Palette Scope")]
    public sealed class XrUiPaletteScope : MonoBehaviour
    {
        [SerializeField, Tooltip("Palette for controls below this object. A theming bridge can register a runtime palette in this scope.")]
        private XrUiColorPalette palette;
        private XrUiPaletteContext _context;
        public XrUiPaletteContext Context => _context ?? (_context = new XrUiPaletteContext(() => palette));

        public void SetPalette(XrUiColorPalette value)
        {
            if (palette == value) return;
            palette = value;
            RefreshChildren();
        }

        public static XrUiColorPalette Resolve(Component component)
        {
            for (Transform current = component == null ? null : component.transform; current != null; current = current.parent)
            {
                if (current.TryGetComponent(out XrUiPaletteScope scope) && scope.isActiveAndEnabled)
                    return scope.Context.Current ?? XrUiColorPalette.Global;
            }
            return XrUiColorPalette.Global;
        }

        private void OnEnable()
        {
            Context.Changed += OnContextChanged;
            RefreshChildren();
        }

        private void OnDisable()
        {
            Context.Changed -= OnContextChanged;
            RefreshChildren();
        }

        private void OnValidate() => RefreshChildren();
        private void OnContextChanged(XrUiColorPalette _) => RefreshChildren();
        private void RefreshChildren()
        {
            foreach (PaletteGraphicColor graphic in GetComponentsInChildren<PaletteGraphicColor>(true))
                if (graphic.isActiveAndEnabled) graphic.ApplyPaletteColor();
            foreach (Controls.SliderToggle slider in GetComponentsInChildren<Controls.SliderToggle>(true))
                if (slider.isActiveAndEnabled) slider.ApplySliderPalette();
        }
    }
}
