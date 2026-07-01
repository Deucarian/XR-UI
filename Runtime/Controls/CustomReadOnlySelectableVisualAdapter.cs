using UnityEngine;

namespace Deucarian.XRUI.Controls
{
    [DisallowMultipleComponent]
    public sealed class CustomReadOnlySelectableVisualAdapter : MonoBehaviour, ICustomPressableSelectableVisualOverride
    {
        [SerializeField] private bool _preserveNormalVisuals = true;

        public bool TreatSelectableAsInteractableForCustomFeedback => false;
        public bool TreatSelectableAsVisuallyInteractableForCustomFeedback =>
                enabled && _preserveNormalVisuals;
    }
}
