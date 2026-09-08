using UnityEngine;

namespace Deucarian.XRUI
{
    /// <summary>
    /// Owns global palette selection and isolates Unity resource/fallback creation.
    /// </summary>
    internal static class XrUiColorPaletteRegistry
    {
        private const string ResourcePath = "XrUiColorPalette";

        private static XrUiColorPalette _runtimeFallback;
        private static XrUiColorPalette _runtimeOverride;
        private static XrUiColorPalette _resourcesPalette;
        private static readonly XrUiPaletteContext Context = new XrUiPaletteContext(ResolveFallback);

        static XrUiColorPaletteRegistry()
        {
            Context.Changed += _ => XrUiColorPalette.NotifyGlobalPaletteChangedIfNeeded(true);
        }

        public static System.IDisposable RegisterRuntimePalette(XrUiColorPalette palette) => Context.Register(palette);
        public static XrUiColorPalette Global => Context.Current;

        private static XrUiColorPalette ResolveFallback()
        {
                if (_runtimeOverride != null)
                {
                    return _runtimeOverride;
                }

                if (_resourcesPalette == null)
                {
                    _resourcesPalette = Resources.Load<XrUiColorPalette>(ResourcePath);
                }

                if (_resourcesPalette != null)
                {
                    return _resourcesPalette;
                }

                if (_runtimeFallback == null)
                {
                    _runtimeFallback = ScriptableObject.CreateInstance<XrUiColorPalette>();
                    _runtimeFallback.hideFlags = HideFlags.HideAndDontSave;
                }

                return _runtimeFallback;
        }

        public static bool SetRuntimePalette(XrUiColorPalette palette)
        {
            if (_runtimeOverride == palette)
            {
                return false;
            }

            var previous = Global;
            _runtimeOverride = palette;
            return previous != Global;
        }

        public static bool ClearRuntimePalette()
        {
            if (_runtimeOverride == null)
            {
                return false;
            }

            var previous = Global;
            _runtimeOverride = null;
            return previous != Global;
        }
    }
}
