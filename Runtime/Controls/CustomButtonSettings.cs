using System;
using Deucarian.XRUI;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Deucarian.XRUI.Controls
{
    public enum CustomButtonVisualState
    {
        Normal,
        Highlighted,
        Pressed,
        Selected,
        Disabled,
    }

    public enum CustomPressableVisualStyle
    {
        GenericButton,
        KeyboardKey,
        ItemRow,
        InputField,
        SliderHandle,
        SlideToggle,
        ScrollbarSlider,
    }

    [Serializable]
    public sealed class CustomPressableVisualStyleProfile
    {
        [SerializeField] private Sprite _pressableBackgroundSprite;
        [SerializeField] [Min(0.001f)] private float _pressableBackgroundPixelsPerUnitMultiplier = 1f;
        [SerializeField] [InspectorName("Fill Center")] private bool _pressableBackgroundFillCenter;
        [SerializeField] private Sprite _outlineSprite;
        [SerializeField] [Min(0.001f)] private float _outlinePixelsPerUnitMultiplier = 1f;
        [SerializeField] [InspectorName("Fill Center")] private bool _outlineFillCenter;
        [SerializeField] private Sprite _socketGhostSprite;
        [SerializeField] [Min(0.001f)] private float _socketGhostPixelsPerUnitMultiplier = 1f;
        [SerializeField] [InspectorName("Fill Center")] private bool _socketGhostFillCenter;

        public Sprite PressableBackgroundSprite => _pressableBackgroundSprite;
        public float PressableBackgroundPixelsPerUnitMultiplier => Mathf.Max(0.001f, _pressableBackgroundPixelsPerUnitMultiplier);
        public bool PressableBackgroundFillCenter => _pressableBackgroundFillCenter;
        public Sprite OutlineSprite => _outlineSprite;
        public float OutlinePixelsPerUnitMultiplier => Mathf.Max(0.001f, _outlinePixelsPerUnitMultiplier);
        public bool OutlineFillCenter => _outlineFillCenter;
        public Sprite SocketGhostSprite => _socketGhostSprite;
        public float SocketGhostPixelsPerUnitMultiplier => Mathf.Max(0.001f, _socketGhostPixelsPerUnitMultiplier);
        public bool SocketGhostFillCenter => _socketGhostFillCenter;

        internal void Sanitize()
        {
            _pressableBackgroundPixelsPerUnitMultiplier = Mathf.Max(0.001f, _pressableBackgroundPixelsPerUnitMultiplier);
            _outlinePixelsPerUnitMultiplier = Mathf.Max(0.001f, _outlinePixelsPerUnitMultiplier);
            _socketGhostPixelsPerUnitMultiplier = Mathf.Max(0.001f, _socketGhostPixelsPerUnitMultiplier);
        }
    }

    [CreateAssetMenu(fileName = "CustomButtonSettings", menuName = "Deucarian/XR UI/Custom Button Settings")]
    public sealed class CustomButtonSettings : ScriptableObject
    {
        #region Constants and Fields
        private const string RESOURCE_PATH = "CustomButtonSettings";
        private const float DEFAULT_HOVER_DISTANCE = 20f;
        private const float DEFAULT_HOVER_SENSITIVITY = 1f;
        private const float DEFAULT_COLOR_SMOOTH_SPEED = 18f;
        private const float DEFAULT_PRESSED_SCALE = 0.96f;
        private const float DEFAULT_PRESS_SMOOTH_SPEED = 20f;
        private const float DEFAULT_PRESS_DEPTH_DISTANCE = 10f;
        private const float DEFAULT_ACTIVATION_DEPTH = 1f;
        private const float DEFAULT_RELEASE_DEPTH = 0.05f;
        private const float DEFAULT_SOCKET_GHOST_ALPHA = 0.22f;
        private static readonly Vector4 DEFAULT_RAYCAST_PADDING = Vector4.zero;
        private static readonly Color DEFAULT_SOCKET_GHOST_COLOR_MULTIPLIER = Color.white;
        private const bool DEFAULT_USE_PALETTE_SOCKET_GHOST_TINT = true;
        private const bool DEFAULT_EDITOR_MOUSE_FALLBACK_ENABLED = true;
        private const bool DEFAULT_MOUSE_FALLBACK_USE_HOLD_RAMP = true;
        private const float DEFAULT_MOUSE_FALLBACK_HOLD_RAMP_SECONDS = 0.18f;
        private static readonly Vector4 DEFAULT_MOUSE_FALLBACK_RAYCAST_PADDING = new(10f, 10f, 10f, 10f);

        private static CustomButtonSettings _runtimeFallback;
        private static CustomButtonSettings _resourcesSettings;
#if UNITY_EDITOR
        private static bool _settingsChangedQueued;
#endif
        #endregion

        #region Public Events
        public static event Action<CustomButtonSettings> SettingsChanged;
        #endregion

        #region Serialized Fields
        [Header("Global Parameters")]
        [SerializeField] [Min(0.001f)] private float _hoverDistance = DEFAULT_HOVER_DISTANCE;
        [SerializeField] [Range(0f, 2f)] private float _hoverSensitivity = DEFAULT_HOVER_SENSITIVITY;
        [SerializeField] private Gradient _hoverGradient = CreateDefaultHoverGradient();

        [Header("Palette Tints")]
        [SerializeField] private bool _usePaletteInteractionTints = true;
        [SerializeField] private bool _usePaletteSocketGhostTint = DEFAULT_USE_PALETTE_SOCKET_GHOST_TINT;
        [SerializeField] private Color _socketGhostColorMultiplier = DEFAULT_SOCKET_GHOST_COLOR_MULTIPLIER;

        [Header("Visual Styles")]
        [SerializeField] private CustomPressableVisualStyleProfile _genericButtonStyle = new();
        [SerializeField] private CustomPressableVisualStyleProfile _keyboardKeyStyle = new();
        [SerializeField] private CustomPressableVisualStyleProfile _itemRowStyle = new();
        [SerializeField] private CustomPressableVisualStyleProfile _inputFieldStyle = new();
        [SerializeField] private CustomPressableVisualStyleProfile _sliderHandleStyle = new();
        [SerializeField] private CustomPressableVisualStyleProfile _slideToggleStyle = new();
        [SerializeField] private CustomPressableVisualStyleProfile _scrollbarSliderStyle = new();

        [Header("Motion")]
        [SerializeField] [Range(0.85f, 1f)] private float _pressedScale = DEFAULT_PRESSED_SCALE;
        [SerializeField] [Min(0f)] private float _pressSmoothSpeed = DEFAULT_PRESS_SMOOTH_SPEED;
        [SerializeField] [Min(0f)] private float _colorSmoothSpeed = DEFAULT_COLOR_SMOOTH_SPEED;

        [Header("Physical Press")]
        [SerializeField] [Min(0.001f)] private float _pressDepthDistance = DEFAULT_PRESS_DEPTH_DISTANCE;
        [SerializeField] [Range(0f, 1f)] private float _activationDepth = DEFAULT_ACTIVATION_DEPTH;
        [SerializeField] [Range(0f, 1f)] private float _releaseDepth = DEFAULT_RELEASE_DEPTH;
        [SerializeField] [Range(0f, 1f)] private float _socketGhostAlpha = DEFAULT_SOCKET_GHOST_ALPHA;
        [SerializeField] private Vector4 _raycastPadding = DEFAULT_RAYCAST_PADDING;

        [Header("Development Mouse Fallback")]
        [SerializeField] private bool _editorMouseFallbackEnabled = DEFAULT_EDITOR_MOUSE_FALLBACK_ENABLED;
        [SerializeField] private bool _mouseFallbackUseHoldRamp = DEFAULT_MOUSE_FALLBACK_USE_HOLD_RAMP;
        [SerializeField] [Min(0f)] private float _mouseFallbackHoldRampSeconds = DEFAULT_MOUSE_FALLBACK_HOLD_RAMP_SECONDS;
        [SerializeField] private Vector4 _mouseFallbackRaycastPadding = DEFAULT_MOUSE_FALLBACK_RAYCAST_PADDING;
        #endregion

        #region Public Properties
        public float HoverDistance => Mathf.Max(0.001f, _hoverDistance);
        public float HoverSensitivity => Mathf.Max(0f, _hoverSensitivity);
        public Gradient HoverGradient => _hoverGradient ?? CreateDefaultHoverGradient();
        public bool UsePaletteInteractionTints => _usePaletteInteractionTints;
        public bool UsePaletteSocketGhostTint => _usePaletteSocketGhostTint;
        public Color SocketGhostColorMultiplier => _socketGhostColorMultiplier;
        public Sprite SocketGhostSprite => ResolveVisualStyle(CustomPressableVisualStyle.GenericButton).SocketGhostSprite;
        public float SocketGhostPixelsPerUnitMultiplier => ResolveVisualStyle(CustomPressableVisualStyle.GenericButton).SocketGhostPixelsPerUnitMultiplier;
        public float PressedScale => Mathf.Clamp(_pressedScale, 0.85f, 1f);
        public float PressSmoothSpeed => Mathf.Max(0f, _pressSmoothSpeed);
        public float ColorSmoothSpeed => Mathf.Max(0f, _colorSmoothSpeed);
        public float PressDepthDistance => Mathf.Max(0.001f, _pressDepthDistance);
        public float ActivationDepth => Mathf.Clamp01(_activationDepth);
        public float ReleaseDepth => Mathf.Min(Mathf.Clamp01(_releaseDepth), ActivationDepth);
        public float SocketGhostAlpha => Mathf.Clamp01(_socketGhostAlpha);
        public Vector4 RaycastPadding => ClampNonNegative(_raycastPadding);
        public bool EditorMouseFallbackEnabled => _editorMouseFallbackEnabled;
        public bool MouseFallbackUseHoldRamp => _mouseFallbackUseHoldRamp;
        public float MouseFallbackHoldRampSeconds => Mathf.Max(0f, _mouseFallbackHoldRampSeconds);
        public Vector4 MouseFallbackRaycastPadding => ClampNonNegative(_mouseFallbackRaycastPadding);

        public static CustomButtonSettings Global
        {
            get
            {
                if (_resourcesSettings == null)
                {
                    _resourcesSettings = Resources.Load<CustomButtonSettings>(RESOURCE_PATH);
                }

                if (_resourcesSettings != null)
                {
                    return _resourcesSettings;
                }

                if (_runtimeFallback == null)
                {
                    _runtimeFallback = CreateInstance<CustomButtonSettings>();
                    _runtimeFallback.hideFlags = HideFlags.HideAndDontSave;
                }

                return _runtimeFallback;
            }
        }
        #endregion

        #region Public Methods
        public static void NotifyGlobalSettingsChanged() => Global.NotifySettingsChanged();

        public static Gradient CreateDefaultHoverGradient()
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                    new[]
                    {
                            new GradientColorKey(new Color(0.72f, 0.72f, 0.72f), 0f),
                            new GradientColorKey(new Color(0.9f, 0.9f, 0.9f), 0.55f),
                            new GradientColorKey(Color.white, 1f),
                    },
                    new[]
                    {
                            new GradientAlphaKey(1f, 0f),
                            new GradientAlphaKey(1f, 1f),
                    });

            return gradient;
        }

        public static Color AsWhiteBlackMultiplier(Color color)
        {
            float multiplier = Mathf.Clamp01(color.grayscale);
            return new Color(multiplier, multiplier, multiplier, color.a);
        }

        public Color ResolveInteractionColor(ColorBlock colors, CustomButtonVisualState state, bool preserveSelectableColors)
        {
            if (state == CustomButtonVisualState.Disabled)
            {
                return ColorPalette.DisabledColor;
            }

            if (!_usePaletteInteractionTints || preserveSelectableColors)
            {
                return state switch
                {
                        CustomButtonVisualState.Highlighted => colors.highlightedColor,
                        CustomButtonVisualState.Pressed => colors.pressedColor,
                        CustomButtonVisualState.Selected => colors.selectedColor,
                        _ => colors.normalColor,
                };
            }

            if (state == CustomButtonVisualState.Normal &&
                XrUiColorPalette.TryResolveSemanticColor(colors.normalColor, out XrUiSemanticColor semanticColor))
            {
                return ColorPalette.Palette.GetInteractionColor(state, semanticColor);
            }

            return ColorPalette.Palette.GetInteractionColor(state);
        }

        public Color ResolveSocketGhostColor(Color baseColor)
        {
            return ResolveSocketGhostColor();
        }

        public Color ResolveSocketGhostColor()
        {
            Color color = ColorPalette.SocketGhostColor;
            if (!_usePaletteSocketGhostTint)
            {
                color = Multiply(color, _socketGhostColorMultiplier);
            }

            color.a *= SocketGhostAlpha;
            return color;
        }

        public CustomPressableVisualStyleProfile ResolveVisualStyle(CustomPressableVisualStyle style)
        {
            return style switch
            {
                    CustomPressableVisualStyle.KeyboardKey => ResolveVisualStyleProfile(_keyboardKeyStyle),
                    CustomPressableVisualStyle.ItemRow => ResolveVisualStyleProfile(_itemRowStyle),
                    CustomPressableVisualStyle.InputField => ResolveVisualStyleProfile(_inputFieldStyle),
                    CustomPressableVisualStyle.SliderHandle => ResolveVisualStyleProfile(_sliderHandleStyle),
                    CustomPressableVisualStyle.SlideToggle => ResolveVisualStyleProfile(_slideToggleStyle),
                    CustomPressableVisualStyle.ScrollbarSlider => ResolveVisualStyleProfile(_scrollbarSliderStyle),
                    _ => ResolveVisualStyleProfile(_genericButtonStyle),
            };
        }
        #endregion

        #region Unity Methods
        private void OnValidate()
        {
            _hoverDistance = Mathf.Max(0.001f, _hoverDistance);
            _hoverSensitivity = Mathf.Max(0f, _hoverSensitivity);
            _pressedScale = Mathf.Clamp(_pressedScale, 0.85f, 1f);
            _pressSmoothSpeed = Mathf.Max(0f, _pressSmoothSpeed);
            _colorSmoothSpeed = Mathf.Max(0f, _colorSmoothSpeed);
            _pressDepthDistance = Mathf.Max(0.001f, _pressDepthDistance);
            _activationDepth = Mathf.Clamp01(_activationDepth);
            _releaseDepth = Mathf.Min(Mathf.Clamp01(_releaseDepth), _activationDepth);
            _socketGhostAlpha = Mathf.Clamp01(_socketGhostAlpha);
            _raycastPadding = ClampNonNegative(_raycastPadding);
            _mouseFallbackRaycastPadding = ClampNonNegative(_mouseFallbackRaycastPadding);
            _socketGhostColorMultiplier.r = Mathf.Max(0f, _socketGhostColorMultiplier.r);
            _socketGhostColorMultiplier.g = Mathf.Max(0f, _socketGhostColorMultiplier.g);
            _socketGhostColorMultiplier.b = Mathf.Max(0f, _socketGhostColorMultiplier.b);
            _socketGhostColorMultiplier.a = Mathf.Clamp01(_socketGhostColorMultiplier.a);
            SanitizeVisualStyles();
            _mouseFallbackHoldRampSeconds = Mathf.Max(0f, _mouseFallbackHoldRampSeconds);
            SanitizeGradient();

#if UNITY_EDITOR
            QueueSettingsChanged();
#else
            NotifySettingsChanged();
#endif
        }
        #endregion

        #region Private Methods
