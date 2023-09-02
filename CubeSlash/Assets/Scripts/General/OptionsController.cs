using Flawliz.GenericOptions;
using UnityEngine;

public class OptionsController : Singleton
{
    public static OptionsController Instance { get { return Instance<OptionsController>(); } }

    protected override void Initialize()
    {
        base.Initialize();

        var data = GenericOptions.GetDataFromPlayerPrefs();
        Screen.SetResolution(data.Resolution.width, data.Resolution.height, data.FullScreenMode, data.Resolution.refreshRate);
        QualitySettings.vSyncCount = data.Vsync ? 1 : 0;
        Application.targetFrameRate = data.FrameRateCap;
    }
}