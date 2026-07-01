using System.Collections.Generic;
using Unity.XR.CoreUtils.Bindings;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace Deucarian.XRUI.Controls
{
    internal sealed class CustomPressablePokeTracker
    {
        private readonly BindingsGroup _bindingsGroup = new();
        private readonly Dictionary<Transform, float> _pokeDepthByTarget = new();
        private readonly List<Transform> _emptyPokeTargets = new();
        private object _contactKey;

        public bool IsTargeted { get; private set; }
        public float PokeDepth01 { get; private set; }

        public void Bind(CustomPressableSurface owner)
        {
            ClearBindings();
            _contactKey = null;

            if (!owner.isActiveAndEnabled)
            {
                return;
            }

            IMultiPokeStateDataProvider multiProvider = owner.GetComponentInParent<IMultiPokeStateDataProvider>();
            if (multiProvider != null)
            {
                _contactKey = multiProvider;
                if (multiProvider is IPokeStateDataProvider contactProvider &&
                    contactProvider.pokeStateData != null)
                {
                    _bindingsGroup.AddBinding(contactProvider.pokeStateData.SubscribeAndUpdate(data => OnContactStateDataUpdated(owner, data)));
                }

                BindMultiPokeProvider(owner, multiProvider);
                return;
            }

            IPokeStateDataProvider provider = owner.GetComponentInParent<IPokeStateDataProvider>();
            if (provider?.pokeStateData != null)
            {
                _contactKey = provider;
                _bindingsGroup.AddBinding(provider.pokeStateData.SubscribeAndUpdate(data => OnContactStateDataUpdated(owner, data)));
                _bindingsGroup.AddBinding(provider.pokeStateData.SubscribeAndUpdate(data => OnPokeStateDataUpdated(owner, owner.transform, data)));
            }
        }

        public void ClearBindings()
        {
            _bindingsGroup.Clear();
            ClearContact();
        }

        public void ClearContact()
        {
            _pokeDepthByTarget.Clear();
            _emptyPokeTargets.Clear();
            IsTargeted = false;
            PokeDepth01 = 0f;
        }

        private void BindMultiPokeProvider(CustomPressableSurface owner, IMultiPokeStateDataProvider multiProvider)
        {
            var boundTargets = new HashSet<Transform>();
            AddPokeTargetBinding(owner, multiProvider, owner.transform, boundTargets);

            Graphic[] graphics = owner.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                if (graphics[i] == null || owner.IsOwnedByNestedSelectable(graphics[i].transform))
                {
                    continue;
                }

                AddPokeTargetBinding(owner, multiProvider, graphics[i].transform, boundTargets);
            }

            AddPokeTargetBinding(owner, multiProvider, owner.PressableVisualRoot, boundTargets);
        }

        private void AddPokeTargetBinding(CustomPressableSurface owner,
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

        private void OnPokeStateDataUpdated(CustomPressableSurface owner, Transform boundTarget, PokeStateData data)
        {
            bool isPokeStateForSurface = IsPokeStateForSurface(owner, data);
            if (isPokeStateForSurface && data.interactionStrength > 0f)
            {
                _pokeDepthByTarget[boundTarget] = Mathf.Clamp01(data.interactionStrength);
                RefreshPokeDepthFromTargets(owner);
                return;
            }

            _pokeDepthByTarget.Remove(boundTarget);
            RefreshPokeDepthFromTargets(owner);
        }

        private void OnContactStateDataUpdated(CustomPressableSurface owner, PokeStateData data)
        {
            if (data.interactionStrength <= 0f)
            {
                owner.ClearReleasedPokeContact(_contactKey);
            }
        }

        private void RefreshPokeDepthFromTargets(CustomPressableSurface owner)
        {
            bool wasTargeted = IsTargeted;
            float maxDepth = 0f;
            _emptyPokeTargets.Clear();

            foreach (KeyValuePair<Transform, float> pair in _pokeDepthByTarget)
            {
                if (pair.Key == null)
                {
                    _emptyPokeTargets.Add(pair.Key);
                    continue;
                }

                maxDepth = Mathf.Max(maxDepth, pair.Value);
            }

            for (int i = 0; i < _emptyPokeTargets.Count; i++)
            {
                _pokeDepthByTarget.Remove(_emptyPokeTargets[i]);
            }

            _emptyPokeTargets.Clear();
            PokeDepth01 = maxDepth;
            IsTargeted = _pokeDepthByTarget.Count > 0;

            if (IsTargeted)
            {
                if (!wasTargeted)
                {
                    owner.BeginPokeContact(_contactKey);
                }

                owner.ApplyResolvedPressDepth();
                return;
            }

            owner.ReleaseContact(false);
            owner.ClearReleasedPokeContact(_contactKey);
        }

        private static bool IsPokeStateForSurface(CustomPressableSurface owner, PokeStateData data)
        {
            Transform pokeTarget = data.target;
            return pokeTarget != null &&
                   (pokeTarget == owner.transform || pokeTarget.IsChildOf(owner.transform));
        }

    }
}
