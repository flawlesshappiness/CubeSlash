using UnityEngine;

namespace Flawliz.GenericOptions
{
    public class OptionsData
    {
        public float MasterVolume = 0.5f;
        public float MusicVolume = 1.0f;
        public float SFXVolume = 1.0f;
        public Resolution Resolution = new Resolution { width = 1920, height = 1080, refreshRate = 60 };
        public FullScreenMode FullScreenMode = FullScreenMode.FullScreenWindow;
        public bool Vsync = true;
        public int FrameRateCap = 60;
    }
}