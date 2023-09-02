using UnityEngine;

namespace Flawliz.GenericOptions
{
    public class FramerateHandler : GenericOptionsHandler
    {
        [SerializeField] private FramerateControl _control;

        private void OnValidate()
        {
            _control ??= GetComponent<FramerateControl>();
        }

        private void Start()
        {
            _control.SetIndexFromValue(Root.Data.FrameRateCap);
            _control.OnValueChanged += v => Root.Data.FrameRateCap = v;
            _control.OnValueChanged += v => Root.SetHasChanges();
        }

        protected override void OnDataChanged(OptionsData data)
        {
            base.OnDataChanged(data);
            _control.SetIndexFromValue(data.FrameRateCap);
        }

        public override void Apply()
        {
            base.Apply();
            var value = _control.GetSelectedValue();
            Application.targetFrameRate = value;
        }

        public override void RestoreDefault()
        {
            base.RestoreDefault();
            _control.SetIndexFromValue(60);
        }
    }
}