#if UNITY_EDITOR
        private void QueueSettingsChanged()
        {
            if (_settingsChangedQueued)
            {
                return;
            }

            _settingsChangedQueued = true;
            EditorApplication.delayCall += () =>
            {
                _settingsChangedQueued = false;
                if (this == null)
                {
                    return;
                }

                if (EditorApplication.isUpdating || EditorApplication.isCompiling)
                {
                    QueueSettingsChanged();
                    return;
                }

                NotifySettingsChanged();
            };
        }
#endif

        private void SanitizeGradient()
        {
            if (_hoverGradient == null)
            {
                _hoverGradient = CreateDefaultHoverGradient();
                return;
            }

            GradientColorKey[] colorKeys = _hoverGradient.colorKeys;
            for (int i = 0; i < colorKeys.Length; i++)
            {
                colorKeys[i].color = AsWhiteBlackMultiplier(colorKeys[i].color);
            }

            _hoverGradient.SetKeys(colorKeys, _hoverGradient.alphaKeys);
        }

        private void SanitizeVisualStyles()
        {
            _genericButtonStyle ??= new CustomPressableVisualStyleProfile();
            _keyboardKeyStyle ??= new CustomPressableVisualStyleProfile();
            _itemRowStyle ??= new CustomPressableVisualStyleProfile();
            _inputFieldStyle ??= new CustomPressableVisualStyleProfile();
            _sliderHandleStyle ??= new CustomPressableVisualStyleProfile();
            _slideToggleStyle ??= new CustomPressableVisualStyleProfile();
            _scrollbarSliderStyle ??= new CustomPressableVisualStyleProfile();

            _genericButtonStyle.Sanitize();
            _keyboardKeyStyle.Sanitize();
            _itemRowStyle.Sanitize();
            _inputFieldStyle.Sanitize();
            _sliderHandleStyle.Sanitize();
            _slideToggleStyle.Sanitize();
            _scrollbarSliderStyle.Sanitize();
        }

        private CustomPressableVisualStyleProfile ResolveVisualStyleProfile(CustomPressableVisualStyleProfile profile)
        {
            if (profile != null)
            {
                return profile;
            }

            _genericButtonStyle ??= new CustomPressableVisualStyleProfile();
            return _genericButtonStyle;
        }

        private static Color Multiply(Color a, Color b) => new(a.r * b.r,
                                                               a.g * b.g,
                                                               a.b * b.b,
                                                               a.a * b.a);

        internal static Vector4 ClampNonNegative(Vector4 value)
        {
            return new Vector4(Mathf.Max(0f, value.x),
                               Mathf.Max(0f, value.y),
                               Mathf.Max(0f, value.z),
                               Mathf.Max(0f, value.w));
        }

        internal Vector4 ResolveRaycastPadding(bool includeMouseFallback)
        {
            Vector4 basePadding = RaycastPadding;
            return includeMouseFallback ? Max(basePadding, MouseFallbackRaycastPadding) : basePadding;
        }

        private static Vector4 Max(Vector4 a, Vector4 b)
        {
            return new Vector4(Mathf.Max(a.x, b.x),
                               Mathf.Max(a.y, b.y),
                               Mathf.Max(a.z, b.z),
                               Mathf.Max(a.w, b.w));
        }

        internal static Vector4 ToUnityRaycastPadding(Vector4 outwardPadding)
        {
            Vector4 clamped = ClampNonNegative(outwardPadding);
            return new Vector4(-clamped.x, -clamped.y, -clamped.z, -clamped.w);
        }

        private void NotifySettingsChanged()
        {
            SettingsChanged?.Invoke(this);

            CustomPressableSurface[] pressableSurfaces = Resources.FindObjectsOfTypeAll<CustomPressableSurface>();
            foreach (CustomPressableSurface pressableSurface in pressableSurfaces)
            {
                if (pressableSurface == null || !ShouldApplyToObject(pressableSurface))
                {
                    continue;
                }

                pressableSurface.ApplySettingsChanged(this);
            }

            CustomSelectableFeedback[] feedbacks = Resources.FindObjectsOfTypeAll<CustomSelectableFeedback>();
            foreach (CustomSelectableFeedback feedback in feedbacks)
            {
                if (feedback == null || !ShouldApplyToObject(feedback))
                {
                    continue;
                }

                feedback.ApplySettingsChanged(this);
            }

#if UNITY_EDITOR
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
#endif
        }

        private static bool ShouldApplyToObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return false;
            }

