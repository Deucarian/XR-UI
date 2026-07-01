using System.Collections;
using Deucarian.XRUI.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Deucarian.XRUI
{
    /// <summary>
    /// Ensures ALL World-Space Canvases have a valid Event Camera.
    /// Uses the registered XR UI camera provider, falls back to Camera.main.
    /// Re-applies on scene load to catch newly spawned canvases.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class WorldCanvasEventCameraAssigner : MonoBehaviour
    {
        private const float SCAN_INTERVAL_SECONDS = 1f;

        private static WorldCanvasEventCameraAssigner _runtimeAssigner;
        private static UnityEngine.Camera _cachedEventCam;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateRuntimeAssigner()
        {
            if (_runtimeAssigner != null ||
                FindFirstObjectByType<WorldCanvasEventCameraAssigner>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            var assignerObject = new GameObject("[Deucarian XR UI] World Canvas Event Camera Assigner")
            {
                    hideFlags = HideFlags.HideAndDontSave,
            };

            DontDestroyOnLoad(assignerObject);
            _runtimeAssigner = assignerObject.AddComponent<WorldCanvasEventCameraAssigner>();
        }

        private void OnEnable()
        {
            _runtimeAssigner = this;
            XrUiRuntimeServices.CameraProviderChanged += OnCameraProviderChanged;
            CacheEventCamera();
            AssignAllWorldSpaceCanvases(true);
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartCoroutine(AssignPeriodically());
        }

        private void OnDisable()
        {
            XrUiRuntimeServices.CameraProviderChanged -= OnCameraProviderChanged;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            StopAllCoroutines();
            if (_runtimeAssigner == this)
            {
                _runtimeAssigner = null;
            }
        }

        private static void OnSceneLoaded(Scene _, LoadSceneMode __)
        {
            CacheEventCamera();
            AssignAllWorldSpaceCanvases(true);
        }

        private static void OnCameraProviderChanged()
        {
            CacheEventCamera();
            AssignAllWorldSpaceCanvases(true);
        }

        private IEnumerator AssignPeriodically()
        {
            while (enabled)
            {
                AssignAllWorldSpaceCanvases();
                yield return new WaitForSeconds(SCAN_INTERVAL_SECONDS);
            }
        }

        public static UnityEngine.Camera ResolveEventCamera(Canvas canvas)
        {
            if (canvas != null && canvas.worldCamera != null)
            {
                return canvas.worldCamera;
            }

            CacheEventCamera();
            return _cachedEventCam;
        }

        /// <summary>Call this manually after Addressables/instantiation if needed.</summary>
        public static void AssignAllWorldSpaceCanvases(bool refreshCustomPressables = false)
        {
            CacheEventCamera();
            if (_cachedEventCam == null) return;

            // Include inactive, cover all scenes currently loaded
            var canvases = Resources.FindObjectsOfTypeAll<Canvas>();
            for (int i = 0; i < canvases.Length; i++)
            {
                AssignWorldSpaceCanvas(canvases[i], refreshCustomPressables);
            }
        }

        public static void AssignWorldSpaceCanvasesIn(GameObject root, bool refreshCustomPressables = false)
        {
            if (root == null)
            {
                return;
            }

            CacheEventCamera();
            if (_cachedEventCam == null) return;

            Canvas[] canvases = root.GetComponentsInChildren<Canvas>(true);
            for (int i = 0; i < canvases.Length; i++)
            {
                AssignWorldSpaceCanvas(canvases[i], refreshCustomPressables);
            }
        }

        private static void AssignWorldSpaceCanvas(Canvas canvas, bool refreshCustomPressables)
        {
            if (!IsValidWorldSpaceCanvas(canvas) || _cachedEventCam == null)
            {
                return;
            }

            if (canvas.worldCamera != _cachedEventCam)
            {
                canvas.worldCamera = _cachedEventCam;
            }

            // Optional: tiny safety for plane-distance ties; only set if unset.
            if (canvas.planeDistance <= 0f)
            {
                canvas.planeDistance = 1f;
            }

            if (refreshCustomPressables)
            {
                RefreshCustomPressableBindings(canvas);
            }
        }

        private static bool IsValidWorldSpaceCanvas(Canvas c)
        {
            // Filter out prefab assets and non-world canvases
            if (c == null) return false;
#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.IsPersistent(c)) return false; // skip assets/prefabs
#endif
            return c.renderMode == RenderMode.WorldSpace;
        }

        private static void CacheEventCamera()
        {
            _cachedEventCam = XrUiRuntimeServices.ResolveEventCamera();
        }

        private static void RefreshCustomPressableBindings(Canvas canvas)
        {
            if (canvas == null)
            {
                return;
            }

            CustomPressableSurface[] surfaces = canvas.GetComponentsInChildren<CustomPressableSurface>(true);
            for (int i = 0; i < surfaces.Length; i++)
            {
                if (surfaces[i] == null)
                {
                    continue;
                }

                surfaces[i].RefreshPokeTargetBindings();
            }

            CustomSelectableFeedback[] feedbacks = canvas.GetComponentsInChildren<CustomSelectableFeedback>(true);
            for (int i = 0; i < feedbacks.Length; i++)
            {
                if (feedbacks[i] == null)
                {
                    continue;
                }

                feedbacks[i].RefreshPokeTargetBindings();
            }
        }
    }
}
