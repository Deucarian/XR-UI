using UnityEngine;
using UnityEngine.UI;

namespace Deucarian.XRUI.Scrollbars
{
    /// <summary>
    ///     sets the size of a scrollbar handle based on the ratio of viewport to content size.
    /// </summary>
    public class ScrollHandleSizer : MonoBehaviour
    {
        #region Fields
        private bool _sliderVisualVisibilityApplied;
        private bool _lastSliderVisualVisibility;
        #endregion

        #region Serialized Fields
        [SerializeField] private RectTransform _viewport;
        [SerializeField] private RectTransform _content;
        [SerializeField] private Slider _slider;
        [SerializeField] private bool _horizontal;

        [Tooltip("If viewport is at least this fraction of content, hide the scrollbar.")]
        [SerializeField]
        private float _noScrollThreshold = 0.99f;
        #endregion

        #region Unity Methods
        private void LateUpdate()
        {
            if (!_viewport || !_content || !_slider)
            {
                return;
            }

            float contentSize = _horizontal ? _content.rect.width : _content.rect.height;
            float viewportSize = _horizontal ? _viewport.rect.width : _viewport.rect.height;

            if (contentSize <= 0f || viewportSize <= 0f)
            {
                return;
            }

            float ratio = viewportSize / contentSize;

            bool needsScroll = ratio < _noScrollThreshold;

            SetSliderVisualVisibility(needsScroll);
        }

        private void SetSliderVisualVisibility(bool visible)
        {
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
