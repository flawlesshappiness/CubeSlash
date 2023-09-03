using Flawliz.GenericOptions;
using UnityEngine;

public class OptionsController : Singleton
{
    public static OptionsController Instance { get { return Instance<OptionsController>(); } }

    protected override void Initialize()
    {
        base.Initialize();

        var data = GenericOptions.GetDataFromPlayerPrefs();
        var resolution = data.Resolution;
        if (resolution.width == 0 || resolution.height == 0)
        {
            resolution = Screen.currentResolution;
        }

        Screen.SetResolution(resolution.width, resolution.height, data.FullScreenMode, data.Resolution.refreshRate);
        QualitySettings.vSyncCount = data.Vsync ? 1 : 0;
        Application.targetFrameRate = data.FrameRateCap;
    }
}