using System;

namespace Deucarian.XRUI
{
    public static class XrUiRuntimeSettings
    {
        private static bool _keyboardRaycastProtectorEnabled = true;
        private static IXrUiSettingsProvider _subscribedProvider;

        public static event Action<bool> KeyboardRaycastProtectorEnabledChanged;

        public static bool KeyboardRaycastProtectorEnabled
        {
            get => XrUiRuntimeServices.SettingsProvider?.KeyboardRaycastProtectorEnabled
                   ?? _keyboardRaycastProtectorEnabled;
            set
            {
                if (_keyboardRaycastProtectorEnabled == value)
                {
                    return;
                }

                _keyboardRaycastProtectorEnabled = value;
                KeyboardRaycastProtectorEnabledChanged?.Invoke(value);
            }
        }

        static XrUiRuntimeSettings()
        {
            XrUiRuntimeServices.SettingsProviderChanged += OnSettingsProviderChanged;
        }

        private static void OnSettingsProviderChanged()
        {
            if (_subscribedProvider != null)
            {
                _subscribedProvider.KeyboardRaycastProtectorEnabledChanged -= OnProviderKeyboardRaycastProtectorEnabledChanged;
            }

            _subscribedProvider = XrUiRuntimeServices.SettingsProvider;
            if (_subscribedProvider != null)
            {
                _subscribedProvider.KeyboardRaycastProtectorEnabledChanged += OnProviderKeyboardRaycastProtectorEnabledChanged;
            }

            KeyboardRaycastProtectorEnabledChanged?.Invoke(KeyboardRaycastProtectorEnabled);
        }

        private static void OnProviderKeyboardRaycastProtectorEnabledChanged(bool enabled)
        {
            KeyboardRaycastProtectorEnabledChanged?.Invoke(enabled);
        }
    }
}
