using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Deucarian.XRUI.Controls
{
    [AddComponentMenu("UI (Canvas)/Custom Slider", 32)]
    [RequireComponent(typeof(CustomPressableSurface))]
    [RequireComponent(typeof(CustomSelectableFeedback))]
    public class CustomSlider : Slider, ICustomPressActivationTarget, ICustomPressReleaseTarget
    {
        private CustomPressableSurface _pressableSurface;
        private bool _isArmedForDrag;
        private bool _needsPointerDownForward;
        private bool _isMouseDragFallback;
        private bool _hasForwardedPointerDown;
        private CustomPressActivationGate _activationGate;

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

            _isArmedForDrag = false;
            _needsPointerDownForward = false;
            _isMouseDragFallback = false;
            _hasForwardedPointerDown = false;
            base.OnDisable();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _isMouseDragFallback = IsMouseOrPenPointer(eventData);

            if (_isArmedForDrag || _pressableSurface == null)
            {
                ForwardPointerDown(eventData);
                _needsPointerDownForward = false;
                return;
            }

            _needsPointerDownForward = true;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (_hasForwardedPointerDown)
            {
                base.OnPointerUp(eventData);
            }

            _isArmedForDrag = false;
            _needsPointerDownForward = false;
            _isMouseDragFallback = false;
            _hasForwardedPointerDown = false;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!_isArmedForDrag)
            {
                if (_isMouseDragFallback || IsMouseOrPenPointer(eventData))
                {
                    _isMouseDragFallback = true;
                    _isArmedForDrag = true;
                }
                else
                {
                    _needsPointerDownForward = true;
                    return;
                }
            }

            if (_needsPointerDownForward || !_hasForwardedPointerDown)
            {
                ForwardPointerDown(eventData);
                _needsPointerDownForward = false;
            }

            base.OnDrag(eventData);
        }

        private void ForwardPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            _hasForwardedPointerDown = true;
        }

        private static bool IsMouseOrPenPointer(PointerEventData eventData)
        {
            if (eventData == null)
            {
                return false;
            }

#if ENABLE_INPUT_SYSTEM
            if (eventData is ExtendedPointerEventData extendedPointerEventData)
            {
                return extendedPointerEventData.pointerType == UIPointerType.MouseOrPen;
            }
#endif

            return eventData.pointerId < 0;
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
            _isArmedForDrag = true;
            if (!_hasForwardedPointerDown)
            {
                _needsPointerDownForward = true;
            }
        }

        public void ReleaseFromPress(CustomPressableSurface _)
        {
            if (_isMouseDragFallback)
            {
                return;
            }

            _isArmedForDrag = false;
            _needsPointerDownForward = false;
        }
    }
}
