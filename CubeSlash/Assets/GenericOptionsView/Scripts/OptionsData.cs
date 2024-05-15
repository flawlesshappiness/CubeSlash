using UnityEngine;

namespace Flawliz.GenericOptions
{
    public class OptionsData
    {
        public float MasterVolume = 0.5f;
        public float MusicVolume = 1.0f;
        public float SFXVolume = 1.0f;
        public int screen_resolution_width = 1920;
        public int screen_resolution_height = 1080;
        public uint refresh_ratio_numerator = 60;
        public uint refresh_ratio_denominator = 1;
        public FullScreenMode FullScreenMode = FullScreenMode.FullScreenWindow;
        public bool Vsync = true;
        public int FrameRateCap = 60;

        public Resolution Resolution => new Resolution
        {
            width = screen_resolution_width,
            height = screen_resolution_height,
            refreshRateRatio = new RefreshRate
            {
                numerator = refresh_ratio_numerator,
                denominator = refresh_ratio_denominator
            },
        };

        public void SetResolution(Resolution resolution)
        {
            screen_resolution_width = resolution.width;
            screen_resolution_height = resolution.height;
            refresh_ratio_numerator = resolution.refreshRateRatio.numerator;
            refresh_ratio_denominator = resolution.refreshRateRatio.denominator;
        }
    }
}