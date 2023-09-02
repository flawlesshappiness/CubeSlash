using UnityEngine;

namespace Flawliz.GenericOptions
{
    public class FullScreenModeControl : LeftRightIndexControl<FullScreenMode>
    {
        public override FullScreenMode[] Values => _values ?? (_values = GetValues());

        private FullScreenMode[] _values;

        protected override void Awake()
        {
            base.Awake();
            OnIndexChanged += _ => UpdateText();
        }

        private void Start()
        {
            UpdateText();
        }

        private void UpdateText()
        {
            var mode = GetSelectedValue();
            var text = mode switch
            {
                FullScreenMode.Windowed => "Windowed",
                FullScreenMode.FullScreenWindow => "Borderless",
                FullScreenMode.ExclusiveFullScreen => "Fullscreen",
                FullScreenMode.MaximizedWindow => "Fullscreen",
                _ => "Unknown"
            };
            Control.SetText(text);
        }

        private FullScreenMode[] GetValues()
        {
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
            {
                return new FullScreenMode[3]
                {
                    FullScreenMode.MaximizedWindow, FullScreenMode.FullScreenWindow, FullScreenMode.Windowed
                };
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                return new FullScreenMode[3]
                {
                    FullScreenMode.ExclusiveFullScreen, FullScreenMode.FullScreenWindow, FullScreenMode.Windowed
                };
            }
            else
            {
                return new FullScreenMode[2]
                {
                    FullScreenMode.FullScreenWindow, FullScreenMode.Windowed
                };
            }
        }
    }
}