using System;
using UnityEngine;

namespace Deucarian.XRUI
{
    public interface IXrUiCameraProvider
    {
        Camera EventCamera { get; }
        Transform HeadTransform { get; }
    }

    public interface IXrUiSettingsProvider
    {
        bool KeyboardRaycastProtectorEnabled { get; }
        event Action<bool> KeyboardRaycastProtectorEnabledChanged;
    }

    public interface IXrUiLifecycleProvider
    {
        event Action SceneUiInvalidated;
    }

    public static class XrUiRuntimeServices
    {
        private static IXrUiCameraProvider _cameraProvider;
        private static IXrUiSettingsProvider _settingsProvider;
        private static IXrUiLifecycleProvider _lifecycleProvider;

        public static event Action CameraProviderChanged;
        public static event Action SettingsProviderChanged;
        public static event Action LifecycleProviderChanged;

        public static IXrUiCameraProvider CameraProvider => _cameraProvider;
        public static IXrUiSettingsProvider SettingsProvider => _settingsProvider;
        public static IXrUiLifecycleProvider LifecycleProvider => _lifecycleProvider;

        public static void SetCameraProvider(IXrUiCameraProvider provider)
        {
            if (ReferenceEquals(_cameraProvider, provider))
            {
                return;
            }

            _cameraProvider = provider;
            CameraProviderChanged?.Invoke();
        }

        public static void ClearCameraProvider(IXrUiCameraProvider provider)
        {
            if (!ReferenceEquals(_cameraProvider, provider))
            {
                return;
            }

            _cameraProvider = null;
            CameraProviderChanged?.Invoke();
        }

        public static void SetSettingsProvider(IXrUiSettingsProvider provider)
        {
            if (ReferenceEquals(_settingsProvider, provider))
            {
                return;
            }

            _settingsProvider = provider;
            SettingsProviderChanged?.Invoke();
        }

        public static void ClearSettingsProvider(IXrUiSettingsProvider provider)
        {
            if (!ReferenceEquals(_settingsProvider, provider))
            {
                return;
            }

            _settingsProvider = null;
            SettingsProviderChanged?.Invoke();
        }

        public static void SetLifecycleProvider(IXrUiLifecycleProvider provider)
        {
            if (ReferenceEquals(_lifecycleProvider, provider))
            {
                return;
            }

            _lifecycleProvider = provider;
            LifecycleProviderChanged?.Invoke();
        }

        public static void ClearLifecycleProvider(IXrUiLifecycleProvider provider)
        {
            if (!ReferenceEquals(_lifecycleProvider, provider))
            {
                return;
            }

            _lifecycleProvider = null;
            LifecycleProviderChanged?.Invoke();
        }

        public static Camera ResolveEventCamera(Canvas canvas = null)
        {
            if (canvas != null && canvas.worldCamera != null)
            {
                return canvas.worldCamera;
            }

            Camera provided = _cameraProvider?.EventCamera;
            return provided != null ? provided : Camera.main;
        }

        public static Transform ResolveHeadTransform()
        {
            Transform provided = _cameraProvider?.HeadTransform;
            if (provided != null)
            {
                return provided;
            }

            Camera camera = ResolveEventCamera();
            return camera != null ? camera.transform : null;
        }
    }
}