#if UNITY_EDITOR
            if (EditorUtility.IsPersistent(target))
            {
                return false;
            }

            if (target is Component component)
            {
                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                return component.gameObject.scene.IsValid() ||
                       prefabStage != null &&
                       prefabStage.prefabContentsRoot != null &&
                       component.transform.IsChildOf(prefabStage.prefabContentsRoot.transform);
            }
#endif

            return true;
        }
        #endregion
    }

    public static class CustomPressableSocketVisual
    {
        public static Color ResolveColor(CustomButtonSettings settings)
        {
            return (settings != null ? settings : CustomButtonSettings.Global).ResolveSocketGhostColor();
        }

        public static Color ResolveColor(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            return ResolveColor(settings);
        }

        public static Color ResolveImageColor(CustomButtonSettings settings)
        {
            return ResolveImageColor(settings, CustomPressableVisualStyle.GenericButton);
        }

        public static Color ResolveImageColor(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            Color color = resolvedSettings.ResolveSocketGhostColor();
            if (ResolveSocketSprite(resolvedSettings, style) == null)
            {
                color.a = 0f;
            }

            return color;
        }

        public static Sprite ResolveSprite(CustomButtonSettings settings)
        {
            return ResolveSocketSprite(settings, CustomPressableVisualStyle.GenericButton);
        }

        public static Sprite ResolveSocketSprite(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            return resolvedSettings.ResolveVisualStyle(style).SocketGhostSprite;
        }

        public static Sprite ResolvePressableBackgroundSprite(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            return resolvedSettings.ResolveVisualStyle(style).PressableBackgroundSprite;
        }

        public static Sprite ResolveOutlineSprite(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            return resolvedSettings.ResolveVisualStyle(style).OutlineSprite;
        }

        public static Image.Type ResolveImageType(CustomButtonSettings settings)
        {
            return ResolveSocketImageType(settings, CustomPressableVisualStyle.GenericButton);
        }

        public static Image.Type ResolveSocketImageType(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            if (ResolveSocketFillCenter(settings, style))
            {
                return Image.Type.Sliced;
            }

            return ResolveImageType(ResolveSocketSprite(settings, style));
        }

        public static Image.Type ResolvePressableBackgroundImageType(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            if (ResolvePressableBackgroundFillCenter(settings, style))
            {
                return Image.Type.Sliced;
            }

            return ResolveImageType(ResolvePressableBackgroundSprite(settings, style));
        }

        public static Image.Type ResolveOutlineImageType(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            if (ResolveOutlineFillCenter(settings, style))
            {
                return Image.Type.Sliced;
            }

            return ResolveImageType(ResolveOutlineSprite(settings, style));
        }

        public static bool ResolvePreserveAspect(CustomPressableVisualStyle style)
        {
            return false;
        }

        public static bool ResolveSocketFillCenter(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            return resolvedSettings.ResolveVisualStyle(style).SocketGhostFillCenter;
        }

        public static bool ResolvePressableBackgroundFillCenter(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            return resolvedSettings.ResolveVisualStyle(style).PressableBackgroundFillCenter;
        }

        public static bool ResolveOutlineFillCenter(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            return resolvedSettings.ResolveVisualStyle(style).OutlineFillCenter;
        }

        private static Image.Type ResolveImageType(Sprite sprite)
        {
            return sprite != null && sprite.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
        }

        public static float ResolvePixelsPerUnitMultiplier(CustomButtonSettings settings)
        {
            return ResolveSocketPixelsPerUnitMultiplier(settings, CustomPressableVisualStyle.GenericButton);
        }

        public static float ResolveSocketPixelsPerUnitMultiplier(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            CustomPressableVisualStyleProfile profile = resolvedSettings.ResolveVisualStyle(style);
            return profile.SocketGhostSprite != null ? profile.SocketGhostPixelsPerUnitMultiplier : 1f;
        }

        public static float ResolvePressableBackgroundPixelsPerUnitMultiplier(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            CustomPressableVisualStyleProfile profile = resolvedSettings.ResolveVisualStyle(style);
            return profile.PressableBackgroundSprite != null ? profile.PressableBackgroundPixelsPerUnitMultiplier : 1f;
        }

        public static float ResolveOutlinePixelsPerUnitMultiplier(CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            CustomButtonSettings resolvedSettings = settings != null ? settings : CustomButtonSettings.Global;
            CustomPressableVisualStyleProfile profile = resolvedSettings.ResolveVisualStyle(style);
            return profile.OutlineSprite != null ? profile.OutlinePixelsPerUnitMultiplier : 1f;
        }

        public static bool Configure(Image image, CustomButtonSettings settings)
        {
            return Configure(image, settings, CustomPressableVisualStyle.GenericButton);
        }

        public static bool Configure(Image image, CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            if (image == null)
            {
                return false;
            }

            bool changed = false;
            Sprite targetSprite = ResolveSocketSprite(settings, style);
            if (image.sprite != targetSprite)
            {
                image.sprite = targetSprite;
                changed = true;
            }

            if (ClearOverrideSprite(image))
            {
                changed = true;
            }

            Image.Type targetType = ResolveSocketImageType(settings, style);
            if (image.type != targetType)
            {
                image.type = targetType;
                changed = true;
            }

            bool targetPreserveAspect = ResolvePreserveAspect(style);
            if (image.preserveAspect != targetPreserveAspect)
            {
                image.preserveAspect = targetPreserveAspect;
                changed = true;
            }

            bool targetFillCenter = ResolveSocketFillCenter(settings, style);
            if (image.fillCenter != targetFillCenter)
            {
                image.fillCenter = targetFillCenter;
                changed = true;
            }

            if (image.fillMethod != Image.FillMethod.Horizontal)
            {
                image.fillMethod = Image.FillMethod.Horizontal;
                changed = true;
            }

            if (!Mathf.Approximately(image.fillAmount, 1f))
            {
                image.fillAmount = 1f;
                changed = true;
            }

            if (!image.fillClockwise)
            {
                image.fillClockwise = true;
                changed = true;
            }

            if (image.fillOrigin != 0)
            {
                image.fillOrigin = 0;
                changed = true;
            }

            if (image.useSpriteMesh)
            {
                image.useSpriteMesh = false;
                changed = true;
            }

            float targetPixelsPerUnitMultiplier = ResolveSocketPixelsPerUnitMultiplier(settings, style);
            if (!Mathf.Approximately(image.pixelsPerUnitMultiplier, targetPixelsPerUnitMultiplier))
            {
                image.pixelsPerUnitMultiplier = targetPixelsPerUnitMultiplier;
                changed = true;
            }

            Color targetColor = ResolveImageColor(settings, style);
            if (!Approximately(image.color, targetColor))
            {
                image.color = targetColor;
                changed = true;
            }

            if (image.raycastTarget)
            {
                image.raycastTarget = false;
                changed = true;
            }

            bool targetMaskable = ResolveManagedLayerMaskable(style);
            if (image.maskable != targetMaskable)
            {
                image.maskable = targetMaskable;
                changed = true;
            }

            return changed;
        }

        public static bool ConfigurePressableBackground(Image image,
                                                        CustomButtonSettings settings,
                                                        CustomPressableVisualStyle style,
                                                        Image sourceImage)
        {
            if (image == null)
            {
                return false;
            }

            bool changed = false;
            Sprite styleSprite = ResolvePressableBackgroundSprite(settings, style);
            Sprite targetSprite = styleSprite != null ? styleSprite : sourceImage != null ? sourceImage.sprite : null;
            if (image.sprite != targetSprite)
            {
                image.sprite = targetSprite;
                changed = true;
            }

            if (ClearOverrideSprite(image))
            {
                changed = true;
            }

            Image.Type targetType = styleSprite != null || ResolvePressableBackgroundFillCenter(settings, style)
                                            ? ResolvePressableBackgroundImageType(settings, style)
                                            : sourceImage != null ? sourceImage.type : ResolveImageType(targetSprite);
            if (image.type != targetType)
            {
                image.type = targetType;
                changed = true;
            }

            bool targetPreserveAspect = ResolvePreserveAspect(style);
            if (image.preserveAspect != targetPreserveAspect)
            {
                image.preserveAspect = targetPreserveAspect;
                changed = true;
            }

            bool targetFillCenter = ResolvePressableBackgroundFillCenter(settings, style);
            if (image.fillCenter != targetFillCenter)
            {
                image.fillCenter = targetFillCenter;
                changed = true;
            }

            Image.FillMethod targetFillMethod = sourceImage != null ? sourceImage.fillMethod : Image.FillMethod.Horizontal;
            if (image.fillMethod != targetFillMethod)
            {
                image.fillMethod = targetFillMethod;
                changed = true;
            }

            float targetFillAmount = sourceImage != null ? sourceImage.fillAmount : 1f;
            if (!Mathf.Approximately(image.fillAmount, targetFillAmount))
            {
                image.fillAmount = targetFillAmount;
                changed = true;
            }

            bool targetFillClockwise = sourceImage == null || sourceImage.fillClockwise;
            if (image.fillClockwise != targetFillClockwise)
            {
                image.fillClockwise = targetFillClockwise;
                changed = true;
            }

            int targetFillOrigin = sourceImage != null ? sourceImage.fillOrigin : 0;
            if (image.fillOrigin != targetFillOrigin)
            {
                image.fillOrigin = targetFillOrigin;
                changed = true;
            }

            if (image.useSpriteMesh)
            {
                image.useSpriteMesh = false;
                changed = true;
            }

            float targetPixelsPerUnitMultiplier = styleSprite != null
                                                          ? ResolvePressableBackgroundPixelsPerUnitMultiplier(settings, style)
                                                          : sourceImage != null ? sourceImage.pixelsPerUnitMultiplier : 1f;
            if (!Mathf.Approximately(image.pixelsPerUnitMultiplier, targetPixelsPerUnitMultiplier))
            {
                image.pixelsPerUnitMultiplier = targetPixelsPerUnitMultiplier;
                changed = true;
            }

            Color targetColor = targetSprite != null ? Color.white : ColorPalette.TransparentColor;
            if (!Approximately(image.color, targetColor))
            {
                image.color = targetColor;
                changed = true;
            }

            if (image.raycastTarget)
            {
                image.raycastTarget = false;
                changed = true;
            }

            return changed;
        }

        public static bool ConfigureOutline(Image image, CustomButtonSettings settings, CustomPressableVisualStyle style)
        {
            if (image == null)
            {
                return false;
            }

            bool changed = false;
            Sprite targetSprite = ResolveOutlineSprite(settings, style);
            bool shouldBeActive = targetSprite != null;
            if (image.gameObject.activeSelf != shouldBeActive)
            {
                image.gameObject.SetActive(shouldBeActive);
                changed = true;
            }

            if (image.sprite != targetSprite)
            {
                image.sprite = targetSprite;
                changed = true;
            }

            if (ClearOverrideSprite(image))
            {
                changed = true;
            }

            Image.Type targetType = ResolveOutlineImageType(settings, style);
            if (image.type != targetType)
            {
                image.type = targetType;
                changed = true;
            }

            bool targetPreserveAspect = ResolvePreserveAspect(style);
            if (image.preserveAspect != targetPreserveAspect)
            {
                image.preserveAspect = targetPreserveAspect;
                changed = true;
            }

            bool targetFillCenter = ResolveOutlineFillCenter(settings, style);
            if (image.fillCenter != targetFillCenter)
            {
                image.fillCenter = targetFillCenter;
                changed = true;
            }

            if (image.fillMethod != Image.FillMethod.Horizontal)
            {
                image.fillMethod = Image.FillMethod.Horizontal;
                changed = true;
            }

            if (!Mathf.Approximately(image.fillAmount, 1f))
            {
                image.fillAmount = 1f;
                changed = true;
            }

            if (!image.fillClockwise)
            {
                image.fillClockwise = true;
                changed = true;
            }

            if (image.fillOrigin != 0)
            {
                image.fillOrigin = 0;
                changed = true;
            }

            if (image.useSpriteMesh)
            {
                image.useSpriteMesh = false;
                changed = true;
            }

            float targetPixelsPerUnitMultiplier = ResolveOutlinePixelsPerUnitMultiplier(settings, style);
            if (!Mathf.Approximately(image.pixelsPerUnitMultiplier, targetPixelsPerUnitMultiplier))
            {
                image.pixelsPerUnitMultiplier = targetPixelsPerUnitMultiplier;
                changed = true;
            }

            Color targetColor = shouldBeActive ? ColorPalette.OutlineColor : ColorPalette.TransparentColor;
            if (!Approximately(image.color, targetColor))
            {
                image.color = targetColor;
                changed = true;
            }

            if (image.raycastTarget)
            {
                image.raycastTarget = false;
                changed = true;
            }

            bool targetMaskable = ResolveManagedLayerMaskable(style);
            if (image.maskable != targetMaskable)
            {
                image.maskable = targetMaskable;
                changed = true;
            }

            return changed;
        }

        private static bool ResolveManagedLayerMaskable(CustomPressableVisualStyle style)
        {
            return style != CustomPressableVisualStyle.ScrollbarSlider;
        }

        private static bool ClearOverrideSprite(Image image)
        {
            if (image == null)
            {
                return false;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                SerializedObject serializedImage = new SerializedObject(image);
                SerializedProperty overrideSprite = serializedImage.FindProperty("m_OverrideSprite");
                if (overrideSprite != null)
                {
                    bool hadOverride = overrideSprite.objectReferenceValue != null;
                    if (hadOverride)
                    {
                        overrideSprite.objectReferenceValue = null;
                        serializedImage.ApplyModifiedPropertiesWithoutUndo();
                    }

                    return hadOverride;
                }
            }
#endif

            image.overrideSprite = null;
            return false;
        }

        private static bool Approximately(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.001f &&
                   Mathf.Abs(a.g - b.g) < 0.001f &&
                   Mathf.Abs(a.b - b.b) < 0.001f &&
                   Mathf.Abs(a.a - b.a) < 0.001f;
        }
    }
}
