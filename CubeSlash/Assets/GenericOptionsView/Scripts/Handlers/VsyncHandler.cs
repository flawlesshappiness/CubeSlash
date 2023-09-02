using UnityEngine;

namespace Flawliz.GenericOptions
{
    public class VsyncHandler : GenericOptionsHandler
    {
        [SerializeField] private VSyncControl _control;

        private void OnValidate()
        {
            _control ??= GetComponent<VSyncControl>();
        }

        private void Start()
        {
            _control.SetValue(Root.Data.Vsync);
            _control.OnValueChanged += v => Root.Data.Vsync = v;
            _control.OnValueChanged += v => Root.SetHasChanges();
        }

        protected override void OnDataChanged(OptionsData data)
        {
            base.OnDataChanged(data);
            _control.SetValue(data.Vsync);
        }

        public override void Apply()
        {
            base.Apply();
            var value = _control.GetSelectedValue() ? 1 : 0;
            QualitySettings.vSyncCount = value;
        }

        public override void RestoreDefault()
        {
            base.RestoreDefault();
            _control.SetValue(true);
        }
    }
}