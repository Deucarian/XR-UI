using System;
using Deucarian.XRUI.Controls;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Deucarian.XRUI
{
    public enum XrUiSemanticColor
    {
        Primary,
        Secondary,
        Success,
        Danger,
        Warning,
        Info,
        Background,
        Disabled,
        SocketGhost,
        TitleText,
        BodyText,
        SmallText,
        MutedText,
        InputText,
        PlaceholderText,
        Icon,
        Image,
        ImageMuted,
        ImageSubtle,
        SliderTrack,
        Outline,
        ErrorText,
        KeyboardAccent,
        KeyboardBackground,
        KeyboardOutline,
        KeyboardInputText,
        ControlSubtleBackground,
        ControlDarkBorder,
        SliderHandle,
        LoadingIndicator,
        DropdownInvalidState,
        KeyboardContentAccent,
        Transparent,
    }

    [CreateAssetMenu(fileName = "XrUiColorPalette", menuName = "Deucarian/XR UI/Color Palette")]
    public sealed class XrUiColorPalette : ScriptableObject
    {
        private const float SemanticColorMatchTolerance = 0.08f;
        private const float SemanticAlphaMatchTolerance = 0.02f;

        public static readonly Color DefaultSuccess = Deucarian.Theming.DeucarianLegacyControlColors.Success;
        public static readonly Color DefaultDanger = Deucarian.Theming.DeucarianLegacyControlColors.Danger;
        public static readonly Color DefaultWarning = Deucarian.Theming.DeucarianLegacyControlColors.Warning;
        public static readonly Color DefaultInfo = Deucarian.Theming.DeucarianLegacyControlColors.Info;
        public static readonly Color DefaultPrimary = Deucarian.Theming.DeucarianLegacyControlColors.Primary;
        public static readonly Color DefaultSecondary = Deucarian.Theming.DeucarianLegacyControlColors.Secondary;
        public static readonly Color DefaultBackground = Deucarian.Theming.DeucarianLegacyControlColors.Background;
        public static readonly Color DefaultDisabled = Deucarian.Theming.DeucarianLegacyControlColors.Disabled;
        public static readonly Color DefaultSocketGhost = Deucarian.Theming.DeucarianLegacyControlColors.SocketGhost;
        public static readonly Color DefaultTitleText = Deucarian.Theming.DeucarianLegacyControlColors.TitleText;
        public static readonly Color DefaultBodyText = Deucarian.Theming.DeucarianLegacyControlColors.BodyText;
        public static readonly Color DefaultSmallText = Deucarian.Theming.DeucarianLegacyControlColors.SmallText;
        public static readonly Color DefaultMutedText = Deucarian.Theming.DeucarianLegacyControlColors.MutedText;
        public static readonly Color DefaultInputText = Deucarian.Theming.DeucarianLegacyControlColors.InputText;
        public static readonly Color DefaultPlaceholderText = Deucarian.Theming.DeucarianLegacyControlColors.PlaceholderText;
        public static readonly Color DefaultIcon = Deucarian.Theming.DeucarianLegacyControlColors.Icon;
        public static readonly Color DefaultImage = Deucarian.Theming.DeucarianLegacyControlColors.Image;
        public static readonly Color DefaultImageMuted = Deucarian.Theming.DeucarianLegacyControlColors.ImageMuted;
        public static readonly Color DefaultImageSubtle = Deucarian.Theming.DeucarianLegacyControlColors.ImageSubtle;
        public static readonly Color DefaultSliderTrack = Deucarian.Theming.DeucarianLegacyControlColors.SliderTrack;
        public static readonly Color DefaultOutline = Deucarian.Theming.DeucarianLegacyControlColors.Outline;
        public static readonly Color DefaultErrorText = Deucarian.Theming.DeucarianLegacyControlColors.ErrorText;
        public static readonly Color DefaultKeyboardAccent = Deucarian.Theming.DeucarianLegacyControlColors.KeyboardAccent;
        public static readonly Color DefaultKeyboardBackground = Deucarian.Theming.DeucarianLegacyControlColors.KeyboardBackground;
        public static readonly Color DefaultKeyboardOutline = Deucarian.Theming.DeucarianLegacyControlColors.KeyboardOutline;
        public static readonly Color DefaultKeyboardInputText = Deucarian.Theming.DeucarianLegacyControlColors.KeyboardInputText;
        public static readonly Color DefaultControlSubtleBackground = Deucarian.Theming.DeucarianLegacyControlColors.ControlSubtleBackground;
        public static readonly Color DefaultControlDarkBorder = Deucarian.Theming.DeucarianLegacyControlColors.ControlDarkBorder;
        public static readonly Color DefaultSliderHandle = Deucarian.Theming.DeucarianLegacyControlColors.SliderHandle;
        public static readonly Color DefaultLoadingIndicator = Deucarian.Theming.DeucarianLegacyControlColors.LoadingIndicator;
        public static readonly Color DefaultDropdownInvalidState = Deucarian.Theming.DeucarianLegacyControlColors.DropdownInvalidState;
        public static readonly Color DefaultKeyboardContentAccent = Deucarian.Theming.DeucarianLegacyControlColors.KeyboardContentAccent;
        public static readonly Color DefaultTransparent = Deucarian.Theming.DeucarianLegacyControlColors.Transparent;

        public static event Action<XrUiColorPalette> PaletteChanged;

        [Header("Semantic Colors")]
        [SerializeField] private Color _primary = DefaultPrimary;
        [SerializeField] private Color _secondary = DefaultSecondary;
        [SerializeField] private Color _success = DefaultSuccess;
        [SerializeField] private Color _danger = DefaultDanger;
        [SerializeField] private Color _warning = DefaultWarning;
        [SerializeField] private Color _info = DefaultInfo;
        [SerializeField] private Color _background = DefaultBackground;
        [SerializeField] private Color _disabled = DefaultDisabled;
        [SerializeField] private Color _socketGhost = DefaultSocketGhost;

        [Header("UI Element Colors")]
        [SerializeField] private Color _titleText = DefaultTitleText;
        [SerializeField] private Color _bodyText = DefaultBodyText;
        [SerializeField] private Color _smallText = DefaultSmallText;
        [SerializeField] private Color _mutedText = DefaultMutedText;
        [SerializeField] private Color _inputText = DefaultInputText;
        [SerializeField] private Color _placeholderText = DefaultPlaceholderText;
        [SerializeField] private Color _icon = DefaultIcon;
        [SerializeField] private Color _image = DefaultImage;
        [SerializeField] private Color _imageMuted = DefaultImageMuted;
        [SerializeField] private Color _imageSubtle = DefaultImageSubtle;
        [SerializeField] private Color _sliderTrack = DefaultSliderTrack;
        [SerializeField] private Color _outline = DefaultOutline;
        [SerializeField] private Color _errorText = DefaultErrorText;
        [SerializeField] private Color _keyboardAccent = DefaultKeyboardAccent;
        [SerializeField] private Color _keyboardBackground = DefaultKeyboardBackground;
        [SerializeField] private Color _keyboardOutline = DefaultKeyboardOutline;
        [SerializeField] private Color _keyboardInputText = DefaultKeyboardInputText;
        [SerializeField] private Color _controlSubtleBackground = DefaultControlSubtleBackground;
        [SerializeField] private Color _controlDarkBorder = DefaultControlDarkBorder;
        [SerializeField] private Color _sliderHandle = DefaultSliderHandle;
        [SerializeField] private Color _loadingIndicator = DefaultLoadingIndicator;
        [SerializeField] private Color _dropdownInvalidState = DefaultDropdownInvalidState;
        [SerializeField] private Color _keyboardContentAccent = DefaultKeyboardContentAccent;
        [SerializeField] private Color _transparent = DefaultTransparent;

        [Header("Palette Output Mode")]
        [SerializeField] private bool _useInteractionStateMultipliers = true;

        [Header("Interaction State Multipliers")]
        [SerializeField] [Range(0f, 2f)] private float _normalMultiplier = 1f;
        [SerializeField] [Range(0f, 2f)] private float _highlightedMultiplier = 1f;
        [SerializeField] [Range(0f, 2f)] private float _pressedMultiplier = 1f;
        [SerializeField] [Range(0f, 2f)] private float _selectedMultiplier = 1f;
        [SerializeField] [Range(0f, 2f)] private float _disabledMultiplier = 1f;

        private XrUiInteractionColors? resolvedInteractionColors;

        /// <summary>Supplies fully resolved state colors from an external theme, without changing authored multiplier settings.</summary>
        public void SetResolvedInteractionColors(Color normal, Color highlighted, Color pressed, Color selected, Color disabled)
        {
            resolvedInteractionColors = new XrUiInteractionColors(normal, highlighted, pressed, selected, disabled);
            NotifyPaletteChanged();
        }

        public Color Normal { get => GetInteractionColor(CustomButtonVisualState.Normal); set => Background = value; }
        public Color Highlighted { get => GetInteractionColor(CustomButtonVisualState.Highlighted); set { Secondary = value; HighlightedMultiplier = 1f; } }
        public Color Pressed { get => GetInteractionColor(CustomButtonVisualState.Pressed); set { Primary = value; PressedMultiplier = 1f; } }
        public Color Selected { get => GetInteractionColor(CustomButtonVisualState.Selected); set { Primary = value; SelectedMultiplier = 1f; } }
        public Color Disabled { get => _disabled; set => SetColor(ref _disabled, value); }
        public Color Primary { get => _primary; set => SetColor(ref _primary, value); }
        public Color Secondary { get => _secondary; set => SetColor(ref _secondary, value); }
        public Color Success { get => _success; set => SetColor(ref _success, value); }
        public Color Danger { get => _danger; set => SetColor(ref _danger, value); }
        public Color Warning { get => _warning; set => SetColor(ref _warning, value); }
        public Color Info { get => _info; set => SetColor(ref _info, value); }
        public Color Background { get => _background; set => SetColor(ref _background, value); }
        public Color SocketGhost { get => _socketGhost; set => SetColor(ref _socketGhost, value); }
        public Color TitleText { get => _titleText; set => SetColor(ref _titleText, value); }
        public Color BodyText { get => _bodyText; set => SetColor(ref _bodyText, value); }
        public Color SmallText { get => _smallText; set => SetColor(ref _smallText, value); }
        public Color MutedText { get => _mutedText; set => SetColor(ref _mutedText, value); }
        public Color InputText { get => _inputText; set => SetColor(ref _inputText, value); }
        public Color PlaceholderText { get => _placeholderText; set => SetColor(ref _placeholderText, value); }
        public Color Icon { get => _icon; set => SetColor(ref _icon, value); }
        public Color Image { get => _image; set => SetColor(ref _image, value); }
        public Color ImageMuted { get => _imageMuted; set => SetColor(ref _imageMuted, value); }
        public Color ImageSubtle { get => _imageSubtle; set => SetColor(ref _imageSubtle, value); }
        public Color SliderTrack { get => _sliderTrack; set => SetColor(ref _sliderTrack, value); }
        public Color Outline { get => _outline; set => SetColor(ref _outline, value); }
        public Color ErrorText { get => _errorText; set => SetColor(ref _errorText, value); }
        public Color KeyboardAccent { get => _keyboardAccent; set => SetColor(ref _keyboardAccent, value); }
        public Color KeyboardBackground { get => _keyboardBackground; set => SetColor(ref _keyboardBackground, value); }
        public Color KeyboardOutline { get => _keyboardOutline; set => SetColor(ref _keyboardOutline, value); }
        public Color KeyboardInputText { get => _keyboardInputText; set => SetColor(ref _keyboardInputText, value); }
        public Color ControlSubtleBackground { get => _controlSubtleBackground; set => SetColor(ref _controlSubtleBackground, value); }
        public Color ControlDarkBorder { get => _controlDarkBorder; set => SetColor(ref _controlDarkBorder, value); }
        public Color SliderHandle { get => _sliderHandle; set => SetColor(ref _sliderHandle, value); }
        public Color LoadingIndicator { get => _loadingIndicator; set => SetColor(ref _loadingIndicator, value); }
        public Color DropdownInvalidState { get => _dropdownInvalidState; set => SetColor(ref _dropdownInvalidState, value); }
        public Color KeyboardContentAccent { get => _keyboardContentAccent; set => SetColor(ref _keyboardContentAccent, value); }
        public Color Transparent { get => _transparent; set => SetColor(ref _transparent, value); }
        public bool UseInteractionStateMultipliers { get => _useInteractionStateMultipliers; set => SetBool(ref _useInteractionStateMultipliers, value); }
        public float NormalMultiplier { get => _normalMultiplier; set => SetMultiplier(ref _normalMultiplier, value); }
        public float HighlightedMultiplier { get => _highlightedMultiplier; set => SetMultiplier(ref _highlightedMultiplier, value); }
        public float PressedMultiplier { get => _pressedMultiplier; set => SetMultiplier(ref _pressedMultiplier, value); }
        public float SelectedMultiplier { get => _selectedMultiplier; set => SetMultiplier(ref _selectedMultiplier, value); }
        public float DisabledMultiplier { get => _disabledMultiplier; set => SetMultiplier(ref _disabledMultiplier, value); }

        public static XrUiColorPalette Global => XrUiColorPaletteRegistry.Global;

        public Color GetSemanticColor(XrUiSemanticColor semantic)
        {
            return semantic switch
            {
                XrUiSemanticColor.Primary => Primary,
                XrUiSemanticColor.Secondary => Secondary,
                XrUiSemanticColor.Success => Success,
                XrUiSemanticColor.Danger => Danger,
                XrUiSemanticColor.Warning => Warning,
                XrUiSemanticColor.Info => Info,
                XrUiSemanticColor.Disabled => Disabled,
                XrUiSemanticColor.SocketGhost => SocketGhost,
                XrUiSemanticColor.TitleText => TitleText,
                XrUiSemanticColor.BodyText => BodyText,
                XrUiSemanticColor.SmallText => SmallText,
                XrUiSemanticColor.MutedText => MutedText,
                XrUiSemanticColor.InputText => InputText,
                XrUiSemanticColor.PlaceholderText => PlaceholderText,
                XrUiSemanticColor.Icon => Icon,
                XrUiSemanticColor.Image => Image,
                XrUiSemanticColor.ImageMuted => ImageMuted,
                XrUiSemanticColor.ImageSubtle => ImageSubtle,
                XrUiSemanticColor.SliderTrack => SliderTrack,
                XrUiSemanticColor.Outline => Outline,
                XrUiSemanticColor.ErrorText => ErrorText,
                XrUiSemanticColor.KeyboardAccent => KeyboardAccent,
                XrUiSemanticColor.KeyboardBackground => KeyboardBackground,
                XrUiSemanticColor.KeyboardOutline => KeyboardOutline,
                XrUiSemanticColor.KeyboardInputText => KeyboardInputText,
                XrUiSemanticColor.ControlSubtleBackground => ControlSubtleBackground,
                XrUiSemanticColor.ControlDarkBorder => ControlDarkBorder,
                XrUiSemanticColor.SliderHandle => SliderHandle,
                XrUiSemanticColor.LoadingIndicator => LoadingIndicator,
                XrUiSemanticColor.DropdownInvalidState => DropdownInvalidState,
                XrUiSemanticColor.KeyboardContentAccent => KeyboardContentAccent,
                XrUiSemanticColor.Transparent => Transparent,
                _ => Background,
            };
        }

        public void SetSemanticColor(XrUiSemanticColor semantic, Color color)
        {
            switch (semantic)
            {
                case XrUiSemanticColor.Primary: Primary = color; break;
                case XrUiSemanticColor.Secondary: Secondary = color; break;
                case XrUiSemanticColor.Success: Success = color; break;
                case XrUiSemanticColor.Danger: Danger = color; break;
                case XrUiSemanticColor.Warning: Warning = color; break;
                case XrUiSemanticColor.Info: Info = color; break;
                case XrUiSemanticColor.Disabled: Disabled = color; break;
                case XrUiSemanticColor.SocketGhost: SocketGhost = color; break;
                case XrUiSemanticColor.TitleText: TitleText = color; break;
                case XrUiSemanticColor.BodyText: BodyText = color; break;
                case XrUiSemanticColor.SmallText: SmallText = color; break;
                case XrUiSemanticColor.MutedText: MutedText = color; break;
                case XrUiSemanticColor.InputText: InputText = color; break;
                case XrUiSemanticColor.PlaceholderText: PlaceholderText = color; break;
                case XrUiSemanticColor.Icon: Icon = color; break;
                case XrUiSemanticColor.Image: Image = color; break;
                case XrUiSemanticColor.ImageMuted: ImageMuted = color; break;
                case XrUiSemanticColor.ImageSubtle: ImageSubtle = color; break;
                case XrUiSemanticColor.SliderTrack: SliderTrack = color; break;
                case XrUiSemanticColor.Outline: Outline = color; break;
                case XrUiSemanticColor.ErrorText: ErrorText = color; break;
                case XrUiSemanticColor.KeyboardAccent: KeyboardAccent = color; break;
                case XrUiSemanticColor.KeyboardBackground: KeyboardBackground = color; break;
                case XrUiSemanticColor.KeyboardOutline: KeyboardOutline = color; break;
                case XrUiSemanticColor.KeyboardInputText: KeyboardInputText = color; break;
                case XrUiSemanticColor.ControlSubtleBackground: ControlSubtleBackground = color; break;
                case XrUiSemanticColor.ControlDarkBorder: ControlDarkBorder = color; break;
                case XrUiSemanticColor.SliderHandle: SliderHandle = color; break;
                case XrUiSemanticColor.LoadingIndicator: LoadingIndicator = color; break;
                case XrUiSemanticColor.DropdownInvalidState: DropdownInvalidState = color; break;
                case XrUiSemanticColor.KeyboardContentAccent: KeyboardContentAccent = color; break;
                case XrUiSemanticColor.Transparent: Transparent = color; break;
                default: Background = color; break;
            }
        }

        public float GetInteractionMultiplier(CustomButtonVisualState state)
        {
            if (!_useInteractionStateMultipliers)
            {
                return 1f;
            }

            return state switch
            {
                CustomButtonVisualState.Highlighted => HighlightedMultiplier,
                CustomButtonVisualState.Pressed => PressedMultiplier,
                CustomButtonVisualState.Selected => SelectedMultiplier,
                CustomButtonVisualState.Disabled => DisabledMultiplier,
                _ => NormalMultiplier,
            };
        }

        public Color GetInteractionColor(CustomButtonVisualState state)
        {
            return GetInteractionColor(state, XrUiSemanticColor.Background);
        }

        /// <summary>Resolves authored semantic colors and multipliers without transient theme-state overrides.</summary>
        public Color GetAuthoredInteractionColor(CustomButtonVisualState state) =>
            XrUiInteractionColors.ResolveLegacy(this, state, XrUiSemanticColor.Background);

        public Color GetInteractionColor(CustomButtonVisualState state, XrUiSemanticColor normalSemantic)
        {
            return resolvedInteractionColors.HasValue
                ? resolvedInteractionColors.Value.Resolve(this, state, normalSemantic)
                : XrUiInteractionColors.ResolveLegacy(this, state, normalSemantic);
        }

        public static bool TryResolveSemanticColor(Color color, out XrUiSemanticColor semanticColor)
            => TryResolveSemanticColor(color, Global, out semanticColor);

        public static bool TryResolveSemanticColor(Color color, XrUiColorPalette palette, out XrUiSemanticColor semanticColor)
        {
            palette = palette != null ? palette : Global;
            if (TryResolveSemantic(color, DefaultSuccess, palette.Success, XrUiSemanticColor.Success, out semanticColor) ||
                TryResolveSemantic(color, DefaultDanger, palette.Danger, XrUiSemanticColor.Danger, out semanticColor) ||
                TryResolveSemantic(color, DefaultWarning, palette.Warning, XrUiSemanticColor.Warning, out semanticColor) ||
                TryResolveSemantic(color, DefaultInfo, palette.Info, XrUiSemanticColor.Info, out semanticColor) ||
                TryResolveSemantic(color, DefaultPrimary, palette.Primary, XrUiSemanticColor.Primary, out semanticColor) ||
                TryResolveSemantic(color, DefaultSecondary, palette.Secondary, XrUiSemanticColor.Secondary, out semanticColor) ||
                TryResolveSemantic(color, DefaultBackground, palette.Background, XrUiSemanticColor.Background, out semanticColor) ||
                TryResolveSemantic(color, DefaultDisabled, palette.Disabled, XrUiSemanticColor.Disabled, out semanticColor) ||
                TryResolveSemantic(color, DefaultSocketGhost, palette.SocketGhost, XrUiSemanticColor.SocketGhost, out semanticColor))
            {
                return true;
            }

            semanticColor = XrUiSemanticColor.Background;
            return false;
        }

        public static bool TryResolveUiColor(Color color, out XrUiSemanticColor semanticColor)
        {
            XrUiColorPalette palette = Global;
            if (color.a <= SemanticAlphaMatchTolerance)
            {
                semanticColor = XrUiSemanticColor.Transparent;
                return true;
            }

            if (TryResolveSemanticWithAlpha(color, DefaultPlaceholderText, palette.PlaceholderText, XrUiSemanticColor.PlaceholderText, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultImageSubtle, palette.ImageSubtle, XrUiSemanticColor.ImageSubtle, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultTitleText, palette.TitleText, XrUiSemanticColor.TitleText, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultSmallText, palette.SmallText, XrUiSemanticColor.SmallText, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultMutedText, palette.MutedText, XrUiSemanticColor.MutedText, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultInputText, palette.InputText, XrUiSemanticColor.InputText, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultImageMuted, palette.ImageMuted, XrUiSemanticColor.ImageMuted, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultSliderTrack, palette.SliderTrack, XrUiSemanticColor.SliderTrack, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultErrorText, palette.ErrorText, XrUiSemanticColor.ErrorText, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultKeyboardAccent, palette.KeyboardAccent, XrUiSemanticColor.KeyboardAccent, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultKeyboardBackground, palette.KeyboardBackground, XrUiSemanticColor.KeyboardBackground, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultKeyboardOutline, palette.KeyboardOutline, XrUiSemanticColor.KeyboardOutline, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultKeyboardInputText, palette.KeyboardInputText, XrUiSemanticColor.KeyboardInputText, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultControlSubtleBackground, palette.ControlSubtleBackground, XrUiSemanticColor.ControlSubtleBackground, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultControlDarkBorder, palette.ControlDarkBorder, XrUiSemanticColor.ControlDarkBorder, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultSliderHandle, palette.SliderHandle, XrUiSemanticColor.SliderHandle, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultLoadingIndicator, palette.LoadingIndicator, XrUiSemanticColor.LoadingIndicator, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultDropdownInvalidState, palette.DropdownInvalidState, XrUiSemanticColor.DropdownInvalidState, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultKeyboardContentAccent, palette.KeyboardContentAccent, XrUiSemanticColor.KeyboardContentAccent, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultBodyText, palette.BodyText, XrUiSemanticColor.BodyText, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultIcon, palette.Icon, XrUiSemanticColor.Icon, out semanticColor) ||
                TryResolveSemanticWithAlpha(color, DefaultImage, palette.Image, XrUiSemanticColor.Image, out semanticColor))
            {
                return true;
            }

            return TryResolveSemanticColor(color, out semanticColor);
        }

        public void NotifyPaletteChanged()
        {
            PaletteChanged?.Invoke(this);
            CustomButtonSettings.NotifyGlobalSettingsChanged();
#if UNITY_EDITOR
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
#endif
        }

        public static void SetRuntimePalette(XrUiColorPalette palette) =>
            NotifyGlobalPaletteChangedIfNeeded(
                XrUiColorPaletteRegistry.SetRuntimePalette(palette));

        public static void ClearRuntimePalette() =>
            NotifyGlobalPaletteChangedIfNeeded(
                XrUiColorPaletteRegistry.ClearRuntimePalette());

        /// <summary>Registers an owner-scoped override. Dispose only removes this registration, never another owner's palette.</summary>
        public static System.IDisposable RegisterRuntimePalette(XrUiColorPalette palette) =>
            XrUiColorPaletteRegistry.RegisterRuntimePalette(palette);

        internal static void NotifyGlobalPaletteChangedIfNeeded(bool paletteChanged)
        {
            if (!paletteChanged)
            {
                return;
            }

            PaletteChanged?.Invoke(Global);
            CustomButtonSettings.NotifyGlobalSettingsChanged();
#if UNITY_EDITOR
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
#endif
        }

        private void SetColor(ref Color target, Color value)
        {
            if (target == value)
            {
                return;
            }

            target = value;
            NotifyPaletteChanged();
        }

        private void SetMultiplier(ref float target, float value)
        {
            value = Mathf.Clamp(value, 0f, 2f);
            if (Mathf.Approximately(target, value))
            {
                return;
            }

            target = value;
            NotifyPaletteChanged();
        }

        private void SetBool(ref bool target, bool value)
        {
            if (target == value)
            {
                return;
            }

            target = value;
            NotifyPaletteChanged();
        }

        private void OnValidate()
        {
            _normalMultiplier = Mathf.Clamp(_normalMultiplier, 0f, 2f);
            _highlightedMultiplier = Mathf.Clamp(_highlightedMultiplier, 0f, 2f);
            _pressedMultiplier = Mathf.Clamp(_pressedMultiplier, 0f, 2f);
            _selectedMultiplier = Mathf.Clamp(_selectedMultiplier, 0f, 2f);
            _disabledMultiplier = Mathf.Clamp(_disabledMultiplier, 0f, 2f);
            NotifyPaletteChanged();
        }

        private static bool IsNearSemantic(Color color, Color defaultColor, Color paletteColor)
        {
            return IsNear(color, defaultColor) || IsNear(color, paletteColor);
        }

        private static bool TryResolveSemantic(Color color,
                                               Color defaultColor,
                                               Color paletteColor,
                                               XrUiSemanticColor semantic,
                                               out XrUiSemanticColor semanticColor)
        {
            if (IsNearSemantic(color, defaultColor, paletteColor))
            {
                semanticColor = semantic;
                return true;
            }

            semanticColor = XrUiSemanticColor.Background;
            return false;
        }

        private static bool IsNear(Color a, Color b)
        {
            float dr = a.r - b.r;
            float dg = a.g - b.g;
            float db = a.b - b.b;
            return dr * dr + dg * dg + db * db <= SemanticColorMatchTolerance * SemanticColorMatchTolerance;
        }

        private static bool IsNearWithAlpha(Color a, Color b)
        {
            return IsNear(a, b) && Mathf.Abs(a.a - b.a) <= SemanticAlphaMatchTolerance;
        }

        private static bool TryResolveSemanticWithAlpha(Color color,
                                                        Color defaultColor,
                                                        Color paletteColor,
                                                        XrUiSemanticColor semantic,
                                                        out XrUiSemanticColor semanticColor)
        {
            if (IsNearWithAlpha(color, defaultColor) || IsNearWithAlpha(color, paletteColor))
            {
                semanticColor = semantic;
                return true;
            }

            semanticColor = XrUiSemanticColor.Background;
            return false;
        }
    }
}
