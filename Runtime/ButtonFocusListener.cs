using System;
using Deucarian.XRUI.Controls;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deucarian.XRUI
{
    [RequireComponent(typeof(Button))]
    public class ButtonFocusListener : MonoBehaviour,
                                              ICustomPressActivationTarget,
                                              ICustomPressSelectedHoldTarget,
                                              IPointerClickHandler
    {
        private Button _button;
        private CustomButton _customButton;
        private CustomSelectableFeedback _feedback;
        private CustomPressableSurface _pressableSurface;
        private CustomPressActivationGate _activationGate;
        private Type _scopeType;
        private Func<bool> _validator;

        public event Action OnFocusFailed;
        public event Action OnFocusGained;
        public event Action OnFocusLost;

        public bool IsSelected { get; private set; }

        private CustomPressActivationGate ActivationGate => _activationGate ??= new CustomPressActivationGate(this);

        private void Awake()
        {
            EnsureRefs();
            IsSelected = false;
        }

        private void OnEnable()
        {
            EnsureRefs();
            RegisterScopeListener();
            BindPressableSurface();
            ApplySelectedPressVisual();
        }

        private void OnDisable()
        {
            ApplySelectedPressVisual(false);
            UnbindPressableSurface();
            if (IsSelected)
            {
                InternalDeselect(false);
            }

            UnregisterScopeListener();
            ButtonFocusScopeRegistry.ClearSelected(_scopeType, this);
        }

        public void Initialize<T>(Func<bool> validator = null)
        {
            UnregisterScopeListener();
            _validator = validator;
            _scopeType = typeof(T);
            ButtonFocusScopeRegistry.EnsureScope(_scopeType);
            RegisterScopeListener();
            EnsureRefs();
            BindPressableSurface();
        }

        public void SetSelected(bool selected, bool invokeEvents = false)
        {
            if (_scopeType == null)
            {
                return;
            }

            EnsureRefs();
            if (_button == null)
            {
                return;
            }

            ButtonFocusListener current = ButtonFocusScopeRegistry.GetSelected(_scopeType);

            if (selected)
            {
                if (current != this)
                {
                    DeselectScopeExcept(this, invokeEvents);
                    ButtonFocusScopeRegistry.SetSelected(_scopeType, this);
                    InternalSelect(invokeEvents);
                }
                else if (!IsSelected)
                {
                    DeselectScopeExcept(this, invokeEvents);
                    InternalSelect(invokeEvents);
                }
                else
                {
                    DeselectScopeExcept(this, invokeEvents);
                }
            }
            else
            {
                if (current == this)
                {
                    ButtonFocusScopeRegistry.ClearSelected(_scopeType, this);
                }

                if (IsSelected)
                {
                    InternalDeselect(invokeEvents);
                }
                else
                {
                    ApplySelectedPressVisual(false);
                    RefreshFeedbackVisual();
                }
            }
        }

        public void OnPointerClick(PointerEventData _)
        {
            if (UsesPressDepthActivation())
            {
                return;
            }

            ToggleFocus(true);
        }

        public void ForceSelect() => SetSelected(true, true);
        public void ForceDeselect() => SetSelected(false, true);

        public bool CanActivatePress(CustomPressableSurface _)
        {
            EnsureRefs();
            return _button != null && isActiveAndEnabled;
        }

        public void ActivateFromPress(CustomPressableSurface _)
        {
            ToggleFocus(true);
        }

        public bool ShouldHoldSelectedPress(CustomPressableSurface _) => IsSelected;

        private void OnCustomButtonClick()
        {
            ToggleFocus(true);
        }

        private void ApplySelectedPressVisual()
        {
            ApplySelectedPressVisual(IsSelected);
        }

        private void ApplySelectedPressVisual(bool selected)
        {
            EnsureRefs();
            _pressableSurface?.SetSelectedVisualHold(selected);
        }

        private void ToggleFocus(bool invokeEvents)
        {
            EnsureRefs();
            if (_button == null)
            {
                return;
            }

            ButtonFocusListener prev = ButtonFocusScopeRegistry.GetSelected(_scopeType);
            if (prev != this)
            {
                DeselectScopeExcept(this, true);

                if (_validator != null && !_validator())
                {
                    ButtonFocusScopeRegistry.ClearSelected(_scopeType, this);
                    OnFocusFailed?.Invoke();
                    return;
                }

                InternalSelect(true);
                ButtonFocusScopeRegistry.SetSelected(_scopeType, this);
            }
            else
            {
                if (!IsSelected)
                {
                    DeselectScopeExcept(this, invokeEvents);
                    InternalSelect(invokeEvents);
                    return;
                }

                ButtonFocusScopeRegistry.ClearSelected(_scopeType, this);
                InternalDeselect(invokeEvents);
            }
        }

        private void EnsureRefs()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            if (_customButton == null)
            {
                _customButton = GetComponent<CustomButton>();
            }

            if (_pressableSurface == null)
            {
                _pressableSurface = GetComponent<CustomPressableSurface>();
            }

            if (_feedback == null)
            {
                _feedback = GetComponent<CustomSelectableFeedback>();
            }
        }

        private void BindPressableSurface()
        {
            EnsureRefs();

            if (_customButton != null)
            {
                _customButton.OnButtonClick.RemoveListener(OnCustomButtonClick);
                _customButton.OnButtonClick.AddListener(OnCustomButtonClick);
                return;
            }

            if (_pressableSurface == null)
            {
                return;
            }

            ActivationGate.Bind(_pressableSurface, this);
        }

        private void UnbindPressableSurface()
        {
            if (_customButton != null)
            {
                _customButton.OnButtonClick.RemoveListener(OnCustomButtonClick);
            }

            if (_pressableSurface == null)
            {
                return;
            }

            _activationGate?.Unbind();
        }

        private bool UsesPressDepthActivation()
        {
            EnsureRefs();
            return (_customButton != null && _customButton.isActiveAndEnabled) ||
                   (_pressableSurface != null && _pressableSurface.isActiveAndEnabled);
        }

        private void RegisterScopeListener()
        {
            if (_scopeType == null)
            {
                return;
            }

            ButtonFocusScopeRegistry.Register(_scopeType, this);
        }

        private void UnregisterScopeListener()
        {
            ButtonFocusScopeRegistry.Unregister(_scopeType, this);
        }

        private void DeselectScopeExcept(ButtonFocusListener selected, bool invokeEvents)
        {
            ButtonFocusScopeRegistry.DeselectExcept(_scopeType, selected, invokeEvents);
        }

        private void RefreshFeedbackVisual()
        {
            EnsureRefs();
            _feedback?.RefreshVisualState();
        }

        private void InternalSelect(bool invokeEvents)
        {
            IsSelected = true;
            ApplySelectedPressVisual(true);
            RefreshFeedbackVisual();

            if (invokeEvents)
            {
                OnFocusGained?.Invoke();
            }
        }

        internal void InternalDeselect(bool invokeEvents)
        {
            IsSelected = false;
            ApplySelectedPressVisual(false);
            RefreshFeedbackVisual();

            if (invokeEvents)
            {
                OnFocusLost?.Invoke();
            }
        }

    }
}
