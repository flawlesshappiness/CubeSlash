using UnityEngine;

namespace Flawliz.GenericOptions
{
    public class FramerateHandler : GenericOptionsHandler
    {
        [SerializeField] private FramerateControl framerate_control;
        [SerializeField] private VSyncControl vsync_control;

        private void OnValidate()
        {
            framerate_control ??= GetComponent<FramerateControl>();
            vsync_control ??= GetComponent<VSyncControl>();
        }

        private void Start()
        {
            framerate_control.OnValueChanged += v => Root.Data.FrameRateCap = v;
            framerate_control.OnValueChanged += v => OnFramerateValueChanged();

            vsync_control.OnValueChanged += v => Root.Data.Vsync = v;
            vsync_control.OnValueChanged += v => OnVsyncValueChanged();

            framerate_control.SetIndexFromValue(Root.Data.FrameRateCap);
            vsync_control.SetValue(Root.Data.Vsync);

            framerate_control.OnValueChanged += v => Root.SetHasChanges();
            vsync_control.OnValueChanged += v => Root.SetHasChanges();
        }

        protected override void OnDataChanged(OptionsData data)
        {
            base.OnDataChanged(data);
            framerate_control.SetIndexFromValue(data.FrameRateCap);
            vsync_control.SetValue(data.Vsync);
        }

        public override void Apply()
        {
            base.Apply();
            var v_framerate = framerate_control.GetSelectedValue();
            var v_vsync = vsync_control.GetSelectedValue() ? 1 : 0;
            Application.targetFrameRate = v_framerate;
            QualitySettings.vSyncCount = v_vsync;
        }

        public override void RestoreDefault()
        {
            base.RestoreDefault();
            framerate_control.SetIndexFromValue(60);
            vsync_control.SetValue(true);
        }

        private void OnFramerateValueChanged()
        {
        }

        private void OnVsyncValueChanged()
        {
            var vsync_on = vsync_control.GetSelectedValue();
            if (vsync_on)
            {
                framerate_control.SetIndexFromValue(60);
            }

            framerate_control.Enabled = !vsync_on;
        }
    }
}