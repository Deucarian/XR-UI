using System.Collections;
using Deucarian.Common;
using Deucarian.XRUI.Controls;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deucarian.XRUI.Dropdowns
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Dropdown))]
    public class TmpDropdownNoBlockerToggle : MonoBehaviour,
                                              ICustomPressActivationTarget,
                                              IPointerClickHandler
    {
        #region Constants and Fields
        private TMP_Dropdown _dropdown;
        private CustomPressableSurface _pressableSurface;
        private CustomPressActivationGate _activationGate;
        private Coroutine _postShowRoutine;
        #endregion

        #region Private Properties
        private CustomPressActivationGate ActivationGate => _activationGate ??= new CustomPressActivationGate(this);
        #endregion

        #region Serialized Fields
        [Tooltip("If true, we destroy the blocker GameObject. If false, we just disable its raycast blocking.")]
        [SerializeField]
        private bool destroyBlocker = true;

        [Tooltip("Also neutralize the blocker by disabling raycast targets / blocksRaycasts if present.")]
        [SerializeField]
        private bool disableRaycastsToo = true;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            _dropdown = GetComponent<TMP_Dropdown>();
            _pressableSurface = GetComponent<CustomPressableSurface>();
        }

        private void OnEnable()
        {
            _pressableSurface = GetComponent<CustomPressableSurface>();
            if (_pressableSurface != null)
            {
                ActivationGate.Bind(_pressableSurface, this);
            }
        }

        private void OnDisable()
        {
            _activationGate?.Unbind();

            if (_postShowRoutine != null)
            {
                StopCoroutine(_postShowRoutine);
                _postShowRoutine = null;
            }

            if (_dropdown != null)
            {
                _dropdown.Hide();
            }

            DropdownInteractionCoordinator.End(this);
        }
        #endregion

        #region Public Methods
        public void BindPressableSurface(CustomPressableSurface pressableSurface)
        {
            if (_pressableSurface == pressableSurface)
                return;

            _activationGate?.Unbind();

            _pressableSurface = pressableSurface;

            if (_pressableSurface != null && isActiveAndEnabled)
                ActivationGate.Bind(_pressableSurface, this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_pressableSurface != null)
            {
                return;
            }

            // Toggle behavior: if open -> close, else -> open.
            if (IsOpen())
            {
                _dropdown.Hide();
                KillBlockerNow();
                DropdownInteractionCoordinator.End(this);
            }
            else
            {
                DropdownInteractionCoordinator.Begin(this, _dropdown);
                _dropdown.Show();

                if (_postShowRoutine != null)
                    StopCoroutine(_postShowRoutine);
                _postShowRoutine = StartCoroutine(KillBlockerEndOfFrame());
            }
        }
        #endregion

        #region Private Methods
        public bool CanActivatePress(CustomPressableSurface _) => _dropdown != null && isActiveAndEnabled;

        public void ActivateFromPress(CustomPressableSurface _)
        {
            if (_postShowRoutine != null)
                StopCoroutine(_postShowRoutine);
            _postShowRoutine = StartCoroutine(KillBlockerEndOfFrame());
        }

        private bool IsOpen()
        {
            // TMP_Dropdown instantiates a child named "Dropdown List" under the root canvas.
            Canvas rootCanvas = GetRootCanvas();
            if (!rootCanvas)
                return false;

            Transform list = rootCanvas.transform.Find("Dropdown List");
            return list != null && list.gameObject.activeInHierarchy;
        }

        private Canvas GetRootCanvas()
        {
            // TMP uses the root canvas for list + blocker.
            Canvas canvas = GetComponentInParent<Canvas>();
            return canvas ? canvas.rootCanvas : null;
        }

        private IEnumerator KillBlockerEndOfFrame()
        {
            // Wait until TMP has spawned the blocker.
            yield return new WaitForEndOfFrame();

            KillBlockerNow();

            while (DropdownInteractionCoordinator.HasOpenDropdownList(this))
            {
                yield return null;
            }

            _postShowRoutine = null;
            DropdownInteractionCoordinator.End(this);
        }

        private void KillBlockerNow()
        {
            Canvas rootCanvas = GetRootCanvas();
            if (!rootCanvas)
                return;

            // TMP names it "Blocker" by default.
            Transform blocker = rootCanvas.transform.Find("Blocker");
            if (!blocker)
                return;

            if (disableRaycastsToo)
            {
                Image img = blocker.GetComponent<Image>();
                if (img)
                    img.raycastTarget = false;

                CanvasGroup cg = blocker.GetComponent<CanvasGroup>();
                if (cg)
                    cg.blocksRaycasts = false;
            }

            if (destroyBlocker)
            {
                UnityObjectUtility.DestroySafely(blocker.gameObject);
            }
        }
        #endregion
    }
}
