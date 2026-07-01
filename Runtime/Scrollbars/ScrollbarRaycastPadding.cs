using Deucarian.XRUI.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace Deucarian.XRUI.Scrollbars
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class ScrollbarRaycastPadding : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private Slider _slider;
        [SerializeField] private CustomPressableSurface _pressableSurface;
        [SerializeField] [Min(0f)] private float _minimumHitThickness = 15f;
        #endregion

        #region Unity Methods
        private void Reset()
        {
            ResolveReferences();
            Apply();
        }

        private void Awake()
        {
            Apply();
        }

        private void OnEnable()
        {
            Apply();
        }

        private void LateUpdate()
        {
            Apply();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _minimumHitThickness = Mathf.Max(0f, _minimumHitThickness);
            ResolveReferences();
            Apply();
        }
#endif
        #endregion

        #region Public Methods
        public void Apply()
        {
            ResolveReferences();

            RectTransform sliderRect = (_slider != null ? _slider.transform : transform) as RectTransform;
            Graphic socketGraphic = _pressableSurface != null ? _pressableSurface.SocketGraphic : null;
            if (sliderRect == null || socketGraphic == null)
            {
                return;
            }

            bool horizontal = IsHorizontal(_slider);
            float rootThickness = GetCrossAxisSize(sliderRect, horizontal);
            float targetThickness = Mathf.Max(rootThickness, _minimumHitThickness, GetMaxVisibleThickness(sliderRect, horizontal));
            float padding = Mathf.Max(0f, (targetThickness - rootThickness) * 0.5f);

            Vector4 raycastPadding = horizontal
                                             ? new Vector4(0f, padding, 0f, padding)
                                             : new Vector4(padding, 0f, padding, 0f);
            socketGraphic.raycastPadding = CustomButtonSettings.ToUnityRaycastPadding(raycastPadding);
            _pressableSurface.RaycastPadding = raycastPadding;
        }
        #endregion

        #region Private Methods
        private void ResolveReferences()
        {
            if (_slider == null)
            {
                _slider = GetComponent<Slider>();
            }

            if (_pressableSurface == null)
            {
                _pressableSurface = GetComponent<CustomPressableSurface>();
            }
        }

        private static bool IsHorizontal(Slider slider)
        {
            if (slider == null)
            {
                return false;
            }

            return slider.direction == Slider.Direction.LeftToRight ||
                   slider.direction == Slider.Direction.RightToLeft;
        }

        private static float GetMaxVisibleThickness(RectTransform root, bool horizontal)
        {
            float maxThickness = GetCrossAxisSize(root, horizontal);
            Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                if (graphics[i] == null || graphics[i].transform is not RectTransform rectTransform)
                {
                    continue;
                }

                maxThickness = Mathf.Max(maxThickness, GetCrossAxisSize(rectTransform, horizontal));
            }

            return maxThickness;
        }

        private static float GetCrossAxisSize(RectTransform rectTransform, bool horizontal)
        {
            Rect rect = rectTransform.rect;
            return horizontal ? rect.height : rect.width;
        }
        #endregion
    }
}
