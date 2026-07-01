using TMPro;
using UnityEngine;

namespace Deucarian.XRUI.Controls
{
    [DisallowMultipleComponent]
    public sealed class CustomInputFieldValueMirror : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TMP_Text _output;
        [SerializeField] private string _emptyValue = "<empty>";
        #endregion

        #region Unity Methods
        private void OnEnable()
        {
            if (_inputField == null)
            {
                _inputField = GetComponent<TMP_InputField>();
            }

            if (_inputField != null)
            {
                _inputField.onValueChanged.AddListener(SetValue);
                SetValue(_inputField.text);
            }
        }

        private void OnDisable()
        {
            if (_inputField != null)
            {
                _inputField.onValueChanged.RemoveListener(SetValue);
            }
        }
        #endregion

        #region Public Methods
        public void Configure(TMP_InputField inputField, TMP_Text output)
        {
            _inputField = inputField;
            _output = output;
            SetValue(_inputField != null ? _inputField.text : string.Empty);
        }

        public void SetValue(string value)
        {
            if (_output == null)
            {
                return;
            }

            _output.text = string.IsNullOrEmpty(value) ? $"Search value: {_emptyValue}" : $"Search value: {value}";
        }
        #endregion
    }
}
