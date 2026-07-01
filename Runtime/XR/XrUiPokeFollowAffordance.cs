using System.Collections.Generic;
using Unity.XR.CoreUtils.Bindings;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace Deucarian.XRUI.XR
{
    [DisallowMultipleComponent]
    public sealed class XrUiPokeFollowAffordance : MonoBehaviour
    {
        [SerializeField]
        RectTransform m_PokeFollowTransform;

        [SerializeField]
        float m_SmoothingSpeed = 16f;

        [SerializeField]
        bool m_ReturnToInitialPosition = true;

        [SerializeField]
        bool m_ApplyIfChildIsTarget = true;

        [SerializeField]
        bool m_ClampToMaxDistance = true;

        [SerializeField]
        float m_MaxDistance = 20f;

        readonly BindingsGroup m_BindingsGroup = new();

        Vector3 m_InitialPosition;
        Vector3 m_TargetPosition;
        bool m_HasInitialPosition;

        public RectTransform PokeFollowTransform
        {
            get => m_PokeFollowTransform;
            set
            {
                if (m_PokeFollowTransform == value)
                    return;

                m_PokeFollowTransform = value;
                CacheInitialPosition();
                BindToPokeProvider();
            }
        }

        void Awake()
        {
            TryAutoAssignFollowTransform();
            CacheInitialPosition();
        }

        void OnEnable()
        {
            TryAutoAssignFollowTransform();
            CacheInitialPosition();
            BindToPokeProvider();
        }

        void OnDisable()
        {
            m_BindingsGroup.Clear();

            if (m_ReturnToInitialPosition && m_PokeFollowTransform != null && m_HasInitialPosition)
                m_PokeFollowTransform.localPosition = m_InitialPosition;
        }

        void OnDestroy()
        {
            m_BindingsGroup.Clear();
        }

        void LateUpdate()
        {
            if (m_PokeFollowTransform == null || !m_HasInitialPosition)
                return;

            if (m_SmoothingSpeed <= 0f)
            {
                m_PokeFollowTransform.localPosition = m_TargetPosition;
                return;
            }

            var t = Mathf.Clamp01(Time.deltaTime * m_SmoothingSpeed);
            m_PokeFollowTransform.localPosition = Vector3.Lerp(m_PokeFollowTransform.localPosition, m_TargetPosition, t);
        }

        void TryAutoAssignFollowTransform()
        {
            if (m_PokeFollowTransform != null)
                return;

            for (var i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i) is RectTransform child)
                {
                    m_PokeFollowTransform = child;
                    return;
                }
            }
        }

        void CacheInitialPosition()
        {
            if (m_PokeFollowTransform == null)
                return;

            m_InitialPosition = m_PokeFollowTransform.localPosition;
            m_TargetPosition = m_InitialPosition;
            m_HasInitialPosition = true;
        }

        void BindToPokeProvider()
        {
            m_BindingsGroup.Clear();

            if (!isActiveAndEnabled || m_PokeFollowTransform == null)
                return;

            var multiProvider = GetComponentInParent<IMultiPokeStateDataProvider>();
            if (multiProvider != null)
            {
                var boundTargets = new HashSet<Transform>();
                AddPokeTargetBinding(multiProvider, transform, boundTargets);

                var graphics = GetComponentsInChildren<Graphic>(true);
                for (var i = 0; i < graphics.Length; i++)
                {
                    if (graphics[i] == null || !graphics[i].raycastTarget)
                        continue;

                    AddPokeTargetBinding(multiProvider, graphics[i].transform, boundTargets);
                }

                return;
            }

            var provider = GetComponentInParent<IPokeStateDataProvider>();
            if (provider?.pokeStateData != null)
                m_BindingsGroup.AddBinding(provider.pokeStateData.SubscribeAndUpdate(OnPokeStateDataUpdated));
        }

        void AddPokeTargetBinding(IMultiPokeStateDataProvider multiProvider, Transform target, HashSet<Transform> boundTargets)
        {
            if (multiProvider == null || target == null || !boundTargets.Add(target))
                return;

            m_BindingsGroup.AddBinding(multiProvider.GetPokeStateDataForTarget(target).SubscribeAndUpdate(OnPokeStateDataUpdated));
        }

        void OnPokeStateDataUpdated(PokeStateData data)
        {
            if (m_PokeFollowTransform == null)
                return;

            if (!m_HasInitialPosition)
                CacheInitialPosition();

            var pokeTarget = data.target;
            var applyFollow = m_ApplyIfChildIsTarget
                ? pokeTarget != null && pokeTarget.IsChildOf(transform)
                : pokeTarget == transform;

            if (applyFollow)
            {
                var localPokePosition = transform.InverseTransformPoint(data.axisAlignedPokeInteractionPoint);
                var nextPosition = m_InitialPosition;
                var deltaZ = localPokePosition.z - m_InitialPosition.z;

                if (m_ClampToMaxDistance)
                    deltaZ = Mathf.Clamp(deltaZ, -m_MaxDistance, m_MaxDistance);

                nextPosition.z = m_InitialPosition.z + deltaZ;
                m_TargetPosition = nextPosition;
            }
            else if (m_ReturnToInitialPosition)
            {
                m_TargetPosition = m_InitialPosition;
            }
        }
    }
}
