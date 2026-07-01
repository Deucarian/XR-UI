using System.Collections;
using TMPro;
using Deucarian.Common;
using Deucarian.XRUI;
using Deucarian.XRUI.Dropdowns;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Deucarian.XRUI.Controls
{
    [DefaultExecutionOrder(1001)]
    public sealed class CustomSelectableFeedbackInstaller : MonoBehaviour
    {
        #region Constants and Fields
        private const float SCAN_INTERVAL_SECONDS = 1f;

        private static CustomSelectableFeedbackInstaller _instance;
        #endregion

        #region Unity Methods
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateRuntimeInstaller()
        {
            if (_instance != null)
            {
                return;
            }

            var installerObject = new GameObject("[Deucarian XR UI] Custom Selectable Feedback Installer")
            {
                    hideFlags = HideFlags.HideAndDontSave,
            };

            DontDestroyOnLoad(installerObject);
            _instance = installerObject.AddComponent<CustomSelectableFeedbackInstaller>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartCoroutine(InstallPeriodically());
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            StopAllCoroutines();
        }
        #endregion

        #region Public Methods
        public static bool Supports(Selectable selectable)
        {
            if (selectable == null)
            {
                return false;
            }

            if (XrUiControlExclusionRegistry.IsExcluded(selectable))
            {
                return false;
            }

            return selectable is Button ||
                   selectable is Toggle ||
                   selectable is Slider ||
                   selectable is UnityEngine.UI.Scrollbar ||
                   selectable is Dropdown ||
                   selectable is TMP_InputField ||
                   selectable.GetType().FullName == "TMPro.TMP_Dropdown";
        }
        #endregion

        #region Private Methods
        private IEnumerator InstallPeriodically()
        {
            yield return null;

            while (enabled)
            {
                InstallOnLoadedCanvases();
                yield return new WaitForSeconds(SCAN_INTERVAL_SECONDS);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            InstallOnLoadedCanvases();
        }

        private static void InstallOnLoadedCanvases()
        {
            Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                if (canvas == null || !canvas.gameObject.scene.IsValid())
                {
                    continue;
                }

                InstallOnCanvas(canvas);
            }
        }

        private static void InstallOnCanvas(Canvas canvas)
        {
            if (XrUiControlExclusionRegistry.IsExcluded(canvas.transform))
            {
                return;
            }

            Selectable[] selectables = canvas.GetComponentsInChildren<Selectable>(true);
            foreach (Selectable selectable in selectables)
            {
                if (!Supports(selectable))
                {
                    continue;
                }

                RemoveLegacyPokeFollowComponents(selectable);

                CustomPressableSurface pressableSurface = selectable.GetComponent<CustomPressableSurface>();
                if (pressableSurface == null)
                {
                    pressableSurface = selectable.gameObject.AddComponent<CustomPressableSurface>();
                }

                if (selectable.GetComponent<CustomSelectableFeedback>() != null)
                {
                    ConfigureSpecializedControl(selectable, pressableSurface);
                    continue;
                }

                selectable.gameObject.AddComponent<CustomSelectableFeedback>();
                ConfigureSpecializedControl(selectable, pressableSurface);
            }
        }

        public static bool IsExcludedFromCustomFeedback(Transform transform)
        {
            return XrUiControlExclusionRegistry.IsExcluded(transform);
        }

        private static void ConfigureSpecializedControl(Selectable selectable, CustomPressableSurface pressableSurface)
        {
            if (selectable == null)
            {
                return;
            }

            if (selectable.TryGetComponent(out SliderToggle sliderToggle))
            {
                sliderToggle.BindPressableSurface(pressableSurface);
            }

            if (selectable is Toggle toggle && selectable.GetComponent<CustomTogglePressTarget>() == null)
            {
                CustomTogglePressTarget target = toggle.gameObject.AddComponent<CustomTogglePressTarget>();
                target.Configure(IsDropdownOptionToggle(toggle)
                                         ? CustomTogglePressTarget.ActivationMode.SelectOn
                                         : CustomTogglePressTarget.ActivationMode.Toggle);
            }

            if (NeedsDropdownPressTarget(selectable) && selectable.GetComponent<CustomDropdownPressTarget>() == null)
            {
                selectable.gameObject.AddComponent<CustomDropdownPressTarget>();
            }

            if (selectable is TMP_InputField inputField)
            {
                CustomInputFieldPressTarget inputTarget = inputField.GetComponent<CustomInputFieldPressTarget>();
                if (inputTarget == null)
                {
                    inputTarget = inputField.gameObject.AddComponent<CustomInputFieldPressTarget>();
                }

                inputTarget.Configure(inputField, pressableSurface);
                inputField.shouldActivateOnSelect = false;

                RefreshOptionalPressTargetBindings(inputField);
            }

            if (selectable.TryGetComponent(out TmpDropdownNoBlockerToggle noBlockerToggle))
            {
                noBlockerToggle.BindPressableSurface(pressableSurface);
            }
        }

        private static void RemoveLegacyPokeFollowComponents(Selectable selectable)
        {
            if (selectable == null)
            {
                return;
            }

            MonoBehaviour[] behaviours = selectable.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null)
                {
                    continue;
                }

                string typeName = behaviour.GetType().Name;
                if (typeName != "XRPokeFollowAffordance" && typeName != "XrUiPokeFollowAffordance")
                {
                    continue;
                }

                UnityObjectUtility.DestroySafely(behaviour);
            }
        }

        private static void RefreshOptionalPressTargetBindings(Component component)
        {
            if (component == null)
            {
                return;
            }

            MonoBehaviour[] behaviours = component.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null)
                {
                    continue;
                }

                System.Reflection.MethodInfo method = behaviour.GetType().GetMethod(
                        "RefreshPressTargetBinding",
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic,
                        null,
                        System.Type.EmptyTypes,
                        null);

                method?.Invoke(behaviour, null);
            }
        }

        private static bool NeedsDropdownPressTarget(Selectable selectable)
        {
            return selectable is Dropdown && !(selectable is CustomDropdown) ||
                   selectable.GetType().FullName == "TMPro.TMP_Dropdown";
        }

        private static bool IsDropdownOptionToggle(Toggle toggle)
        {
            Transform current = toggle.transform;
            while (current != null)
            {
                if (current.name == "Dropdown List")
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }
        #endregion
    }
}
