using System.Collections;
using Deucarian.XRUI.Dropdowns;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deucarian.XRUI.Controls
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CustomPressableSurface))]
    [RequireComponent(typeof(CustomSelectableFeedback))]
    public sealed class CustomDropdownPressTarget : MonoBehaviour,
                                                  ICustomPressableSelectableVisualOverride,
                                                  ICustomPressActivationTarget
    {
        private Dropdown _dropdown;
        private TMP_Dropdown _tmpDropdown;
        private Selectable _selectable;
        private CustomPressableSurface _pressableSurface;
        private readonly SelectablePressTargetState _selectableState = new();
        private CustomPressActivationGate _activationGate;
        private Coroutine _configureOptionsRoutine;

        [SerializeField] private bool _preserveEnabledVisuals = true;

        public bool TreatSelectableAsInteractableForCustomFeedback =>
                enabled &&
                _preserveEnabledVisuals &&
                _selectableState.OriginalInteractable &&
                !DropdownInteractionCoordinator.IsBlocked(_selectable);

        private CustomPressActivationGate ActivationGate => _activationGate ??= new CustomPressActivationGate(this);
        public bool TreatSelectableAsVisuallyInteractableForCustomFeedback =>
                enabled &&
                _preserveEnabledVisuals &&
                _selectableState.OriginalInteractable &&
                !DropdownInteractionCoordinator.IsBlocked(_selectable);

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnEnable()
        {
            EnsureReferences();
            _selectableState.Cache(_selectable);
            _selectableState.ApplyDisabledOverride(_preserveEnabledVisuals);
            ActivationGate.Bind(_pressableSurface, this);
        }

        private void OnDisable()
        {
            _activationGate?.Unbind();
            if (_configureOptionsRoutine != null)
            {
                StopCoroutine(_configureOptionsRoutine);
                _configureOptionsRoutine = null;
            }

            HideDropdown();
            DropdownInteractionCoordinator.End(this);
            _selectableState.Restore();
        }

        private void EnsureReferences()
        {
            if (_dropdown == null)
            {
                _dropdown = GetComponent<Dropdown>();
            }

            if (_tmpDropdown == null)
            {
                _tmpDropdown = GetComponent<TMP_Dropdown>();
            }

            if (_selectable == null)
            {
                _selectable = GetComponent<Selectable>();
            }

            CustomPressableComponentUtility.EnsurePressableFeedback(this, ref _pressableSurface);
        }

        public bool CanActivatePress(CustomPressableSurface _)
        {
            return _selectableState.OriginalInteractable &&
                   _selectable != null &&
                   isActiveAndEnabled &&
                   !DropdownInteractionCoordinator.IsBlocked(_selectable);
        }

        public void ActivateFromPress(CustomPressableSurface _)
        {
            DropdownInteractionCoordinator.Begin(this, _selectable);
            _selectable.interactable = true;

            if (_dropdown != null)
            {
                _dropdown.Show();
            }
            else if (_tmpDropdown != null)
            {
                _tmpDropdown.Show();
            }

            _selectable.interactable = false;

            if (_configureOptionsRoutine != null)
            {
                StopCoroutine(_configureOptionsRoutine);
            }

            _configureOptionsRoutine = StartCoroutine(ConfigureOptionsEndOfFrame());
        }

        private void HideDropdown()
        {
            if (_dropdown != null)
            {
                _dropdown.Hide();
            }
            else if (_tmpDropdown != null)
            {
                _tmpDropdown.Hide();
            }
        }

        private IEnumerator ConfigureOptionsEndOfFrame()
        {
            yield return new WaitForEndOfFrame();

            Canvas canvas = GetComponentInParent<Canvas>();
            Canvas rootCanvas = canvas != null ? canvas.rootCanvas : null;
            if (rootCanvas == null)
            {
                _configureOptionsRoutine = null;
                DropdownInteractionCoordinator.End(this);
                yield break;
            }

            Transform[] transforms = rootCanvas.GetComponentsInChildren<Transform>(true);
            foreach (Transform candidate in transforms)
            {
                if (candidate == null || candidate.name != "Dropdown List")
                {
                    continue;
                }

                Toggle[] optionToggles = candidate.GetComponentsInChildren<Toggle>(true);
                foreach (Toggle optionToggle in optionToggles)
                {
                    CustomDropdownOptionUtility.ConfigureOptionToggle(optionToggle);
                }
            }

            _configureOptionsRoutine = null;

            while (DropdownInteractionCoordinator.HasOpenDropdownList(this))
            {
                yield return null;
            }

            DropdownInteractionCoordinator.End(this);
        }
    }
}
