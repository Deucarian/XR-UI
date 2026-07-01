using UnityEngine;
using UnityEngine.UI;

namespace Deucarian.XRUI.Scrollbars
{
    /// <summary>
    ///     Custom scrollbar adapter that syncs a ScrollRect with a Slider.
    ///     Supports both horizontal and vertical scrolling.
    /// </summary>
    public class ScrollRectSliderAdapter : MonoBehaviour
    {
        #region Constants and Fields
        private const int DEFAULT_CONTENT_HORIZONTAL_PADDING = 10;
        private const int DEFAULT_CONTENT_VERTICAL_SAFE_PADDING = 2;
        private bool _sliderVisualVisibilityApplied;
        private bool _lastSliderVisualVisibility;
        private bool _updating;
        #endregion

        #region Serialized Fields
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Slider _slider;
        [SerializeField] private bool _horizontal;
        [SerializeField] private bool _disableDirectScrollRectInput = true;
        [SerializeField] [Min(0)] private int _contentHorizontalPadding = DEFAULT_CONTENT_HORIZONTAL_PADDING;
        [SerializeField] [Min(0)] private int _contentVerticalSafePadding = DEFAULT_CONTENT_VERTICAL_SAFE_PADDING;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            ApplyScrollRectPolicies();

            if (_scrollRect == null || _slider == null)
            {
                return;
            }

            _slider.minValue = 0f;
            _slider.maxValue = 1f;
            _slider.wholeNumbers = false;

            _slider.onValueChanged.AddListener(OnSliderValueChanged);
            _scrollRect.onValueChanged.AddListener(OnScrollRectChanged);

            SyncFromScrollRect(_scrollRect.normalizedPosition);
            UpdateVisibility();
        }

        private void OnDestroy()
        {
            if (_scrollRect == null || _slider == null)
            {
                return;
            }

            _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            _scrollRect.onValueChanged.RemoveListener(OnScrollRectChanged);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _contentHorizontalPadding = Mathf.Max(0, _contentHorizontalPadding);
            _contentVerticalSafePadding = Mathf.Max(0, _contentVerticalSafePadding);
            ApplyScrollRectPolicies();
        }
#endif
        #endregion

        #region Public Methods
        public void ApplyScrollRectPolicies()
        {
            ApplyDirectScrollRectInputPolicy();
            ApplyContentLayoutPadding();
        }

        public void ApplyDirectScrollRectInputPolicy()
        {
            if (_scrollRect == null)
            {
                return;
            }

            _scrollRect.StopMovement();

            if (_disableDirectScrollRectInput)
            {
                _scrollRect.horizontal = false;
                _scrollRect.vertical = false;
                return;
            }

            _scrollRect.horizontal = _horizontal;
            _scrollRect.vertical = !_horizontal;
        }

        public void ApplyContentLayoutPadding()
        {
            if (_scrollRect == null || _scrollRect.content == null)
            {
                return;
            }

            LayoutGroup layoutGroup = _scrollRect.content.GetComponent<LayoutGroup>();
            if (layoutGroup == null)
            {
                return;
            }

            RectOffset padding = layoutGroup.padding ?? new RectOffset();
            int horizontalPadding = Mathf.Max(0, _contentHorizontalPadding);
            int verticalSafePadding = Mathf.Max(0, _contentVerticalSafePadding);
            if (padding.left == horizontalPadding &&
                padding.right == horizontalPadding &&
                padding.top == verticalSafePadding &&
                padding.bottom == verticalSafePadding)
            {
                return;
            }

            padding.left = horizontalPadding;
            padding.right = horizontalPadding;
            padding.top = verticalSafePadding;
            padding.bottom = verticalSafePadding;
            layoutGroup.padding = padding;
        }
        #endregion

        #region Private Methods
        private void OnSliderValueChanged(float value)
        {
            if (_updating)
            {
                return;
            }

            _updating = true;
            if (_horizontal)
            {
                _scrollRect.horizontalNormalizedPosition = value;
            }
            else
            {
                _scrollRect.verticalNormalizedPosition = 1f - value; // Unity vertical is top=1, bottom=0
            }
            _updating = false;
        }

        private void OnScrollRectChanged(Vector2 normalizedPos)
        {
            if (_updating)
            {
                return;
            }

            _updating = true;
            float value = _horizontal ? normalizedPos.x : 1f - normalizedPos.y;
            _slider.value = value;
            _updating = false;
            UpdateVisibility();
        }

        private void SyncFromScrollRect(Vector2 normalizedPos)
        {
            float value = _horizontal ? normalizedPos.x : 1f - normalizedPos.y;
            _slider.SetValueWithoutNotify(value);
        }
        
        private void UpdateVisibility()
        {
            var content = _scrollRect.content;
            var viewport = _scrollRect.viewport;
            if (content == null || viewport == null)
            {
                return;
            }

            if (_horizontal)
            {
                SetSliderVisualVisibility(content.rect.width > viewport.rect.width);
            }
            else
            {
                SetSliderVisualVisibility(content.rect.height > viewport.rect.height);
            }
        }

        private void SetSliderVisualVisibility(bool visible)
        {
            if (_slider == null)
            {
                return;
            }

            bool wasActive = _slider.gameObject.activeSelf;
            if (_sliderVisualVisibilityApplied && _lastSliderVisualVisibility == visible && wasActive)
            {
                return;
            }

            if (!wasActive)
            {
                _slider.gameObject.SetActive(true);
            }

            if (_slider.interactable != visible)
            {
                _slider.interactable = visible;
            }

            foreach (Graphic graphic in _slider.GetComponentsInChildren<Graphic>(true))
            {
                if (graphic.enabled != visible)
                {
                    graphic.enabled = visible;
                }
            }

            _lastSliderVisualVisibility = visible;
            _sliderVisualVisibilityApplied = true;
        }
        #endregion
    }
}
