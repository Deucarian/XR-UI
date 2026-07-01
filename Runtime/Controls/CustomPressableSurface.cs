using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace Deucarian.XRUI.Controls
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(50)]
    [AddComponentMenu("UI (Canvas)/Custom Pressable Surface", 30)]
    public sealed class CustomPressableSurface : MonoBehaviour,
                                                  IPointerDownHandler,
                                                  IPointerUpHandler,
                                                  IPointerExitHandler
    {
        public const string PressableVisualRootName = "PressableVisualRoot";
        public const string SocketGhostName = "SocketGhost";
        internal const string MovingRootGraphicName = "PressableBackground";
        internal const string OutlineGraphicName = "Outline";

        private readonly CustomPressableSurfaceHierarchy _hierarchy = new();
        private readonly CustomPressablePokeTracker _pokeTracker = new();
        private readonly CustomPressableSurfaceVisuals _visuals = new();

        internal RectTransform _rectTransform;
        internal Selectable _selectable;
        internal ICustomPressableSelectableVisualOverride _selectableOverride;
        private bool _isPointerFallbackActive;
        private bool _hasActivatedThisContact;
        private bool _activationArmedForContact;
        private float _pointerFallbackDepth01;
        private int _pointerFallbackPointerId = int.MinValue;
        private object _pokeContactClaimKey;

        [Header("Global Settings")]
        [SerializeField] internal CustomButtonSettings _settings;
        [SerializeField] internal CustomPressableVisualStyle _visualStyle = CustomPressableVisualStyle.GenericButton;

        [Header("Per Element Overrides")]
        [SerializeField] internal bool _overridePressDepthDistance;
        [SerializeField] [Min(0.001f)] internal float _pressDepthDistance = 10f;
        [SerializeField] internal bool _overrideActivationDepth;
        [SerializeField] [Range(0f, 1f)] internal float _activationDepth = 0.9f;
        [SerializeField] internal bool _overrideRaycastPadding;
        [SerializeField] internal Vector4 _raycastPadding;

        [Header("Hierarchy")]
        [SerializeField] internal RectTransform _pressableVisualRoot;
        [SerializeField] internal Graphic _socketGraphic;
        [SerializeField] internal bool _autoCreateHierarchy = true;
        [SerializeField] internal bool _moveExistingChildrenToVisualRoot = true;

        [Header("Development Fallback")]
        [SerializeField] internal bool _enablePointerFallback;

        public event Action<CustomPressableSurface> Activated;
        public event Action<CustomPressableSurface> Released;
        public event Action<CustomPressableSurface, float> PressDepthChanged;

        public CustomButtonSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                ApplySettingsChanged(null);
            }
        }

        public CustomPressableVisualStyle VisualStyle
        {
            get => _visualStyle;
            set
            {
                if (_visualStyle == value)
                {
                    return;
                }

                _visualStyle = value;
                ApplySettingsChanged(null);
            }
        }

        public RectTransform PressableVisualRoot
        {
            get => _pressableVisualRoot;
            set
            {
                if (_pressableVisualRoot == value)
                {
                    return;
                }

                _pressableVisualRoot = value;
                _visuals.CacheInitialPosition(this, true);
                _visuals.RefreshTargetPosition(this, true, ShouldShowDisabledPressVisual());
                RebindToPokeProviderIfActive();
            }
        }

        public Graphic SocketGraphic
        {
            get => _socketGraphic;
            set
            {
                _socketGraphic = value;
                _visuals.ResetSocketColorCache();
                _visuals.ApplySocketPressColor(this, _socketGraphic);
            }
        }

        public float PressDepthDistance
        {
            get => _overridePressDepthDistance ? Mathf.Max(0.001f, _pressDepthDistance) : ResolvedSettings.PressDepthDistance;
            set
            {
                _overridePressDepthDistance = true;
                _pressDepthDistance = Mathf.Max(0.001f, value);
                _visuals.RefreshTargetPosition(this, true, ShouldShowDisabledPressVisual());
            }
        }

        public float ActivationDepth
        {
            get => _overrideActivationDepth ? Mathf.Clamp01(_activationDepth) : ResolvedSettings.ActivationDepth;
            set
            {
                _overrideActivationDepth = true;
                _activationDepth = Mathf.Clamp01(value);
            }
        }

        public Vector4 RaycastPadding
        {
            get => _overrideRaycastPadding ? CustomButtonSettings.ClampNonNegative(_raycastPadding) : ResolvedSettings.RaycastPadding;
            set
            {
                Vector4 clamped = CustomButtonSettings.ClampNonNegative(value);
                if (_overrideRaycastPadding && _raycastPadding == clamped)
                {
                    return;
                }

                _overrideRaycastPadding = true;
                _raycastPadding = clamped;
                if (!Application.isPlaying || _pressableVisualRoot == null)
                {
                    EnsureHierarchy();
                }
                else
                {
                    ApplyHitRaycastPadding();
                }
            }
        }

        public float PressDepth01 => _visuals.PressDepth01;
        public float TargetPressDepth01 => _visuals.TargetPressDepth01;
        public float Hover01 => _visuals.Hover01;
        public float SelectedHoldDepth01 => _visuals.SelectedHoldDepth01;
        public bool IsTargeted => _pokeTracker.IsTargeted || _isPointerFallbackActive;
        public bool IsActivated => _hasActivatedThisContact;
        public bool IsFullyPressed => _hasActivatedThisContact;

        public bool AutoCreateHierarchy
        {
            get => _autoCreateHierarchy;
            set
            {
                _autoCreateHierarchy = value;
                ApplySettingsChanged(null);
            }
        }

        public bool MoveExistingChildrenToVisualRootOnCreate
        {
            get => _moveExistingChildrenToVisualRoot;
            set => _moveExistingChildrenToVisualRoot = value;
        }

        public bool EnablePointerFallback
        {
            get => _enablePointerFallback;
            set => _enablePointerFallback = value;
        }

        internal CustomButtonSettings ResolvedSettings => _settings != null ? _settings : CustomButtonSettings.Global;
        internal CustomPressableVisualStyle ResolvedVisualStyle => _visualStyle;
        internal Vector4 ResolvedRaycastPadding => RaycastPadding;
        internal Vector4 ResolvedHitRaycastPadding => ResolvedSettings.ResolveRaycastPadding(AllowsPointerFallback);

        private bool AllowsPointerFallback
        {
            get
            {
                if (_enablePointerFallback)
                {
                    return true;
                }

#if UNITY_EDITOR
                return ResolvedSettings.EditorMouseFallbackEnabled;
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            EnsureReferences();
            EnsureHierarchy();
            _visuals.CacheInitialPosition(this, false);
            _visuals.RefreshTargetPosition(this, true, ShouldShowDisabledPressVisual());
        }

        private void OnEnable()
        {
            EnsureReferences();
            EnsureHierarchy();
            _visuals.CacheInitialPosition(this, false);
            _visuals.RefreshTargetPosition(this, true, ShouldShowDisabledPressVisual());
            _pokeTracker.Bind(this);
        }

        private void OnDisable()
        {
            _pokeTracker.ClearBindings();
            _isPointerFallbackActive = false;
            _pointerFallbackDepth01 = 0f;
            _visuals.ClearSelectedHoldDepth();
            _activationArmedForContact = false;
            ReleaseContact(true);
            _visuals.ResetToRestPosition(this);
        }

        private void OnDestroy()
        {
            _pokeTracker.ClearBindings();
            EndPokeContactClaim();
        }

        private void LateUpdate()
        {
            UpdatePointerFallbackDepth();

            if (_pressableVisualRoot == null || !_visuals.HasInitialPosition)
            {
                return;
            }

            bool showDisabledPressVisual = ShouldShowDisabledPressVisual();
            RefreshDisabledPressVisualState();
            _visuals.RefreshTargetPosition(this, false, showDisabledPressVisual);
            _visuals.MoveVisualRootTowardTarget(this);
            _visuals.PublishCurrentDepthIfChanged(this);
            if (!showDisabledPressVisual)
            {
                UpdateActivationState();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _pressDepthDistance = Mathf.Max(0.001f, _pressDepthDistance);
            _activationDepth = Mathf.Clamp01(_activationDepth);
            _raycastPadding = CustomButtonSettings.ClampNonNegative(_raycastPadding);
            _visuals.RefreshEditorVisualState(this);
        }

        private void OnDrawGizmosSelected()
        {
            if (transform is not RectTransform rectTransform)
            {
                return;
            }

            DrawRaycastPaddingGizmo(rectTransform, ResolvedHitRaycastPadding);
        }
#endif

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!AllowsPointerFallback || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (!CustomPressablePointerContactRegistry.TryBegin(eventData.pointerId, eventData.clickTime, this))
            {
                return;
            }

            _pointerFallbackPointerId = eventData.pointerId;
            _isPointerFallbackActive = true;
            _activationArmedForContact = true;
            _pointerFallbackDepth01 = ResolvedSettings.MouseFallbackUseHoldRamp ? 0f : 1f;
            ApplyResolvedPressDepth();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                CustomPressablePointerContactRegistry.End(eventData.pointerId);
            }

            if (!_isPointerFallbackActive || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _isPointerFallbackActive = false;
            _pointerFallbackPointerId = int.MinValue;
            _pointerFallbackDepth01 = 0f;
            if (_pokeTracker.IsTargeted)
            {
                ApplyResolvedPressDepth();
                return;
            }

            ReleaseContact(false);
        }

        public void OnPointerExit(PointerEventData _)
        {
            if (!AllowsPointerFallback && !_isPointerFallbackActive)
            {
                return;
            }

            if (_isPointerFallbackActive)
            {
                return;
            }

            _isPointerFallbackActive = false;
            _pointerFallbackDepth01 = 0f;
            ReleaseContact(false);
        }

        public void ForceRelease()
        {
            EndPointerFallbackClaim();
            _isPointerFallbackActive = false;
            _pointerFallbackDepth01 = 0f;
            ReleaseContact(false);
        }

        public void SetSocketBaseColor(Color baseColor)
        {
            EnsureReferences();
            EnsureHierarchy();
            _visuals.SetSocketBaseColor(this, baseColor);
        }

        public void SetSelectedVisualHold(bool selected)
        {
            SetSelectedVisualHoldDepth(selected ? 1f : 0f);
        }

        public void SetSelectedVisualHoldDepth(float depth01)
        {
            _visuals.SetSelectedHoldDepth(this, depth01);
        }

        public void ApplySettingsChanged(CustomButtonSettings changedSettings)
        {
            if (!UsesSettings(changedSettings))
            {
                return;
            }

            EnsureReferences();
            EnsureHierarchy();
            _visuals.ApplySocketPressColor(this, _socketGraphic);
            RebindToPokeProviderIfActive();

            if (_pressableVisualRoot == null)
            {
                return;
            }

            _visuals.CacheInitialPosition(this, false);
            _visuals.RefreshTargetPosition(this, true, ShouldShowDisabledPressVisual());
        }

        public void RefreshPokeTargetBindings()
        {
            EnsureReferences();
            EnsureHierarchy();
            _pokeTracker.Bind(this);
        }

        internal void EnsureReferences()
        {
            _rectTransform ??= transform as RectTransform;
            _selectable ??= GetComponent<Selectable>();

            if (_selectableOverride is UnityEngine.Object unityObject && unityObject == null)
            {
                _selectableOverride = null;
            }
        }

        private void ApplyHitRaycastPadding()
        {
            Graphic rootGraphic = GetComponent<Graphic>();
            Graphic hitGraphic = rootGraphic != null ? rootGraphic : _socketGraphic;
            if (hitGraphic == null)
            {
                return;
            }

            hitGraphic.raycastTarget = true;
            hitGraphic.raycastPadding = CustomButtonSettings.ToUnityRaycastPadding(ResolvedRaycastPadding);
        }

        internal void EnsureHierarchy()
        {
            _hierarchy.Ensure(this);
        }

        internal bool IsOwnedByNestedSelectable(Transform candidate)
        {
            return _hierarchy.IsOwnedByNestedSelectable(this, candidate);
        }

        internal void RaisePressDepthChanged(float depth01)
        {
            PressDepthChanged?.Invoke(this, depth01);
        }

        internal void ApplyResolvedPressDepth()
        {
            if (_pressableVisualRoot == null)
            {
                EnsureHierarchy();
            }

            _visuals.CacheInitialPosition(this, false);
            bool showDisabledPressVisual = ShouldShowDisabledPressVisual();
            bool applyPointerDepthInstantly = _isPointerFallbackActive && !_pokeTracker.IsTargeted;
            _visuals.SetContactDepth(this,
                                     Mathf.Max(_isPointerFallbackActive ? _pointerFallbackDepth01 : 0f,
                                               _pokeTracker.IsTargeted ? _pokeTracker.PokeDepth01 : 0f));

            _visuals.RefreshTargetPosition(this, applyPointerDepthInstantly, showDisabledPressVisual);
            if (showDisabledPressVisual)
            {
                RefreshDisabledPressVisualState();
                return;
            }

            UpdateActivationState();
        }

        internal void BeginPokeContact(object contactKey)
        {
            bool claimed = CustomPressablePokeContactRegistry.TryBegin(contactKey, this);
            if (!claimed)
            {
                return;
            }

            _pokeContactClaimKey = contactKey;
            _activationArmedForContact = true;
        }

        internal void ReleaseContact(bool silent)
        {
            bool wasContacted = _pokeTracker.IsTargeted ||
                                _isPointerFallbackActive ||
                                _hasActivatedThisContact ||
                                _visuals.PressDepth01 > ResolvedSettings.ReleaseDepth;

            _pokeTracker.ClearContact();
            _visuals.SetContactDepth(this, 0f);
            EndPokeContactClaim();
            EndPointerFallbackClaim();
            _isPointerFallbackActive = false;
            _pointerFallbackDepth01 = 0f;
            _activationArmedForContact = false;

            if (_visuals.HasInitialPosition)
            {
                _visuals.RefreshTargetPosition(this, false, ShouldShowDisabledPressVisual());
            }

            bool wasActivated = _hasActivatedThisContact;
            _hasActivatedThisContact = false;

            if (!silent && (wasContacted || wasActivated))
            {
                Released?.Invoke(this);
            }
        }

        internal void ClearReleasedPokeContact(object contactKey)
        {
            CustomPressablePokeContactRegistry.ClearIfReleased(contactKey);
        }

        private void UpdatePointerFallbackDepth()
        {
            if (!_isPointerFallbackActive)
            {
                return;
            }

            CustomButtonSettings settings = ResolvedSettings;
            if (!settings.MouseFallbackUseHoldRamp || settings.MouseFallbackHoldRampSeconds <= 0f)
            {
                if (_visuals.TargetPressDepth01 < 1f)
                {
                    _pointerFallbackDepth01 = 1f;
                    ApplyResolvedPressDepth();
                }

                return;
            }

            _pointerFallbackDepth01 = Mathf.MoveTowards(_pointerFallbackDepth01,
                                                        1f,
                                                        Time.unscaledDeltaTime / settings.MouseFallbackHoldRampSeconds);
            ApplyResolvedPressDepth();
        }

        private void UpdateActivationState()
        {
            float activationDepth = RequiredActivationDepth;
            float activationProgressDepth = ActivationProgressDepth;

            if (_hasActivatedThisContact && _visuals.PressDepth01 <= ResolvedSettings.ReleaseDepth)
            {
                _hasActivatedThisContact = false;
                _activationArmedForContact = false;
                Released?.Invoke(this);
            }

            if (!_hasActivatedThisContact &&
                _activationArmedForContact &&
                activationProgressDepth >= activationDepth - 0.0001f)
            {
                _hasActivatedThisContact = true;
                _activationArmedForContact = false;
                Activated?.Invoke(this);
            }
        }

        private float RequiredActivationDepth => Mathf.Clamp01(Mathf.Max(ActivationDepth, _isPointerFallbackActive ? 1f : 0f));
        private float ActivationProgressDepth => _visuals.PressDepth01;

        private void EndPointerFallbackClaim()
        {
            if (_pointerFallbackPointerId == int.MinValue)
            {
                return;
            }

            CustomPressablePointerContactRegistry.End(_pointerFallbackPointerId, this);
            _pointerFallbackPointerId = int.MinValue;
        }

        private void EndPokeContactClaim()
        {
            if (_pokeContactClaimKey == null)
            {
                return;
            }

            CustomPressablePokeContactRegistry.End(_pokeContactClaimKey, this);
            _pokeContactClaimKey = null;
        }

        private void RefreshDisabledPressVisualState()
        {
            if (!ShouldShowDisabledPressVisual())
            {
                return;
            }

            _activationArmedForContact = false;
            if (!_hasActivatedThisContact)
            {
                return;
            }

            _hasActivatedThisContact = false;
            Released?.Invoke(this);
        }

        private bool ShouldShowDisabledPressVisual()
        {
            return _selectable != null &&
                   !CustomPressableSelectableUtility.IsInteractableForCustomFeedback(_selectable,
                                                                                    this,
                                                                                    ref _selectableOverride);
        }

        private bool UsesSettings(CustomButtonSettings changedSettings)
        {
            if (changedSettings == null)
            {
                return true;
            }

            return _settings != null
                           ? _settings == changedSettings
                           : changedSettings == CustomButtonSettings.Global;
        }

        private void RebindToPokeProviderIfActive()
        {
            if (isActiveAndEnabled)
            {
                _pokeTracker.Bind(this);
            }
        }

        private static class CustomPressablePointerContactRegistry
        {
            private struct PointerClaim
            {
                public CustomPressableSurface Owner;
                public float ClickTime;
            }

            private static readonly System.Collections.Generic.Dictionary<int, PointerClaim> ActiveClaims = new();

            public static bool TryBegin(int pointerId, float clickTime, CustomPressableSurface owner)
            {
                if (ActiveClaims.TryGetValue(pointerId, out PointerClaim claim))
                {
                    if (claim.Owner == owner)
                    {
                        return true;
                    }

                    if (clickTime <= claim.ClickTime + 0.0001f)
                    {
                        return false;
                    }
                }

                ActiveClaims[pointerId] = new PointerClaim
                {
                        Owner = owner,
                        ClickTime = clickTime,
                };
                return true;
            }

            public static void End(int pointerId)
            {
                ActiveClaims.Remove(pointerId);
            }

            public static void End(int pointerId, CustomPressableSurface owner)
            {
                if (!ActiveClaims.TryGetValue(pointerId, out PointerClaim claim) || claim.Owner != owner)
                {
                    return;
                }

                ActiveClaims.Remove(pointerId);
            }
        }

        private static class CustomPressablePokeContactRegistry
        {
            private struct PokeClaim
            {
                public CustomPressableSurface Owner;
            }

            private static readonly System.Collections.Generic.Dictionary<object, PokeClaim> ActiveClaims =
                    new(ReferenceComparer.Instance);

            public static bool TryBegin(object contactKey, CustomPressableSurface owner)
            {
                if (contactKey == null)
                {
                    return false;
                }

                if (ActiveClaims.TryGetValue(contactKey, out PokeClaim claim))
                {
                    if (claim.Owner == owner)
                    {
                        return true;
                    }

                    if (IsContactActive(contactKey))
                    {
                        return false;
                    }

                    ActiveClaims.Remove(contactKey);
                }

                ActiveClaims[contactKey] = new PokeClaim
                {
                        Owner = owner,
                };
                return true;
            }

            public static void End(object contactKey, CustomPressableSurface owner)
            {
                if (contactKey == null ||
                    !ActiveClaims.TryGetValue(contactKey, out PokeClaim claim) ||
                    claim.Owner != owner)
                {
                    return;
                }

                if (IsContactActive(contactKey))
                {
                    return;
                }

                ActiveClaims.Remove(contactKey);
            }

            public static void ClearIfReleased(object contactKey)
            {
                if (contactKey == null || IsContactActive(contactKey))
                {
                    return;
                }

                ActiveClaims.Remove(contactKey);
            }

            public static string DescribeOwner(object contactKey)
            {
                if (contactKey == null ||
                    !ActiveClaims.TryGetValue(contactKey, out PokeClaim claim) ||
                    claim.Owner == null)
                {
                    return "<none>";
                }

                return GetPath(claim.Owner.transform);
            }

            private static bool IsContactActive(object contactKey)
            {
                return contactKey is IPokeStateDataProvider provider &&
                       provider.pokeStateData?.Value.interactionStrength > 0f;
            }

            private static string GetPath(Transform transform)
            {
                if (transform == null)
                {
                    return "<null>";
                }

                var names = new System.Collections.Generic.List<string>();
                Transform current = transform;
                while (current != null)
                {
                    names.Add(current.name);
                    current = current.parent;
                }

                names.Reverse();
                return string.Join("/", names);
            }

            private sealed class ReferenceComparer : System.Collections.Generic.IEqualityComparer<object>
            {
                public static readonly ReferenceComparer Instance = new();

                public new bool Equals(object x, object y)
                {
                    return ReferenceEquals(x, y);
                }

                public int GetHashCode(object obj)
                {
                    return RuntimeHelpers.GetHashCode(obj);
                }
            }
        }

#if UNITY_EDITOR
        private static void DrawRaycastPaddingGizmo(RectTransform rectTransform, Vector4 padding)
        {
            Rect rect = rectTransform.rect;
            Vector3[] baseCorners =
            {
                    rectTransform.TransformPoint(new Vector3(rect.xMin, rect.yMin, 0f)),
                    rectTransform.TransformPoint(new Vector3(rect.xMin, rect.yMax, 0f)),
                    rectTransform.TransformPoint(new Vector3(rect.xMax, rect.yMax, 0f)),
                    rectTransform.TransformPoint(new Vector3(rect.xMax, rect.yMin, 0f)),
                    rectTransform.TransformPoint(new Vector3(rect.xMin, rect.yMin, 0f)),
            };

            Rect padded = new Rect(rect.xMin - padding.x,
                                   rect.yMin - padding.y,
                                   rect.width + padding.x + padding.z,
                                   rect.height + padding.y + padding.w);
            Vector3[] paddedCorners =
            {
                    rectTransform.TransformPoint(new Vector3(padded.xMin, padded.yMin, 0f)),
                    rectTransform.TransformPoint(new Vector3(padded.xMin, padded.yMax, 0f)),
                    rectTransform.TransformPoint(new Vector3(padded.xMax, padded.yMax, 0f)),
                    rectTransform.TransformPoint(new Vector3(padded.xMax, padded.yMin, 0f)),
                    rectTransform.TransformPoint(new Vector3(padded.xMin, padded.yMin, 0f)),
            };

            UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.35f);
            UnityEditor.Handles.DrawAAPolyLine(1.5f, baseCorners);
            UnityEditor.Handles.color = new Color(0f, 0.9f, 1f, 0.85f);
            UnityEditor.Handles.DrawAAPolyLine(3f, paddedCorners);
        }
#endif
    }
}
