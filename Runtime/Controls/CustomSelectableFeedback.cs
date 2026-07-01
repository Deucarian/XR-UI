using TMPro;
using Deucarian.XRUI;
using Deucarian.XRUI.Controls;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Deucarian.XRUI.Controls
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(CustomPressableSurface))]
    public sealed class CustomSelectableFeedback : MonoBehaviour,
                                                   IPointerEnterHandler,
                                                   IPointerExitHandler,
                                                   IPointerMoveHandler,
                                                   IPointerDownHandler,
                                                   IPointerUpHandler,
                                                   ISelectHandler,
                                                   IDeselectHandler
    {
        private readonly CustomSelectablePokeTracker _pokeTracker = new();
        private readonly List<Graphic> _linkedColorTargets = new();

        private Selectable _selectable;
        private ButtonFocusListener _focusListener;
        private ICustomPressableSelectableVisualOverride _selectableOverride;
        private CustomPressableSurface _pressableSurface;
        private Graphic _targetGraphic;
        private RectTransform _rectTransform;
        private RectTransform _animationTarget;
        private Vector3 _baseScale = Vector3.one;
        private bool _hasBaseScale;
        private bool _pointerInside;
        private bool _pointerDown;
        private bool _selected;
        private float _pointerProximity;
        private float _currentMultiplier = 1f;
        private float _targetMultiplier = 1f;
        private XrUiSemanticColor _baseInteractionSemanticColor;
        private bool _hasSemanticBaseInteractionColor;

        [Header("Global Settings")]
        [SerializeField] private CustomButtonSettings _settings;

        [Header("Per Element Overrides")]
        [SerializeField] private bool _overrideHoverDistance;
        [SerializeField] [Min(0.001f)] private float _hoverDistance = 20f;
        [SerializeField] private bool _overrideHoverGradient;
        [SerializeField] private Gradient _hoverGradient = CustomButtonSettings.CreateDefaultHoverGradient();
        [SerializeField] private bool _overrideBaseInteractionColor;
        [SerializeField] private Color _baseInteractionColor = Color.white;
        [SerializeField] [Min(0f)] private float _baseInteractionSelectedBoost = 1f;
        [SerializeField] private bool _useBaseInteractionMultipliers;

        [Header("Targets")]
        [SerializeField] private Graphic _graphicOverride;
        [SerializeField] private RectTransform _animationTargetOverride;
        [SerializeField] private bool _animatePressScale = true;

        public CustomButtonSettings Settings
        {
            get => _settings;
            set => _settings = value;
        }

        public float HoverDistance
        {
            get => _overrideHoverDistance ? Mathf.Max(0.001f, _hoverDistance) : ResolvedSettings.HoverDistance;
            set
            {
                _overrideHoverDistance = true;
                _hoverDistance = Mathf.Max(0.001f, value);
            }
        }

        public Gradient HoverGradient
        {
            get => _overrideHoverGradient && _hoverGradient != null ? _hoverGradient : ResolvedSettings.HoverGradient;
            set
            {
                _overrideHoverGradient = true;
                _hoverGradient = value ?? CustomButtonSettings.CreateDefaultHoverGradient();
            }
        }

        private CustomButtonSettings ResolvedSettings => _settings != null ? _settings : CustomButtonSettings.Global;
        internal CustomPressableSurface PressableSurface => _pressableSurface;
        internal Graphic TargetGraphic => _targetGraphic;
        internal RectTransform AnimationTarget => _animationTarget;
        internal bool BlocksCompetingTransientOwner =>
                _pointerDown ||
                _pressableSurface != null &&
                (_pressableSurface.IsTargeted || _pressableSurface.TargetPressDepth01 > 0f);

        public void SetBaseInteractionColor(Color baseColor,
                                            float selectedBoost = 1f,
                                            bool useBaseInteractionMultipliers = false)
        {
            _baseInteractionColor = baseColor;
            _baseInteractionSelectedBoost = Mathf.Max(0f, selectedBoost);
            _useBaseInteractionMultipliers = useBaseInteractionMultipliers;
            _overrideBaseInteractionColor = true;
            ResolveBaseInteractionSemantic();
            RefreshVisualState();
        }

        internal bool HasBaseInteractionColor(Color baseColor,
                                              float selectedBoost = 1f,
                                              bool useBaseInteractionMultipliers = false)
        {
            return _overrideBaseInteractionColor &&
                   AreColorsApproximatelyEqual(_baseInteractionColor, baseColor) &&
                   Mathf.Approximately(_baseInteractionSelectedBoost, Mathf.Max(0f, selectedBoost)) &&
                   _useBaseInteractionMultipliers == useBaseInteractionMultipliers;
        }

        public void ClearBaseInteractionColor()
        {
            if (!_overrideBaseInteractionColor)
            {
                return;
            }

            _overrideBaseInteractionColor = false;
            _hasSemanticBaseInteractionColor = false;
            _useBaseInteractionMultipliers = false;
            _baseInteractionSelectedBoost = 1f;
            RefreshVisualState();
        }

        public void RefreshVisualState(bool instant = true)
        {
            EnsureReferences();
            RefreshSelectedState();
            UpdateTargetMultiplier();
            ApplyVisualState(instant);
        }

        public void RefreshPokeTargetBindings()
        {
            EnsureReferences();
            _pokeTracker.Bind(this);
        }

        private void Awake()
        {
            ResolveBaseInteractionSemantic();
            EnsureReferences();
            CacheBaseScale();
        }

        private void OnEnable()
        {
            ResolveBaseInteractionSemantic();
            EnsureReferences();
            CacheBaseScale();
            _pokeTracker.Bind(this);
            ApplyVisualState(true);
        }

        private void OnDisable()
        {
            _pokeTracker.ClearBindings();
            ClearPointerState();
            _selected = false;
            _currentMultiplier = 1f;
            _targetMultiplier = 1f;
            CustomSelectableTransientOwners.Unregister(this);
            ResetAnimatedScale();
        }

        private void LateUpdate()
        {
            EnsureReferences();
            ValidatePointerHover();
            RefreshSelectedState();
            UpdateTransientOwnerRegistration();
            UpdateTargetMultiplier();
            ApplyColorMultiplier();
            ApplyPressScale();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _hoverDistance = Mathf.Max(0.001f, _hoverDistance);
            _baseInteractionSelectedBoost = Mathf.Max(0f, _baseInteractionSelectedBoost);
            ResolveBaseInteractionSemantic();
            SanitizeHoverGradient();
            DisableUnityTransition(GetComponent<Selectable>(), true);
        }
#endif

        public void ApplySettingsChanged(CustomButtonSettings changedSettings)
        {
            if (!UsesSettings(changedSettings))
            {
                return;
            }

            EnsureReferences();
            UpdateTargetMultiplier();
            ApplyVisualState(true);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _pointerInside = true;
            UpdatePointerProximity(eventData);
            CustomSelectableTransientOwners.Register(this);
        }

        public void OnPointerExit(PointerEventData _)
        {
            _pointerInside = false;
            _pointerProximity = 0f;
            UpdateTransientOwnerRegistration();
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            UpdatePointerProximity(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _pointerDown = true;
            _pointerInside = true;
            _pointerProximity = 1f;
            CustomSelectableTransientOwners.Register(this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _pointerDown = false;
            UpdatePointerProximity(eventData);
            UpdateTransientOwnerRegistration();
        }

        public void OnSelect(BaseEventData _)
        {
            _selected = true;
        }

        public void OnDeselect(BaseEventData _)
        {
            _selected = false;
        }

        private void EnsureReferences()
        {
            if (_selectable == null)
            {
                _selectable = GetComponent<Selectable>();
            }

            DisableUnityTransition(_selectable, false);

            if (_pressableSurface == null)
            {
                _pressableSurface = GetComponent<CustomPressableSurface>();
            }

            if (_focusListener == null)
            {
                _focusListener = GetComponent<ButtonFocusListener>();
            }

            if (_selectableOverride is UnityEngine.Object unityObject && unityObject == null)
            {
                _selectableOverride = null;
            }

            if (_rectTransform == null)
            {
                _rectTransform = transform as RectTransform;
            }

            _targetGraphic = ResolveTargetGraphic();
            if (_targetGraphic == null)
            {
                _targetGraphic = GetComponent<Graphic>() ?? GetComponentInChildren<Graphic>(true);
            }
            ResolveLinkedColorTargets();

            _animationTarget = _animationTargetOverride != null
                                       ? _animationTargetOverride
                                       : _targetGraphic != null && _targetGraphic.transform is RectTransform targetRect
                                               ? targetRect
                                               : _rectTransform;
        }

        private void CacheBaseScale()
        {
            if (_animationTarget == null || _hasBaseScale)
            {
                return;
            }

            _baseScale = _animationTarget.localScale;
            _hasBaseScale = true;
        }

        private Graphic ResolveTargetGraphic()
        {
            if (_graphicOverride != null)
            {
                return _graphicOverride;
            }

            if (!ShouldPreserveSelectableTargetGraphic(_selectable))
            {
                Graphic pressableBackground = ResolvePressableBackgroundGraphic();
                if (pressableBackground != null)
                {
                    RetargetSelectableGraphic(pressableBackground);
                    return pressableBackground;
                }
            }

            return _selectable != null ? _selectable.targetGraphic : null;
        }

        private Graphic ResolvePressableBackgroundGraphic()
        {
            RectTransform visualRoot = _pressableSurface != null ? _pressableSurface.PressableVisualRoot : null;
            if (visualRoot == null)
            {
                return null;
            }

            Transform background = visualRoot.Find(CustomPressableSurface.MovingRootGraphicName);
            return background != null && background.TryGetComponent(out Graphic graphic) ? graphic : null;
        }

        private void RetargetSelectableGraphic(Graphic target)
        {
            if (_selectable == null ||
                target == null ||
                _selectable.targetGraphic == target ||
                ShouldPreserveSelectableTargetGraphic(_selectable) ||
                !CanRetargetSelectableGraphic())
            {
                return;
            }

            _selectable.targetGraphic = target;
        }

        private static bool ShouldPreserveSelectableTargetGraphic(Selectable selectable)
        {
            return selectable is Slider || selectable is UnityEngine.UI.Scrollbar;
        }

        private void ResolveLinkedColorTargets()
        {
            _linkedColorTargets.Clear();

            if (_selectable is not Slider slider ||
                slider.GetComponent<SliderToggle>() != null)
            {
                return;
            }

            AddLinkedSliderColorTarget(slider.fillRect);
            AddLinkedSliderColorTarget(slider.fillRect != null ? slider.fillRect.parent as RectTransform : null);
            AddLinkedSliderColorTarget(slider.handleRect != null ? slider.handleRect.parent as RectTransform : null);
        }

        private void AddLinkedSliderColorTarget(RectTransform rectTransform)
        {
            if (rectTransform == null ||
                !rectTransform.TryGetComponent(out Graphic graphic) ||
                graphic == null ||
                graphic == _targetGraphic ||
                _linkedColorTargets.Contains(graphic))
            {
                return;
            }

            _linkedColorTargets.Add(graphic);
        }

        private bool CanRetargetSelectableGraphic()
        {
#if UNITY_EDITOR
            return Application.isPlaying || !EditorUtility.IsPersistent(gameObject);
#else
            return true;
#endif
        }

        private void ResetAnimatedScale()
        {
            if (_animationTarget != null && _hasBaseScale)
            {
                _animationTarget.localScale = _baseScale;
            }
        }

        private void ClearPointerState()
        {
            _pointerInside = false;
            _pointerDown = false;
            _pointerProximity = 0f;
        }

        internal bool IsOwnedByNestedSelectable(Transform candidate)
        {
            Transform current = candidate;
            while (current != null && current != transform)
            {
                if (current.TryGetComponent(out Selectable selectable) && selectable != _selectable)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        internal void RegisterTransientOwner()
        {
            CustomSelectableTransientOwners.Register(this);
        }

        internal void ClearTransientStateFromCompetingOwner()
        {
            bool keepActivePress = BlocksCompetingTransientOwner;
            ClearPointerState();
            _pokeTracker.ClearProximity();
            _targetMultiplier = 1f;
            _currentMultiplier = 1f;
            CustomSelectableTransientOwners.Unregister(this);
            if (!keepActivePress)
            {
                _pressableSurface?.ForceRelease();
            }

            ApplyVisualState(true);
        }

        private void UpdateTransientOwnerRegistration()
        {
            if (HasTransientHoverState())
            {
                CustomSelectableTransientOwners.Register(this);
                return;
            }

            CustomSelectableTransientOwners.Unregister(this);
        }

        private bool HasTransientHoverState()
        {
            return _pointerInside ||
                   _pointerDown ||
                   _pokeTracker.IsInRange ||
                   _pressableSurface != null &&
                   (_pressableSurface.IsTargeted || _pressableSurface.TargetPressDepth01 > 0f);
        }

        private static void DisableUnityTransition(Selectable selectable, bool markDirty)
        {
            if (selectable == null || selectable.transition == Selectable.Transition.None)
            {
                return;
            }

            selectable.transition = Selectable.Transition.None;
#if UNITY_EDITOR
            if (markDirty)
            {
                EditorUtility.SetDirty(selectable);
            }
#endif
        }

        private void SanitizeHoverGradient()
        {
            if (_hoverGradient == null)
            {
                _hoverGradient = CustomButtonSettings.CreateDefaultHoverGradient();
                return;
            }

            GradientColorKey[] keys = _hoverGradient.colorKeys;
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i].color = CustomButtonSettings.AsWhiteBlackMultiplier(keys[i].color);
            }

            _hoverGradient.SetKeys(keys, _hoverGradient.alphaKeys);
        }

        private void ValidatePointerHover()
        {
#if ENABLE_INPUT_SYSTEM
            if (!_pointerInside || _pointerDown || _rectTransform == null || Mouse.current == null)
            {
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            UnityEngine.Camera camera = ResolveEventCamera(canvas);
            if (ContainsScreenPointWithPadding(_rectTransform, ResolveRaycastPadding(), Mouse.current.position.ReadValue(), camera))
            {
                return;
            }

            _pointerInside = false;
            _pointerProximity = 0f;
#endif
        }

        private void UpdatePointerProximity(PointerEventData eventData)
        {
            if (!_pointerInside || _rectTransform == null || eventData == null)
            {
                _pointerProximity = 0f;
                return;
            }

            UnityEngine.Camera eventCamera = eventData.pressEventCamera ?? eventData.enterEventCamera;
            if (eventCamera == null)
            {
                eventCamera = ResolveEventCamera(GetComponentInParent<Canvas>());
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform,
                                                                         eventData.position,
                                                                         eventCamera,
                                                                         out Vector2 localPoint))
            {
                _pointerProximity = 1f;
                return;
            }

            Rect rect = ExpandRect(_rectTransform.rect, ResolveRaycastPadding());
            float dx = Mathf.Max(Mathf.Max(rect.xMin - localPoint.x, 0f), localPoint.x - rect.xMax);
            float dy = Mathf.Max(Mathf.Max(rect.yMin - localPoint.y, 0f), localPoint.y - rect.yMax);
            float distance = Mathf.Sqrt(dx * dx + dy * dy);
            _pointerProximity = Mathf.Clamp01(1f - distance / HoverDistance);
        }

        private static UnityEngine.Camera ResolveEventCamera(Canvas canvas)
        {
            if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }

            return WorldCanvasEventCameraAssigner.ResolveEventCamera(canvas);
        }

        private void UpdateTargetMultiplier()
        {
            if (!ColorPalette.Palette.UseInteractionStateMultipliers)
            {
                _targetMultiplier = 1f;
                return;
            }

            if (!IsInteractableForFeedback())
            {
                _targetMultiplier = 1f;
                return;
            }

            float pressDepth = _pressableSurface != null
                                       ? _pressableSurface.TargetPressDepth01
                                       : 0f;
            float proximity = Mathf.Max(_pointerInside ? _pointerProximity : 0f,
                                        _pokeTracker.IsInRange ? _pokeTracker.Proximity : 0f,
                                        pressDepth);
            if (_pointerDown && _pressableSurface == null)
            {
                proximity = 1f;
            }

            if (proximity <= 0f)
            {
                _targetMultiplier = 1f;
                return;
            }

            Color gradientColor = CustomButtonSettings.AsWhiteBlackMultiplier(HoverGradient.Evaluate(proximity));
            float sensitivity = ResolvedSettings.HoverSensitivity;
            _targetMultiplier = Mathf.Lerp(1f, gradientColor.grayscale, Mathf.Clamp01(proximity * sensitivity));
        }

        private void RefreshSelectedState()
        {
            if (_focusListener != null)
            {
                _selected = _focusListener.IsSelected;
                return;
            }

            if (_selectable is TMP_InputField inputField)
            {
                EventSystem eventSystem = EventSystem.current;
                _selected = inputField.isFocused ||
                            eventSystem != null && eventSystem.currentSelectedGameObject == gameObject;
                return;
            }

            _selected = false;
        }

        private void ApplyColorMultiplier()
        {
            if (_selectable == null || _targetGraphic == null)
            {
                return;
            }

            bool interactableForFeedback = IsInteractableForFeedback();
            bool visuallyInteractable = IsVisuallyInteractableForFeedback();
            if (!interactableForFeedback || !visuallyInteractable)
            {
                _targetMultiplier = 1f;
                _currentMultiplier = 1f;
            }

            float smoothSpeed = interactableForFeedback && visuallyInteractable ? ResolvedSettings.ColorSmoothSpeed : 0f;
            _currentMultiplier = smoothSpeed <= 0f
                                         ? _targetMultiplier
                                         : Mathf.Lerp(_currentMultiplier,
                                                      _targetMultiplier,
                                                      Mathf.Clamp01(Time.unscaledDeltaTime * smoothSpeed));

            EnsureNeutralColorTargetImages();

            Color stateColor = ResolveSelectableColor();
            Color targetColor = ColorPalette.Palette.UseInteractionStateMultipliers
                                        ? stateColor * _currentMultiplier
                                        : stateColor;
            targetColor.a = stateColor.a;
            ApplyColorToTargets(targetColor);
        }

        private void ApplyColorToTargets(Color targetColor)
        {
            _targetGraphic.CrossFadeColor(targetColor, 0f, true, true);

            for (int i = _linkedColorTargets.Count - 1; i >= 0; i--)
            {
                Graphic linkedTarget = _linkedColorTargets[i];
                if (linkedTarget == null)
                {
                    _linkedColorTargets.RemoveAt(i);
                    continue;
                }

                linkedTarget.CrossFadeColor(targetColor, 0f, true, true);
            }
        }

        private void ApplyPressScale()
        {
            if (!_animatePressScale || _animationTarget == null || _pressableSurface != null)
            {
                return;
            }

            CacheBaseScale();

            float pressedScale = _pointerDown ? ResolvedSettings.PressedScale : 1f;
            Vector3 targetScale = _baseScale * pressedScale;
            float smoothSpeed = ResolvedSettings.PressSmoothSpeed;

            _animationTarget.localScale = smoothSpeed <= 0f
                                                  ? targetScale
                                                  : Vector3.Lerp(_animationTarget.localScale,
                                                                 targetScale,
                                                                 Mathf.Clamp01(Time.unscaledDeltaTime * smoothSpeed));
        }

        private Color ResolveSelectableColor()
        {
            if (_selectable == null)
            {
                return Color.white;
            }

            ColorBlock colors = _selectable.colors;
            CustomButtonVisualState state = ResolveVisualState();
            if (state == CustomButtonVisualState.Disabled)
            {
                return ColorPalette.DisabledColor;
            }

            Color stateColor = _overrideBaseInteractionColor
                                       ? ResolveBaseInteractionColor(state)
                                       : ResolvedSettings.ResolveInteractionColor(colors, state, false);
            return stateColor;
        }

        private Color ResolveBaseInteractionColor(CustomButtonVisualState state)
        {
            Color baseColor = ResolveCurrentBaseInteractionColor();
            if (_useBaseInteractionMultipliers)
            {
                return ResolveBaseMultiplierInteractionColor(baseColor, state);
            }

            return state switch
            {
                    CustomButtonVisualState.Highlighted => ColorPalette.HighlightedColor,
                    CustomButtonVisualState.Pressed => ColorPalette.PressedColor,
                    CustomButtonVisualState.Selected => UiButtonTint.Tint(ColorPalette.SelectedColor,
                                                                          ResolveSelectedBoost()),
                    CustomButtonVisualState.Disabled => ColorPalette.DisabledColor,
                    _ => baseColor,
            };
        }

        private Color ResolveBaseMultiplierInteractionColor(Color baseColor, CustomButtonVisualState state)
        {
            XrUiColorPalette palette = ColorPalette.Palette;
            if (state == CustomButtonVisualState.Disabled)
            {
                return ColorPalette.DisabledColor;
            }

            Color color = UiButtonTint.Tint(baseColor, palette.GetInteractionMultiplier(state));
            return state == CustomButtonVisualState.Selected
                           ? UiButtonTint.Tint(color, ResolveSelectedBoost())
                           : color;
        }

        private Color ResolveCurrentBaseInteractionColor()
        {
            return _hasSemanticBaseInteractionColor
                           ? ColorPalette.Palette.GetSemanticColor(_baseInteractionSemanticColor)
                           : _baseInteractionColor;
        }

        private void ResolveBaseInteractionSemantic()
        {
            _hasSemanticBaseInteractionColor = _overrideBaseInteractionColor &&
                                               XrUiColorPalette.TryResolveSemanticColor(_baseInteractionColor,
                                                                                            out _baseInteractionSemanticColor);
        }

        private float ResolveSelectedBoost()
        {
            return ColorPalette.Palette.UseInteractionStateMultipliers ? _baseInteractionSelectedBoost : 1f;
        }

        private void EnsureNeutralColorTargetImages()
        {
            EnsureNeutralColorTargetImage(_targetGraphic);

            for (int i = _linkedColorTargets.Count - 1; i >= 0; i--)
            {
                Graphic linkedTarget = _linkedColorTargets[i];
                if (linkedTarget == null)
                {
                    _linkedColorTargets.RemoveAt(i);
                    continue;
                }

                EnsureNeutralColorTargetImage(linkedTarget);
            }
        }

        private void EnsureNeutralColorTargetImage(Graphic graphic)
        {
            if (graphic == null || !ShouldKeepGraphicColorNeutral(graphic))
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying && EditorUtility.IsPersistent(graphic))
            {
                return;
            }
#endif

            if (graphic.color != Color.white)
            {
                graphic.color = Color.white;
            }
        }

        private bool ShouldKeepGraphicColorNeutral(Graphic graphic)
        {
            return IsPressableBackgroundTarget(graphic) || IsLinkedSliderColorTarget(graphic);
        }

        private bool IsLinkedSliderColorTarget(Graphic graphic)
        {
            return graphic != null &&
                   _selectable is Slider &&
                   (graphic == _targetGraphic || _linkedColorTargets.Contains(graphic));
        }

        private static bool AreColorsApproximatelyEqual(Color a, Color b)
        {
            const float tolerance = 0.001f;
            return Mathf.Abs(a.r - b.r) <= tolerance &&
                   Mathf.Abs(a.g - b.g) <= tolerance &&
                   Mathf.Abs(a.b - b.b) <= tolerance &&
                   Mathf.Abs(a.a - b.a) <= tolerance;
        }

        private bool IsPressableBackgroundTarget(Graphic graphic)
        {
            RectTransform visualRoot = _pressableSurface != null ? _pressableSurface.PressableVisualRoot : null;
            return graphic != null &&
                   visualRoot != null &&
                   graphic.transform.parent == visualRoot &&
                   graphic.transform.name == CustomPressableSurface.MovingRootGraphicName;
        }

        private CustomButtonVisualState ResolveVisualState()
        {
            bool surfaceActivated = _pressableSurface != null && _pressableSurface.IsActivated;
            bool surfaceTargeted = _pressableSurface != null &&
                                   (_pressableSurface.IsTargeted || _pressableSurface.TargetPressDepth01 > 0f);

            if (!IsVisuallyInteractableForFeedback())
            {
                return CustomButtonVisualState.Disabled;
            }

            if (surfaceActivated || (_pointerDown && _pressableSurface == null))
            {
                return CustomButtonVisualState.Pressed;
            }

            if (_selected)
            {
                return CustomButtonVisualState.Selected;
            }

            if (_pointerInside || _pokeTracker.IsInRange || surfaceTargeted)
            {
                return CustomButtonVisualState.Highlighted;
            }

            return CustomButtonVisualState.Normal;
        }

        private bool IsInteractableForFeedback()
        {
            return CustomPressableSelectableUtility.IsInteractableForCustomFeedback(_selectable,
                                                                                   this,
                                                                                   ref _selectableOverride);
        }

        private bool IsVisuallyInteractableForFeedback()
        {
            return CustomPressableSelectableUtility.IsVisuallyInteractableForCustomFeedback(_selectable,
                                                                                           this,
                                                                                           ref _selectableOverride);
        }

        private Vector4 ResolveRaycastPadding()
        {
            return _pressableSurface != null ? _pressableSurface.ResolvedHitRaycastPadding : Vector4.zero;
        }

        private static bool ContainsScreenPointWithPadding(RectTransform rectTransform,
                                                           Vector4 padding,
                                                           Vector2 screenPoint,
                                                           UnityEngine.Camera camera)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, camera, out Vector2 localPoint))
            {
                return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, camera);
            }

            return ExpandRect(rectTransform.rect, padding).Contains(localPoint);
        }

        private static Rect ExpandRect(Rect rect, Vector4 padding)
        {
            return new Rect(rect.xMin - padding.x,
                            rect.yMin - padding.y,
                            rect.width + padding.x + padding.z,
                            rect.height + padding.y + padding.w);
        }

        private void ApplyVisualState(bool instant)
        {
            _currentMultiplier = instant ? _targetMultiplier : _currentMultiplier;
            ApplyColorMultiplier();
            ApplyPressScale();
        }

        private bool UsesSettings(CustomButtonSettings changedSettings)
        {
            if (changedSettings == null)
            {
                return true;
            }

            if (_settings != null)
            {
                return _settings == changedSettings;
            }

            return changedSettings == CustomButtonSettings.Global;
        }
    }
}
