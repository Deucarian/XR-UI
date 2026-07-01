using System.Collections.Generic;
using Unity.XR.CoreUtils.Bindings;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace Deucarian.XRUI.Controls
{
    internal sealed class CustomSelectablePokeTracker
    {
        private readonly BindingsGroup _bindingsGroup = new();
        private readonly Dictionary<Transform, float> _pokeProximityByTarget = new();
        private readonly List<Transform> _emptyPokeTargets = new();

        public bool IsInRange { get; private set; }
        public float Proximity { get; private set; }

        public void Bind(CustomSelectableFeedback owner)
        {
            ClearBindings();

            if (!owner.isActiveAndEnabled)
            {
                return;
            }

            IMultiPokeStateDataProvider multiProvider = owner.GetComponentInParent<IMultiPokeStateDataProvider>();
            if (multiProvider != null)
            {
                BindMultiPokeProvider(owner, multiProvider);
                return;
            }

            IPokeStateDataProvider provider = owner.GetComponentInParent<IPokeStateDataProvider>();
            if (provider?.pokeStateData != null)
            {
                _bindingsGroup.AddBinding(provider.pokeStateData.SubscribeAndUpdate(data => OnPokeStateDataUpdated(owner, owner.transform, data)));
            }
        }

        public void ClearBindings()
        {
            _bindingsGroup.Clear();
            ClearProximity();
        }

        public void ClearProximity()
        {
            _pokeProximityByTarget.Clear();
            _emptyPokeTargets.Clear();
            IsInRange = false;
            Proximity = 0f;
        }

        private void BindMultiPokeProvider(CustomSelectableFeedback owner, IMultiPokeStateDataProvider multiProvider)
        {
            var boundTargets = new HashSet<Transform>();
            AddPokeTargetBinding(owner, multiProvider, owner.transform, boundTargets);
            AddPokeTargetBinding(owner, multiProvider, owner.PressableSurface != null ? owner.PressableSurface.PressableVisualRoot : null, boundTargets);
            AddPokeTargetBinding(owner, multiProvider, owner.TargetGraphic != null ? owner.TargetGraphic.transform : null, boundTargets);
            AddPokeTargetBinding(owner, multiProvider, owner.AnimationTarget, boundTargets);

            Graphic[] graphics = owner.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                if (graphics[i] == null || owner.IsOwnedByNestedSelectable(graphics[i].transform))
                {
                    continue;
                }

                AddPokeTargetBinding(owner, multiProvider, graphics[i].transform, boundTargets);
            }
        }

        private void AddPokeTargetBinding(CustomSelectableFeedback owner,
                                          IMultiPokeStateDataProvider multiProvider,
                                          Transform target,
                                          HashSet<Transform> boundTargets)
        {
            if (multiProvider == null || target == null || !boundTargets.Add(target))
            {
                return;
            }

            _bindingsGroup.AddBinding(multiProvider.GetPokeStateDataForTarget(target)
                                                   .SubscribeAndUpdate(data => OnPokeStateDataUpdated(owner, target, data)));
        }

        private void OnPokeStateDataUpdated(CustomSelectableFeedback owner, Transform boundTarget, PokeStateData data)
        {
            if (!IsPokeStateForSelectable(owner, data))
            {
                _pokeProximityByTarget.Remove(boundTarget);
                RefreshPokeProximityFromTargets();
                return;
            }

            _pokeProximityByTarget[boundTarget] = 1f;
            RefreshPokeProximityFromTargets();
            owner.RegisterTransientOwner();
        }

        private void RefreshPokeProximityFromTargets()
        {
            float maxProximity = 0f;
            _emptyPokeTargets.Clear();

            foreach (KeyValuePair<Transform, float> pair in _pokeProximityByTarget)
            {
                if (pair.Key == null)
                {
                    _emptyPokeTargets.Add(pair.Key);
                    continue;
                }

                maxProximity = Mathf.Max(maxProximity, pair.Value);
            }

            for (int i = 0; i < _emptyPokeTargets.Count; i++)
            {
                _pokeProximityByTarget.Remove(_emptyPokeTargets[i]);
            }

            _emptyPokeTargets.Clear();
            Proximity = maxProximity;
            IsInRange = _pokeProximityByTarget.Count > 0 && Proximity > 0f;
        }

        private static bool IsPokeStateForSelectable(CustomSelectableFeedback owner, PokeStateData data)
        {
            Transform pokeTarget = data.target;
            return pokeTarget != null &&
                   (pokeTarget == owner.transform || pokeTarget.IsChildOf(owner.transform));
        }
    }
}
