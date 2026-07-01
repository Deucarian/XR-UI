using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Deucarian.XRUI.Controls
{
    public interface ICustomPressableSelectableVisualOverride
    {
        bool TreatSelectableAsInteractableForCustomFeedback { get; }
        bool TreatSelectableAsVisuallyInteractableForCustomFeedback { get; }
    }

    public interface ICustomPressActivationStatus
    {
        bool IsPressActivationSatisfied { get; }
    }

    public interface ICustomPressActivationTarget
    {
        bool CanActivatePress(CustomPressableSurface surface);
        void ActivateFromPress(CustomPressableSurface surface);
    }

    public interface ICustomPressReleaseTarget
    {
        void ReleaseFromPress(CustomPressableSurface surface);
    }

    public interface ICustomPressSelectedHoldTarget
    {
        bool ShouldHoldSelectedPress(CustomPressableSurface surface);
    }

    internal static class CustomPressableComponentUtility
    {
        public static CustomPressableSurface EnsurePressableFeedback(Component owner,
                                                                     ref CustomPressableSurface cachedSurface)
        {
            CustomPressableSurface surface = EnsurePressableSurface(owner, ref cachedSurface);
            EnsureSelectableFeedback(owner);
            return surface;
        }

        public static CustomPressableSurface EnsurePressableSurface(Component owner,
                                                                    ref CustomPressableSurface cachedSurface)
        {
            if (owner == null)
            {
                return null;
            }

            if (cachedSurface == null)
            {
                cachedSurface = owner.GetComponent<CustomPressableSurface>();
            }

            if (cachedSurface == null)
            {
                cachedSurface = owner.gameObject.AddComponent<CustomPressableSurface>();
            }

            return cachedSurface;
        }

        public static void EnsureSelectableFeedback(Component owner)
        {
            if (owner != null && owner.GetComponent<CustomSelectableFeedback>() == null)
            {
                owner.gameObject.AddComponent<CustomSelectableFeedback>();
            }
        }

        public static CustomTogglePressTarget EnsureTogglePressTarget(Toggle toggle)
        {
            if (toggle == null)
            {
                return null;
            }

            CustomTogglePressTarget target = toggle.GetComponent<CustomTogglePressTarget>();
            if (target == null)
            {
                target = toggle.gameObject.AddComponent<CustomTogglePressTarget>();
            }

            return target;
        }
    }

    internal static class CustomDropdownOptionUtility
    {
        public static void ConfigureOptionToggle(Toggle toggle)
        {
            CustomTogglePressTarget target = CustomPressableComponentUtility.EnsureTogglePressTarget(toggle);
            target?.Configure(CustomTogglePressTarget.ActivationMode.SelectOn);
        }
    }

    internal sealed class CustomPressActivationGate : ICustomPressActivationStatus
    {
        private static readonly List<MonoBehaviour> MonoBehaviours = new();
        private readonly MonoBehaviour _owner;
        private CustomPressableSurface _pressableSurface;
        private ICustomPressActivationTarget _activationTarget;
        private ICustomPressReleaseTarget _releaseTarget;
        private ICustomPressSelectedHoldTarget _selectedHoldTarget;
        private Coroutine _clearActivationCoroutine;
        private bool _activatingFromPress;
        private bool _pressActivationSatisfied;

        public CustomPressActivationGate(MonoBehaviour owner)
        {
            _owner = owner;
        }

        public bool IsPressActivationSatisfied => _activatingFromPress || _pressActivationSatisfied;

        public void Bind(CustomPressableSurface pressableSurface, ICustomPressActivationTarget activationTarget)
        {
            if (_pressableSurface == pressableSurface && ReferenceEquals(_activationTarget, activationTarget))
            {
                return;
            }

            Unbind();
            _pressableSurface = pressableSurface;
            _activationTarget = activationTarget;
            _releaseTarget = activationTarget as ICustomPressReleaseTarget;
            _selectedHoldTarget = activationTarget as ICustomPressSelectedHoldTarget;

            if (_pressableSurface == null || _activationTarget == null)
            {
                return;
            }

            _pressableSurface.Activated += HandlePressActivated;
            _pressableSurface.Released += HandlePressReleased;
        }

        public void Unbind()
        {
            if (_pressableSurface != null)
            {
                _pressableSurface.Activated -= HandlePressActivated;
                _pressableSurface.Released -= HandlePressReleased;
            }

            StopClearActivationCoroutine();
            _pressableSurface = null;
            _activationTarget = null;
            _releaseTarget = null;
            _selectedHoldTarget = null;
            ClearActivation();
        }

        public void ClearActivation()
        {
            StopClearActivationCoroutine();
            _activatingFromPress = false;
            _pressActivationSatisfied = false;
        }

        private void HandlePressActivated(CustomPressableSurface surface)
        {
            StopClearActivationCoroutine();
            _pressActivationSatisfied = false;

            bool canActivate = _activationTarget != null && _activationTarget.CanActivatePress(surface);
            if (!canActivate)
            {
                return;
            }

            _activatingFromPress = true;

            try
            {
                _activationTarget.ActivateFromPress(surface);
            }
            finally
            {
                _activatingFromPress = false;
            }

            _pressActivationSatisfied = true;

            ICustomPressSelectedHoldTarget selectedHoldTarget = ResolveSelectedHoldTarget(surface);
            if (selectedHoldTarget != null && selectedHoldTarget.ShouldHoldSelectedPress(surface))
            {
                surface.SetSelectedVisualHold(true);
            }
        }

        private void HandlePressReleased(CustomPressableSurface surface)
        {
            _releaseTarget?.ReleaseFromPress(surface);

            if (_owner == null || !_owner.isActiveAndEnabled)
            {
                _pressActivationSatisfied = false;
                return;
            }

            StopClearActivationCoroutine();
            _clearActivationCoroutine = _owner.StartCoroutine(ClearActivationAfterClickFrame());
        }

        private IEnumerator ClearActivationAfterClickFrame()
        {
            yield return null;
            _clearActivationCoroutine = null;
            _pressActivationSatisfied = false;
        }

        private void StopClearActivationCoroutine()
        {
            if (_clearActivationCoroutine == null || _owner == null)
            {
                _clearActivationCoroutine = null;
                return;
            }

            _owner.StopCoroutine(_clearActivationCoroutine);
            _clearActivationCoroutine = null;
        }

        private ICustomPressSelectedHoldTarget ResolveSelectedHoldTarget(CustomPressableSurface surface)
        {
            if (IsAlive(_selectedHoldTarget))
            {
                return _selectedHoldTarget;
            }

            _selectedHoldTarget = _activationTarget as ICustomPressSelectedHoldTarget;
            if (IsAlive(_selectedHoldTarget))
            {
                return _selectedHoldTarget;
            }

            _selectedHoldTarget = FindSelectedHoldTarget(_activationTarget as Component, _activationTarget);
            if (IsAlive(_selectedHoldTarget))
            {
                return _selectedHoldTarget;
            }

            _selectedHoldTarget = FindSelectedHoldTarget(surface, _activationTarget);
            return IsAlive(_selectedHoldTarget) ? _selectedHoldTarget : null;
        }

        private static ICustomPressSelectedHoldTarget FindSelectedHoldTarget(Component component, object excludedTarget)
        {
            if (component == null)
            {
                return null;
            }

            MonoBehaviours.Clear();
            component.GetComponents(MonoBehaviours);
            for (int i = 0; i < MonoBehaviours.Count; i++)
            {
                if (MonoBehaviours[i] is ICustomPressSelectedHoldTarget target &&
                    !ReferenceEquals(target, excludedTarget))
                {
                    MonoBehaviours.Clear();
                    return target;
                }
            }

            MonoBehaviours.Clear();
            return null;
        }

        private static bool IsAlive(ICustomPressSelectedHoldTarget target)
        {
            return target != null && (target is not UnityEngine.Object unityObject || unityObject != null);
        }
    }

    internal static class CustomPressableSelectableUtility
    {
        private static readonly List<CanvasGroup> CanvasGroups = new();
        private static readonly List<MonoBehaviour> MonoBehaviours = new();

        public static bool IsInteractableForCustomFeedback(Selectable selectable,
                                                           Component owner,
                                                           ref ICustomPressableSelectableVisualOverride cachedOverride)
        {
            if (selectable == null || selectable.IsInteractable())
            {
                return true;
            }

            if (selectable.interactable || !CanvasGroupsAllowInteraction(selectable.transform))
            {
                return false;
            }

            if (cachedOverride is UnityEngine.Object unityObject && unityObject == null)
            {
                cachedOverride = null;
            }

            cachedOverride ??= FindSelectableOverride(owner);
            cachedOverride ??= selectable != owner ? FindSelectableOverride(selectable) : null;
            return cachedOverride != null && cachedOverride.TreatSelectableAsInteractableForCustomFeedback;
        }

        public static bool IsVisuallyInteractableForCustomFeedback(Selectable selectable,
                                                                   Component owner,
                                                                   ref ICustomPressableSelectableVisualOverride cachedOverride)
        {
            if (selectable == null || selectable.IsInteractable())
            {
                return true;
            }

            if (selectable.interactable || !CanvasGroupsAllowInteraction(selectable.transform))
            {
                return false;
            }

            if (cachedOverride is UnityEngine.Object unityObject && unityObject == null)
            {
                cachedOverride = null;
            }

            cachedOverride ??= FindSelectableOverride(owner);
            cachedOverride ??= selectable != owner ? FindSelectableOverride(selectable) : null;
            return cachedOverride != null && cachedOverride.TreatSelectableAsVisuallyInteractableForCustomFeedback;
        }

        private static ICustomPressableSelectableVisualOverride FindSelectableOverride(Component owner)
        {
            if (owner == null)
            {
                return null;
            }

            MonoBehaviours.Clear();
            owner.GetComponents(MonoBehaviours);

            for (int i = 0; i < MonoBehaviours.Count; i++)
            {
                if (MonoBehaviours[i] is ICustomPressableSelectableVisualOverride selectableOverride)
                {
                    MonoBehaviours.Clear();
                    return selectableOverride;
                }
            }

            MonoBehaviours.Clear();
            return null;
        }

        private static bool CanvasGroupsAllowInteraction(Transform transform)
        {
            Transform current = transform;
            while (current != null)
            {
                CanvasGroups.Clear();
                current.GetComponents(CanvasGroups);

                bool ignoreParentGroups = false;
                for (int i = 0; i < CanvasGroups.Count; i++)
                {
                    CanvasGroup group = CanvasGroups[i];
                    if (group == null)
                    {
                        continue;
                    }

                    if (!group.interactable)
                    {
                        CanvasGroups.Clear();
                        return false;
                    }

                    ignoreParentGroups |= group.ignoreParentGroups;
                }

                CanvasGroups.Clear();

                if (ignoreParentGroups)
                {
                    return true;
                }

                current = current.parent;
            }

            return true;
        }
    }

    internal sealed class SelectablePressTargetState
    {
        private Selectable _selectable;
        private bool _cachedOriginalState;
        private bool _originalInteractable;
        private ColorBlock _originalColors;

        public bool OriginalInteractable => !_cachedOriginalState || _originalInteractable;

        public void Cache(Selectable selectable)
        {
            if (selectable == null || _cachedOriginalState && _selectable == selectable)
            {
                return;
            }

            _selectable = selectable;
            _originalInteractable = selectable.interactable;
            _originalColors = selectable.colors;
            _cachedOriginalState = true;
        }

        public void ApplyDisabledOverride(bool preserveEnabledVisuals)
        {
            if (_selectable == null)
            {
                return;
            }

            if (!_cachedOriginalState)
            {
                Cache(_selectable);
            }

            if (preserveEnabledVisuals)
            {
                ColorBlock colors = _selectable.colors;
                colors.disabledColor = _originalColors.normalColor;
                _selectable.colors = colors;
            }

            _selectable.interactable = false;
        }

        public void Restore()
        {
            if (_selectable == null || !_cachedOriginalState)
            {
                return;
            }

            _selectable.colors = _originalColors;
            _selectable.interactable = _originalInteractable;
        }
    }
}
