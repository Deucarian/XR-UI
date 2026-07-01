using System.Collections;
using Deucarian.Common;
using Deucarian.XRUI.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Deucarian.XRUI.XR
{
    [DefaultExecutionOrder(1000)]
    public sealed class XrUiPokeAffordanceInstaller : MonoBehaviour
    {
        const float k_ScanIntervalSeconds = 1f;

        static XrUiPokeAffordanceInstaller s_Instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void CreateRuntimeInstaller()
        {
            if (s_Instance != null)
                return;

            var installerObject = new GameObject("[Deucarian XR UI] XR UI Poke Affordance Installer")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };

            DontDestroyOnLoad(installerObject);
            s_Instance = installerObject.AddComponent<XrUiPokeAffordanceInstaller>();
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartCoroutine(InstallPeriodically());
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            StopAllCoroutines();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            InstallOnLoadedCanvases();
        }

        IEnumerator InstallPeriodically()
        {
            yield return null;

            while (enabled)
            {
                InstallOnLoadedCanvases();
                yield return new WaitForSeconds(k_ScanIntervalSeconds);
            }
        }

        void InstallOnLoadedCanvases()
        {
            var canvases = Resources.FindObjectsOfTypeAll<Canvas>();
            foreach (var canvas in canvases)
            {
                if (canvas == null || !canvas.gameObject.scene.IsValid())
                    continue;

                if (!canvas.TryGetComponent<TrackedDeviceGraphicRaycaster>(out _))
                    continue;

                InstallOnCanvas(canvas);
            }
        }

        static void InstallOnCanvas(Canvas canvas)
        {
            var selectables = canvas.GetComponentsInChildren<Selectable>(true);
            foreach (var selectable in selectables)
            {
                if (UsesCustomPressableSystem(selectable))
                {
                    RemoveLegacyPokeFollowComponents(selectable);
                    continue;
                }

                if (!ShouldInstall(selectable))
                    continue;

                var followTarget = ResolveFollowTarget(selectable);
                if (followTarget == null)
                    continue;

                var affordance = selectable.gameObject.AddComponent<XrUiPokeFollowAffordance>();
                affordance.PokeFollowTransform = followTarget;
            }
        }

        static bool ShouldInstall(Selectable selectable)
        {
            if (selectable == null)
                return false;

            if (UsesCustomPressableSystem(selectable))
                return false;

            if (selectable.GetComponent<XrUiPokeFollowAffordance>() != null)
                return false;

            if (HasExistingXriPokeFollowAffordance(selectable))
                return false;

            if (CustomSelectableFeedbackInstaller.Supports(selectable))
                return false;

            return selectable is Button ||
                   selectable is Toggle ||
                   selectable is Slider ||
                   selectable is Scrollbar ||
                   selectable is Dropdown ||
                   selectable.GetType().FullName == "TMPro.TMP_Dropdown";
        }

        static bool UsesCustomPressableSystem(Selectable selectable)
        {
            return selectable != null &&
                   (selectable.GetComponent<CustomPressableSurface>() != null ||
                    selectable.GetComponent<CustomSelectableFeedback>() != null);
        }

        static void RemoveLegacyPokeFollowComponents(Selectable selectable)
        {
            if (selectable == null)
                return;

            var behaviours = selectable.GetComponents<MonoBehaviour>();
            foreach (var behaviour in behaviours)
            {
                if (behaviour == null)
                    continue;

                var typeName = behaviour.GetType().Name;
                if (typeName != "XRPokeFollowAffordance" && typeName != "XrUiPokeFollowAffordance")
                    continue;

                UnityObjectUtility.DestroySafely(behaviour);
            }
        }

        static bool HasExistingXriPokeFollowAffordance(Selectable selectable)
        {
            var behaviours = selectable.GetComponents<MonoBehaviour>();
            foreach (var behaviour in behaviours)
            {
                if (behaviour == null)
                    continue;

                if (behaviour.GetType().Name == "XRPokeFollowAffordance")
                    return true;
            }

            return false;
        }

        static RectTransform ResolveFollowTarget(Selectable selectable)
        {
            if (selectable.targetGraphic != null &&
                selectable.targetGraphic.transform != selectable.transform &&
                selectable.targetGraphic.transform.parent == selectable.transform &&
                selectable.targetGraphic.transform is RectTransform targetGraphicTransform)
            {
                return targetGraphicTransform;
            }

            for (var i = 0; i < selectable.transform.childCount; i++)
            {
                if (selectable.transform.GetChild(i) is RectTransform child)
                    return child;
            }

            return null;
        }
    }
}
