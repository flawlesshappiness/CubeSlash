using UnityEngine;

namespace Flawliz.GenericOptions
{
    public class OptionsData
    {
        public float MasterVolume { get; set; } = 0.5f;
        public float MusicVolume { get; set; } = 1.0f;
        public float SFXVolume { get; set; } = 1.0f;
        public Resolution Resolution { get; set; }
        public FullScreenMode FullScreenMode { get; set; } = FullScreenMode.FullScreenWindow;
        public bool Vsync { get; set; } = true;
        public int FrameRateCap { get; set; } = 60;
    }
}