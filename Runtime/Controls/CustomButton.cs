using Deucarian.XRUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Deucarian.XRUI.Controls
{
    [AddComponentMenu("UI (Canvas)/Custom Button", 31)]
    [RequireComponent(typeof(CustomSelectableFeedback))]
    [RequireComponent(typeof(CustomPressableSurface))]
    public class CustomButton : Button, ICustomPressActivationTarget
    {
        [FormerlySerializedAs("OnButtonClick")]
        [SerializeField] private Button.ButtonClickedEvent _onButtonClick = new Button.ButtonClickedEvent();

        private CustomPressableSurface _pressableSurface;
        private CustomPressActivationGate _activationGate;

        public Button.ButtonClickedEvent OnButtonClick
        {
            get => _onButtonClick;
            set => _onButtonClick = value;
        }

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
            base.OnDisable();
        }

        public override void OnPointerClick(PointerEventData _)
        {
            // Activation is owned by CustomPressableSurface so release-clicks cannot fire early.
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            EnsureComponents();
            if (_pressableSurface != null && _pressableSurface.isActiveAndEnabled)
            {
                return;
            }

            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            EnsureComponents();
            if (_pressableSurface != null && _pressableSurface.isActiveAndEnabled)
            {
                return;
            }

            base.OnPointerUp(eventData);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            EnsureComponents();
            if (_pressableSurface != null && _pressableSurface.isActiveAndEnabled)
            {
                return;
            }

            base.OnSelect(eventData);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            EnsureComponents();
            if (_pressableSurface != null && _pressableSurface.isActiveAndEnabled)
            {
                return;
            }

            InvokeCustomButtonClick();
            base.OnSubmit(eventData);
        }

        public bool CanActivatePress(CustomPressableSurface _)
        {
            return IsActive() && IsInteractable();
        }

        public void ActivateFromPress(CustomPressableSurface _)
        {
            _onButtonClick?.Invoke();
            onClick.Invoke();
        }

        private void InvokeCustomButtonClick()
        {
            if (!IsActive() || !IsInteractable())
            {
                return;
            }

            _onButtonClick?.Invoke();
        }

        private void EnsureComponents()
        {
            CustomPressableComponentUtility.EnsurePressableFeedback(this, ref _pressableSurface);
            if (targetGraphic == null)
            {
                targetGraphic = ResolveTargetGraphic();
            }
        }

        private Graphic ResolveTargetGraphic()
        {
            Graphic graphic = GetComponent<Graphic>() ?? GetComponentInChildren<Graphic>(true);
            if (graphic != null || transform is not RectTransform)
            {
                return graphic;
            }

            var image = gameObject.AddComponent<Image>();
            image.color = ColorPalette.BackgroundColor;
            return image;
        }
    }
}
