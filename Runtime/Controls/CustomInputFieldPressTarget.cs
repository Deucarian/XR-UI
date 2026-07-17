using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deucarian.XRUI.Controls
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_InputField))]
    [RequireComponent(typeof(CustomPressableSurface))]
    public sealed class CustomInputFieldPressTarget : MonoBehaviour,
                                                     ICustomPressActivationTarget,
                                                     ICustomPressSelectedHoldTarget,
                                                     ICustomPressActivationStatus,
                                                     IPointerClickHandler,
                                                     ISelectHandler,
                                                     IDeselectHandler
    {
        #region Constants and Fields
        private TMP_InputField _inputField;
        private CustomPressableSurface _pressableSurface;
        private CustomPressActivationGate _activationGate;
        private bool _originalShouldActivateOnSelect;
        private bool _hasOriginalShouldActivateOnSelect;
        private Coroutine _deactivateCoroutine;
        #endregion

        #region Public Properties
        public bool IsPressActivationSatisfied => ActivationGate.IsPressActivationSatisfied;
        private CustomPressActivationGate ActivationGate => _activationGate ??= new CustomPressActivationGate(this);
        #endregion

        #region Unity Methods
        private void Awake()
        {
            EnsureReferences();
            CacheOriginalActivationMode();
        }

        private void OnEnable()
        {
            EnsureReferences();
            CacheOriginalActivationMode();

            if (_inputField != null)
            {
                TmpInputFieldActivationCompatibility.SetShouldActivateOnSelect(_inputField, false);
            }

            if (_pressableSurface != null)
            {
                ActivationGate.Bind(_pressableSurface, this);
            }
        }

        private void OnDisable()
        {
            _activationGate?.Unbind();

            if (_inputField != null && _hasOriginalShouldActivateOnSelect)
            {
                TmpInputFieldActivationCompatibility.SetShouldActivateOnSelect(
                    _inputField,
                    _originalShouldActivateOnSelect);
            }

            StopPendingCoroutines();
            ClearSelectedHold();
        }
        #endregion

        #region Event Methods
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            CancelUngatedActivation();
        }

        public void OnSelect(BaseEventData _) => CancelUngatedActivation();

        public void OnDeselect(BaseEventData _)
        {
            _activationGate?.ClearActivation();
            ClearSelectedHold();
        }
        #endregion

        #region Public Methods
        public void Configure(TMP_InputField inputField, CustomPressableSurface pressableSurface)
        {
            _inputField = inputField;
            _pressableSurface = pressableSurface;
            CacheOriginalActivationMode();

            if (_inputField != null)
            {
                TmpInputFieldActivationCompatibility.SetShouldActivateOnSelect(_inputField, false);
            }
        }
        #endregion

        #region Private Methods
        private void EnsureReferences()
        {
            if (_inputField == null)
            {
                _inputField = GetComponent<TMP_InputField>();
            }

            if (_pressableSurface == null)
            {
                _pressableSurface = GetComponent<CustomPressableSurface>();
            }
        }

        private void CacheOriginalActivationMode()
        {
            if (_hasOriginalShouldActivateOnSelect || _inputField == null)
            {
                return;
            }

            _hasOriginalShouldActivateOnSelect = TmpInputFieldActivationCompatibility.TryGetShouldActivateOnSelect(
                _inputField,
                out _originalShouldActivateOnSelect);
        }

        public bool CanActivatePress(CustomPressableSurface _)
        {
            return _inputField != null && _inputField.interactable;
        }

        public void ActivateFromPress(CustomPressableSurface _)
        {
            if (_deactivateCoroutine != null)
            {
                StopCoroutine(_deactivateCoroutine);
                _deactivateCoroutine = null;
            }

            EventSystem eventSystem = EventSystem.current;
            if (eventSystem != null && eventSystem.currentSelectedGameObject != gameObject)
            {
                eventSystem.SetSelectedGameObject(gameObject);
            }

            _inputField.ActivateInputField();
            _inputField.MoveTextEnd(false);
        }

        public bool ShouldHoldSelectedPress(CustomPressableSurface _) => true;

        private void ClearSelectedHold()
        {
            _pressableSurface?.SetSelectedVisualHold(false);
        }

        private void CancelUngatedActivation()
        {
            if (ActivationGate.IsPressActivationSatisfied)
            {
                return;
            }

            if (!isActiveAndEnabled)
            {
                return;
            }

            if (_deactivateCoroutine != null)
            {
                StopCoroutine(_deactivateCoroutine);
            }

            _deactivateCoroutine = StartCoroutine(DeactivateAfterEventProcessing());
        }

        private IEnumerator DeactivateAfterEventProcessing()
        {
            yield return null;
            _deactivateCoroutine = null;

            if (ActivationGate.IsPressActivationSatisfied || _inputField == null)
            {
                yield break;
            }

            _inputField.DeactivateInputField();
            ClearSelectedHold();

            EventSystem eventSystem = EventSystem.current;
            if (eventSystem != null && eventSystem.currentSelectedGameObject == gameObject)
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }

        private void StopPendingCoroutines()
        {
            if (_deactivateCoroutine != null)
            {
                StopCoroutine(_deactivateCoroutine);
                _deactivateCoroutine = null;
            }
        }
        #endregion
    }

    internal static class TmpInputFieldActivationCompatibility
    {
        private static readonly PropertyInfo ShouldActivateOnSelectProperty = typeof(TMP_InputField).GetProperty(
            "shouldActivateOnSelect",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        internal static bool TryGetShouldActivateOnSelect(TMP_InputField inputField, out bool value)
        {
            value = false;
            if (inputField == null || ShouldActivateOnSelectProperty == null ||
                ShouldActivateOnSelectProperty.PropertyType != typeof(bool) ||
                !ShouldActivateOnSelectProperty.CanRead)
            {
                return false;
            }

            value = (bool)ShouldActivateOnSelectProperty.GetValue(inputField);
            return true;
        }

        internal static void SetShouldActivateOnSelect(TMP_InputField inputField, bool value)
        {
            if (inputField == null || ShouldActivateOnSelectProperty == null ||
                ShouldActivateOnSelectProperty.PropertyType != typeof(bool) ||
                !ShouldActivateOnSelectProperty.CanWrite)
            {
                return;
            }

            ShouldActivateOnSelectProperty.SetValue(inputField, value);
        }
    }
}
