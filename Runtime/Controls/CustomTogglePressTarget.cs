using UnityEngine;
using UnityEngine.UI;

namespace Deucarian.XRUI.Controls
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Toggle))]
    [RequireComponent(typeof(CustomPressableSurface))]
    [RequireComponent(typeof(CustomSelectableFeedback))]
    public sealed class CustomTogglePressTarget : MonoBehaviour,
                                                 ICustomPressableSelectableVisualOverride,
                                                 ICustomPressActivationTarget
    {
        public enum ActivationMode
        {
            Toggle,
            SelectOn,
        }

        private Toggle _toggle;
        private CustomPressableSurface _pressableSurface;
        private readonly SelectablePressTargetState _selectableState = new();
        private CustomPressActivationGate _activationGate;

        [SerializeField] private ActivationMode _activationMode;
        [SerializeField] private bool _preserveEnabledVisuals = true;

        public ActivationMode Mode
        {
            get => _activationMode;
            set => _activationMode = value;
        }

        public bool TreatSelectableAsInteractableForCustomFeedback =>
                enabled && _preserveEnabledVisuals && _selectableState.OriginalInteractable;
        public bool TreatSelectableAsVisuallyInteractableForCustomFeedback =>
                enabled && _preserveEnabledVisuals && _selectableState.OriginalInteractable;

        private CustomPressActivationGate ActivationGate => _activationGate ??= new CustomPressActivationGate(this);

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnEnable()
        {
            EnsureReferences();
            _selectableState.Cache(_toggle);
            _selectableState.ApplyDisabledOverride(_preserveEnabledVisuals);
            ActivationGate.Bind(_pressableSurface, this);
        }

        private void OnDisable()
        {
            _activationGate?.Unbind();
            _selectableState.Restore();
        }

        public void Configure(ActivationMode activationMode)
        {
            _activationMode = activationMode;
        }

        private void EnsureReferences()
        {
            if (_toggle == null)
            {
                _toggle = GetComponent<Toggle>();
            }

            CustomPressableComponentUtility.EnsurePressableFeedback(this, ref _pressableSurface);
        }

        public bool CanActivatePress(CustomPressableSurface _)
        {
            return _toggle != null && _selectableState.OriginalInteractable && isActiveAndEnabled;
        }

        public void ActivateFromPress(CustomPressableSurface _)
        {
            switch (_activationMode)
            {
                case ActivationMode.SelectOn:
                    if (_toggle.isOn)
                    {
                        _toggle.onValueChanged.Invoke(true);
                    }
                    else
                    {
                        _toggle.isOn = true;
                    }
                    break;
                default:
                    _toggle.isOn = !_toggle.isOn;
                    break;
            }
        }
    }
}
