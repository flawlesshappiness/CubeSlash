using UnityEngine;

namespace Flawliz.GenericOptions
{
    public class LeftRightBoolControl : MonoBehaviour
    {
        [SerializeField] private LeftRightControl _control;

        protected LeftRightControl Control => _control;

        private bool _value;

        public System.Action<bool> OnValueChanged;

        protected virtual void Awake()
        {
            _control.OnLeft += OnLeft;
            _control.OnRight += OnRight;
        }

        protected virtual void OnValidate()
        {
            _control ??= GetComponentInChildren<LeftRightControl>();
        }

        private void OnRight()
        {
            ToggleValue();
        }

        private void OnLeft()
        {
            ToggleValue();
        }

        private void ToggleValue()
        {
            SetValue(!_value);
        }

        public void SetValue(bool b)
        {
            _value = b;
            OnValueChanged?.Invoke(_value);
        }

        public bool GetSelectedValue() => _value;
    }
}