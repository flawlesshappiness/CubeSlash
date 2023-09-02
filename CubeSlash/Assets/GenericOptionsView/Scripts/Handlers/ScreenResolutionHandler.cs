using System.Linq;
using UnityEngine;

namespace Flawliz.GenericOptions
{
    public class ScreenResolutionHandler : GenericOptionsHandler
    {
        [SerializeField] private ScreenResolutionControl _resolution_control;
        [SerializeField] private FullScreenModeControl _full_screen_mode_control;

        private void OnValidate()
        {
            _resolution_control ??= GetComponentInChildren<ScreenResolutionControl>();
            _full_screen_mode_control ??= GetComponentInChildren<FullScreenModeControl>();
        }

        private void Start()
        {
            SetResolution(Root.Data.Resolution);
            SetFullScreenMode(Root.Data.FullScreenMode);

            _resolution_control.OnIndexChanged += i => Root.Data.Resolution = _resolution_control.GetSelectedValue();
            _resolution_control.OnIndexChanged += i => Root.SetHasChanges();

            _full_screen_mode_control.OnIndexChanged += i => Root.Data.FullScreenMode = _full_screen_mode_control.GetSelectedValue();
            _full_screen_mode_control.OnIndexChanged += i => Root.SetHasChanges();
        }

        protected override void OnDataChanged(OptionsData data)
        {
            base.OnDataChanged(data);
            SetResolution(data.Resolution);
            SetFullScreenMode(data.FullScreenMode);
        }

        public override void RestoreDefault()
        {
            base.RestoreDefault();
            SetResolution(Screen.currentResolution);
            SetFullScreenMode(FullScreenMode.FullScreenWindow);
        }

        public override void Apply()
        {
            base.Apply();
            var resolution = _resolution_control.GetSelectedValue();
            var mode = _full_screen_mode_control.GetSelectedValue();
            Screen.SetResolution(resolution.width, resolution.height, mode, resolution.refreshRate);
        }

        private void SetResolution(Resolution resolution)
        {
            var resolutions = _resolution_control.Values.ToList();

            var matching_res = resolutions.FirstOrDefault(res =>
                    res.width == resolution.width &&
                    res.height == resolution.height &&
                    res.refreshRate == resolution.refreshRate);

            resolution = matching_res;

            if (resolution.width == 0 || resolution.height == 0)
            {
                var current = Screen.currentResolution;
                resolution = resolutions.FirstOrDefault(res => res.width == current.width && res.height == current.height);
            }

            var idx = resolutions.IndexOf(resolution);
            _resolution_control.SetIndex(idx);
        }

        private void SetFullScreenMode(FullScreenMode mode)
        {
            var modes = _full_screen_mode_control.Values.ToList();
            var idx = modes.IndexOf(mode);
            _full_screen_mode_control.SetIndex(idx);
        }
    }
}