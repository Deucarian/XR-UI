using System.Collections;
using Deucarian.XRUI.Dropdowns;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deucarian.XRUI.Controls
{
    [AddComponentMenu("UI (Canvas)/Custom Dropdown", 33)]
    [RequireComponent(typeof(CustomPressableSurface))]
    [RequireComponent(typeof(CustomSelectableFeedback))]
    public class CustomDropdown : Dropdown, ICustomPressActivationTarget
    {
        private CustomPressableSurface _pressableSurface;
        private CustomPressActivationGate _activationGate;
        private Coroutine _dropdownStateRoutine;

        private CustomPressActivationGate ActivationGate => _activationGate ??= new CustomPressActivationGate(this);

        protected override void Awake()
        {
            base.Awake();
            EnsureComponents();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EnsureComponents();
            ActivationGate.Bind(_pressableSurface, this);
        }

        protected override void OnDisable()
        {
            _activationGate?.Unbind();

            StopDropdownStateRoutine();
            Hide();
            DropdownInteractionCoordinator.End(this);
            base.OnDisable();
        }

        public override void OnPointerClick(PointerEventData _)
        {
            // Opening is gated by physical press depth.
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            if (_pressableSurface != null && _pressableSurface.isActiveAndEnabled)
            {
                return;
            }

            ShowWithGlobalBlock();
            base.OnSubmit(eventData);
        }

        protected override DropdownItem CreateItem(DropdownItem itemTemplate)
        {
            DropdownItem item = base.CreateItem(itemTemplate);
            CustomDropdownOptionUtility.ConfigureOptionToggle(item != null ? item.toggle : null);
            return item;
        }

        private void EnsureComponents()
        {
            CustomPressableComponentUtility.EnsurePressableFeedback(this, ref _pressableSurface);
        }

        public bool CanActivatePress(CustomPressableSurface _)
        {
            return IsActive() && IsInteractable();
        }

        public void ActivateFromPress(CustomPressableSurface _)
        {
            ShowWithGlobalBlock();
        }

        private void ShowWithGlobalBlock()
        {
            DropdownInteractionCoordinator.Begin(this, this);
            Show();
            StartDropdownStateRoutine();
        }

        private void StartDropdownStateRoutine()
        {
            StopDropdownStateRoutine();
            _dropdownStateRoutine = StartCoroutine(EndGlobalBlockWhenClosed());
        }

        private void StopDropdownStateRoutine()
        {
            if (_dropdownStateRoutine == null)
            {
                return;
            }

            StopCoroutine(_dropdownStateRoutine);
            _dropdownStateRoutine = null;
        }

        private IEnumerator EndGlobalBlockWhenClosed()
        {
            yield return new WaitForEndOfFrame();

            while (DropdownInteractionCoordinator.HasOpenDropdownList(this))
            {
                yield return null;
            }

            _dropdownStateRoutine = null;
            DropdownInteractionCoordinator.End(this);
        }
    }

    [AddComponentMenu("UI (Canvas)/Custom TMP Dropdown", 34)]
    [RequireComponent(typeof(CustomPressableSurface))]
    [RequireComponent(typeof(CustomSelectableFeedback))]
    public class CustomTmpDropdown : TMP_Dropdown, ICustomPressActivationTarget
    {
        private CustomPressableSurface _pressableSurface;
        private CustomPressActivationGate _activationGate;
        private Coroutine _dropdownStateRoutine;

        private CustomPressActivationGate ActivationGate => _activationGate ??= new CustomPressActivationGate(this);

        protected override void Awake()
        {
            base.Awake();
            EnsureComponents();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EnsureComponents();
            ActivationGate.Bind(_pressableSurface, this);
        }

        protected override void OnDisable()
        {
            _activationGate?.Unbind();

            StopDropdownStateRoutine();
            Hide();
            DropdownInteractionCoordinator.End(this);
            base.OnDisable();
        }

        public override void OnPointerClick(PointerEventData _)
        {
            // Opening is gated by physical press depth.
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            if (_pressableSurface != null && _pressableSurface.isActiveAndEnabled)
            {
                return;
            }

            ShowWithGlobalBlock();
            base.OnSubmit(eventData);
        }

        protected override DropdownItem CreateItem(DropdownItem itemTemplate)
        {
            DropdownItem item = base.CreateItem(itemTemplate);
            CustomDropdownOptionUtility.ConfigureOptionToggle(item != null ? item.toggle : null);
            return item;
        }

        private void EnsureComponents()
        {
            CustomPressableComponentUtility.EnsurePressableFeedback(this, ref _pressableSurface);
        }

        public bool CanActivatePress(CustomPressableSurface _)
        {
            return IsActive() && IsInteractable();
        }

        public void ActivateFromPress(CustomPressableSurface _)
        {
            ShowWithGlobalBlock();
        }

        private void ShowWithGlobalBlock()
        {
            DropdownInteractionCoordinator.Begin(this, this);
            Show();
            StartDropdownStateRoutine();
        }

        private void StartDropdownStateRoutine()
        {
            StopDropdownStateRoutine();
            _dropdownStateRoutine = StartCoroutine(EndGlobalBlockWhenClosed());
        }

        private void StopDropdownStateRoutine()
        {
            if (_dropdownStateRoutine == null)
            {
                return;
            }

            StopCoroutine(_dropdownStateRoutine);
            _dropdownStateRoutine = null;
        }

        private IEnumerator EndGlobalBlockWhenClosed()
        {
            yield return new WaitForEndOfFrame();

            while (DropdownInteractionCoordinator.HasOpenDropdownList(this))
            {
                yield return null;
            }

            _dropdownStateRoutine = null;
            DropdownInteractionCoordinator.End(this);
        }
    }
}
