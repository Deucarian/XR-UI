using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Deucarian.XRUI.Controls
{
    internal sealed class CustomPressableSurfaceVisuals
    {
        private Vector3 _initialVisualLocalPosition;
        private Vector3 _targetVisualLocalPosition;
        private Graphic _cachedSocketGraphic;
        private Color _cachedSocketRestColor;
        private bool _hasCachedSocketRestColor;

        public bool HasInitialPosition { get; private set; }
        public float PressDepth01 { get; private set; }
        public float TargetPressDepth01 { get; private set; }
        public float Hover01 { get; private set; }
        public float SelectedHoldDepth01 { get; private set; }

        public void ResetSocketColorCache()
        {
            RestoreSocketRestColor();
            _cachedSocketGraphic = null;
            _hasCachedSocketRestColor = false;
        }

        public void SetSocketBaseColor(CustomPressableSurface owner, Color baseColor)
        {
            if (owner.SocketGraphic == null)
            {
                return;
            }

            _cachedSocketGraphic = owner.SocketGraphic;
            _cachedSocketRestColor = ResolveSocketGraphicColor(owner, owner.SocketGraphic);
            _hasCachedSocketRestColor = true;
            ApplySocketPressColor(owner, owner.SocketGraphic);
        }

        public void SetSelectedHoldDepth(CustomPressableSurface owner, float depth01)
        {
            depth01 = Mathf.Clamp01(depth01);
            if (Mathf.Approximately(SelectedHoldDepth01, depth01))
            {
                return;
            }

            SelectedHoldDepth01 = depth01;
            if (owner.PressableVisualRoot == null)
            {
                owner.EnsureHierarchy();
            }

            if (owner.PressableVisualRoot == null)
            {
                return;
            }

            CacheInitialPosition(owner, false);
            RefreshTargetPosition(owner, false, false);
        }

        public void ClearSelectedHoldDepth()
        {
            SelectedHoldDepth01 = 0f;
        }

        public void SetContactDepth(CustomPressableSurface owner, float depth01)
        {
            TargetPressDepth01 = Mathf.Clamp01(depth01);
            Hover01 = TargetPressDepth01;
            ApplySocketPressColor(owner, owner.SocketGraphic);
        }

        public void ApplySocketPressColor(CustomPressableSurface owner, Graphic graphic)
        {
            if (graphic == null)
            {
                return;
            }

            if (_cachedSocketGraphic != graphic || !_hasCachedSocketRestColor)
            {
                _cachedSocketGraphic = graphic;
                _hasCachedSocketRestColor = true;
            }

            _cachedSocketRestColor = ResolveSocketGraphicColor(owner, graphic);
            graphic.color = _cachedSocketRestColor;
        }

        public void RestoreSocketRestColor()
        {
            if (_cachedSocketGraphic != null && _hasCachedSocketRestColor)
            {
                _cachedSocketGraphic.color = _cachedSocketRestColor;
            }
        }

        public void CacheInitialPosition(CustomPressableSurface owner, bool force)
        {
            if (owner.PressableVisualRoot == null || (HasInitialPosition && !force))
            {
                return;
            }

            _initialVisualLocalPosition = owner.PressableVisualRoot.localPosition;
            _targetVisualLocalPosition = _initialVisualLocalPosition;
            HasInitialPosition = true;
        }

        public void RefreshTargetPosition(CustomPressableSurface owner, bool instant, bool showDisabledPressVisual)
        {
            if (!HasInitialPosition)
            {
                return;
            }

            _initialVisualLocalPosition.z = -owner.PressDepthDistance;
            _targetVisualLocalPosition = _initialVisualLocalPosition;

            float visualPressDepth = showDisabledPressVisual ? 1f : Mathf.Max(TargetPressDepth01, SelectedHoldDepth01);
            _targetVisualLocalPosition.z += owner.PressDepthDistance * visualPressDepth;

            if (instant)
            {
                ApplyTargetPositionImmediately(owner, visualPressDepth);
            }
        }

        public void MoveVisualRootTowardTarget(CustomPressableSurface owner)
        {
            float smoothSpeed = owner.ResolvedSettings.PressSmoothSpeed;
            owner.PressableVisualRoot.localPosition = smoothSpeed <= 0f
                                                             ? _targetVisualLocalPosition
                                                             : Vector3.Lerp(owner.PressableVisualRoot.localPosition,
                                                                            _targetVisualLocalPosition,
                                                                            Mathf.Clamp01(Time.unscaledDeltaTime * smoothSpeed));
        }

        public void PublishCurrentDepthIfChanged(CustomPressableSurface owner)
        {
            float nextDepth = Mathf.Clamp01((owner.PressableVisualRoot.localPosition.z - _initialVisualLocalPosition.z) /
                                            owner.PressDepthDistance);
            if (Mathf.Approximately(nextDepth, PressDepth01))
            {
                return;
            }

            PressDepth01 = nextDepth;
            ApplySocketPressColor(owner, owner.SocketGraphic);
            owner.RaisePressDepthChanged(PressDepth01);
        }

        public void ResetToRestPosition(CustomPressableSurface owner)
        {
            if (owner.PressableVisualRoot != null && HasInitialPosition)
            {
                owner.PressableVisualRoot.localPosition = _initialVisualLocalPosition;
            }

            TargetPressDepth01 = 0f;
            Hover01 = 0f;
            PressDepth01 = 0f;
            ApplySocketPressColor(owner, owner.SocketGraphic);
        }

#if UNITY_EDITOR
        public void RefreshEditorVisualState(CustomPressableSurface owner)
        {
            if (owner.PressableVisualRoot == null)
            {
                return;
            }

            if (!HasInitialPosition)
            {
                CacheInitialPosition(owner, false);
            }

            RefreshTargetPosition(owner, true, false);
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }
#endif

        private void ApplyTargetPositionImmediately(CustomPressableSurface owner, float visualPressDepth)
        {
            if (owner.PressableVisualRoot == null)
            {
                return;
            }

            owner.PressableVisualRoot.localPosition = _targetVisualLocalPosition;
            PressDepth01 = visualPressDepth;
            ApplySocketPressColor(owner, owner.SocketGraphic);
            owner.RaisePressDepthChanged(PressDepth01);
        }

        private static Color ResolveSocketGraphicColor(CustomPressableSurface owner, Graphic graphic)
        {
            return graphic is Image
                    ? CustomPressableSocketVisual.ResolveImageColor(owner.ResolvedSettings, owner.ResolvedVisualStyle)
                    : CustomPressableSocketVisual.ResolveColor(owner.ResolvedSettings, owner.ResolvedVisualStyle);
        }
    }
}
