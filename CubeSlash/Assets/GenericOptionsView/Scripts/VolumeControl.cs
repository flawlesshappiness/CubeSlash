using TMPro;
using UnityEngine;

namespace Flawliz.GenericOptions
{
    public class VolumeControl : LeftRightControl
    {
        [SerializeField] protected TMP_Text _tmp_value;

        public System.Action<int> OnValueChanged;
        public System.Action<float> OnPercentageChanged;

        private int _value;

        private const int MIN_VALUE = 0;
        private const int MAX_VALUE = 100;

        protected override void Start()
        {
            base.Start();
            UpdateText();
        }

        protected override void ClickLeft()
        {
            base.ClickLeft();
            AdjustValue(-1);
        }

        protected override void ClickRight()
        {
            base.ClickRight();
            AdjustValue(1);
        }

        private void AdjustValue(int adjust) => SetValue(_value + adjust);

        public void SetPercentage(float percentage)
        {
            var value = (int)(percentage * (MAX_VALUE - MIN_VALUE) + MIN_VALUE);
            SetValue(value);
        }

        public void SetValue(int value)
        {
            _value = Mathf.Clamp(value, MIN_VALUE, MAX_VALUE);
            var t = Mathf.Clamp01((float)(_value - MIN_VALUE) / (MAX_VALUE - MIN_VALUE));

            UpdateText();

            OnValueChanged?.Invoke(value);
            OnPercentageChanged?.Invoke(t);
        }

        private void UpdateText()
        {
            _tmp_value.text = $"{_value}%";
        }
    }
}