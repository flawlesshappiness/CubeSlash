using UnityEngine;

namespace Flawliz.GenericOptions
{
    public class ScreenResolutionControl : LeftRightIndexControl<Resolution>
    {
        public override Resolution[] Values => _values ?? (_values = Screen.resolutions);

        private Resolution[] _values;

        protected override void Awake()
        {
            base.Awake();
            OnIndexChanged += _ => UpdateText();
        }

        private void UpdateText()
        {
            var resolution = GetSelectedValue();
            Control.SetText($"{resolution.width} x {resolution.height} @ {resolution.refreshRateRatio.value}Hz");
        }
    }
}