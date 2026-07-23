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

        public static XrUiColorPalette Global
        {
            get
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
        }

        public static bool SetRuntimePalette(XrUiColorPalette palette)
        {
            if (_runtimeOverride == palette)
            {
                return false;
            }

            _runtimeOverride = palette;
            return true;
        }

        public static bool ClearRuntimePalette()
        {
            if (_runtimeOverride == null)
            {
                return false;
            }

            _runtimeOverride = null;
            return true;
        }
    }
}
