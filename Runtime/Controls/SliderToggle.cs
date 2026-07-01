using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Deucarian.XRUI.Controls
{
    [DefaultExecutionOrder(200)]
    public class SliderToggle : MonoBehaviour,
                                IPointerClickHandler,
                                ICustomPressableSelectableVisualOverride,
                                ICustomPressActivationTarget
    {
        #region Constants and Fields
        private bool _previousValue;
        private Slider _slider;
        private Coroutine _animateSliderCoroutine;
        private CustomPressableSurface _pressableSurface;
        private CustomPressActivationGate _activationGate;
        #endregion

        #region Serialized Fields
        [Header("Slider setup")]
        [SerializeField] [Range(0, 1f)] protected float _sliderValue;

        [Header("Animation")]
        [SerializeField] [Range(0, 1f)] private float _animationDuration = 0.5f;
        [SerializeField] private AnimationCurve _slideEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
       
        
        [Header("Colors")]
        private Graphic _handleGraphic;
        private CustomSelectableFeedback _selectableFeedback;
        private Color _lastFeedbackBaseColor;
        private bool _hasLastFeedbackBaseColor;

        #endregion

        #region Public Events and Delegates
        public event Action<bool> OnToggle;
        #endregion

        #region Public Properties
        public bool IsOn => CurrentValue;
        public bool TreatSelectableAsInteractableForCustomFeedback => enabled;
        public bool TreatSelectableAsVisuallyInteractableForCustomFeedback => enabled;
        #endregion

        #region Private Properties
        private bool CurrentValue { get; set; }
        private CustomPressActivationGate ActivationGate => _activationGate ??= new CustomPressActivationGate(this);
        #endregion

        #region Unity Methods
        protected virtual void OnValidate()
        {
            SetupToggleComponents();

            if (_slider != null)
            {
                _slider.value = _sliderValue;
                UpdateHandleColor(_sliderValue);
            }
        }

        protected virtual void Awake()
        {
            SetupSliderComponent();
            SyncCurrentValueFromSlider();
            SetupPressableSurface();
        }

        protected virtual void OnEnable()
        {
            SetupPressableSurface();
            SubscribePressableSurface();
            XrUiColorPalette.PaletteChanged -= OnPaletteChanged;
            XrUiColorPalette.PaletteChanged += OnPaletteChanged;
            ApplySliderPalette();
        }

        protected virtual void OnDisable()
        {
            UnsubscribePressableSurface();
            XrUiColorPalette.PaletteChanged -= OnPaletteChanged;
            StopActiveAnimation();
        }

        protected virtual void OnDestroy()
        {
            UnsubscribePressableSurface();
            XrUiColorPalette.PaletteChanged -= OnPaletteChanged;
            StopActiveAnimation();
        }

        private void LateUpdate()
        {
            if (_slider == null)
            {
                return;
            }

            UpdateHandleColor(_sliderValue);
        }
        #endregion

        #region Public Methods
        public void BindPressableSurface(CustomPressableSurface pressableSurface)
        {
            if (_pressableSurface == pressableSurface)
            {
                return;
            }

            UnsubscribePressableSurface();
            _pressableSurface = pressableSurface;
            SubscribePressableSurface();
        }

        public void SetIsOnWithoutNotify(bool state)
        {
            SetupSliderComponent();

            float targetValue = state ? 1f : 0f;
            if (CurrentValue == state && _animateSliderCoroutine != null && Application.isPlaying)
            {
                return;
            }

            if (_animateSliderCoroutine != null)
            {
                StopActiveAnimation();
            }

            _previousValue = CurrentValue;
            CurrentValue = state;
            _sliderValue = targetValue;

            if (_slider != null)
            {
                _slider.value = targetValue;
            }

            UpdateHandleColor(_sliderValue);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_pressableSurface != null)
            {
                return;
            }

            Toggle();
        }
        
        public void SetIsOn(bool state, bool notify = true)
        {
            SetStateAndStartAnimation(state, notify);
        }
        #endregion

        #region Private Methods
        private void SetupToggleComponents()
        {
            if (_slider != null)
            {
                return;
            }

            SetupSliderComponent();
        }

        private void SetupSliderComponent()
        {
            _slider = GetComponent<Slider>();

            if (_slider == null)
            {
                return;
            }

            _slider.interactable = false;
            ColorBlock sliderColors = _slider.colors;
            sliderColors.disabledColor = ColorPalette.ImageColor;
            _slider.colors = sliderColors;
            _slider.transition = Selectable.Transition.None;

            if (_slider.handleRect != null)
            {
                Graphic resolvedHandleGraphic = _slider.handleRect.GetComponent<Graphic>();
                _handleGraphic = resolvedHandleGraphic;
            }

            if (_selectableFeedback == null)
            {
                _selectableFeedback = GetComponent<CustomSelectableFeedback>();
            }

            if (_handleGraphic != null)
            {
                UpdateHandleColor(_slider.value);
            }
        }

        private void SetupPressableSurface()
        {
            if (_pressableSurface == null)
            {
                _pressableSurface = GetComponent<CustomPressableSurface>();
            }
        }

        private void SubscribePressableSurface()
        {
            ActivationGate.Bind(_pressableSurface, this);
        }

        private void UnsubscribePressableSurface()
        {
            _activationGate?.Unbind();
        }

        public bool CanActivatePress(CustomPressableSurface _) => isActiveAndEnabled;

        public void ActivateFromPress(CustomPressableSurface _)
        {
            Toggle();
        }

        private void UpdateHandleColor(float value)
        {
            if (_handleGraphic == null)
            {
                return;
            }

            Color baseColor = Color.Lerp(ColorPalette.ImageColor, ColorPalette.PrimaryColor, value);
            if (_selectableFeedback == null)
            {
                _selectableFeedback = GetComponent<CustomSelectableFeedback>();
            }

            if (_selectableFeedback == null)
            {
                _handleGraphic.color = baseColor;
                _hasLastFeedbackBaseColor = false;
                return;
            }

            if (_handleGraphic.color != Color.white)
            {
                _handleGraphic.color = Color.white;
            }

            if (_hasLastFeedbackBaseColor && AreColorsApproximatelyEqual(_lastFeedbackBaseColor, baseColor))
            {
                return;
            }

            _selectableFeedback.SetBaseInteractionColor(baseColor, 1f, true);
            _lastFeedbackBaseColor = baseColor;
            _hasLastFeedbackBaseColor = true;
        }

        private static bool AreColorsApproximatelyEqual(Color a, Color b)
        {
            const float tolerance = 0.001f;
            return Mathf.Abs(a.r - b.r) <= tolerance &&
                   Mathf.Abs(a.g - b.g) <= tolerance &&
                   Mathf.Abs(a.b - b.b) <= tolerance &&
                   Mathf.Abs(a.a - b.a) <= tolerance;
        }

        private void ApplySliderPalette()
        {
            SetupSliderComponent();
            UpdateHandleColor(_sliderValue);
        }

        private void OnPaletteChanged(XrUiColorPalette _)
        {
            if (this == null)
            {
                XrUiColorPalette.PaletteChanged -= OnPaletteChanged;
                return;
            }

            ApplySliderPalette();
        }

        private void Toggle()
        {
            SetStateAndStartAnimation(!CurrentValue, true);
        }

        private void SetStateAndStartAnimation(bool state, bool notifyEvent)
        {
            if (_animateSliderCoroutine != null)
            {
                StopActiveAnimation();
            }
            
            _previousValue = CurrentValue;
            CurrentValue = state;

            if (!isActiveAndEnabled || !Application.isPlaying)
            {
                float targetValue = CurrentValue ? 1f : 0f;
                _slider.value = _sliderValue = targetValue;
                UpdateHandleColor(_sliderValue);
            }
            else
            {
                _animateSliderCoroutine = StartCoroutine(AnimateSlider());
            }

            if (notifyEvent && _previousValue != CurrentValue)
            {
                OnToggle?.Invoke(CurrentValue);
            }
        }

        private void StopActiveAnimation()
        {
            if (_animateSliderCoroutine == null)
            {
                return;
            }

            StopCoroutine(_animateSliderCoroutine);
            _animateSliderCoroutine = null;
        }

        private void SyncCurrentValueFromSlider()
        {
            if (_slider == null)
            {
                CurrentValue = _sliderValue >= 0.5f;
                _previousValue = CurrentValue;
                return;
            }

            CurrentValue = _slider.value >= 0.5f;
            _previousValue = CurrentValue;
            _sliderValue = CurrentValue ? 1f : 0f;
        }

        private IEnumerator AnimateSlider()
        {
            float startValue = _slider.value;
            float endValue = CurrentValue ? 1 : 0;

            float time = 0;
            if (_animationDuration > 0)
            {
                while (time < _animationDuration)
                {
                    time += Time.deltaTime;
                    float lerpFactor = _slideEase.Evaluate(time / _animationDuration);
                    _slider.value = _sliderValue = Mathf.Lerp(startValue, endValue, lerpFactor);
                    UpdateHandleColor(_sliderValue);
                    yield return null;
                }
            }

            _slider.value = endValue;
            _sliderValue = endValue;
            UpdateHandleColor(_sliderValue);
            _animateSliderCoroutine = null;
        }
        #endregion
    }
}
