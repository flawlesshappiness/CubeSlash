using FMOD.Studio;
using UnityEngine;

public class AudioController : Singleton
{
    public static AudioController Instance { get { return Instance<AudioController>(); } }

    private Bus Music;
    private Bus SFX;
    private Bus Master;

    private EventInstance snapshot_menu;

    protected override void Initialize()
    {
        base.Initialize();
        InitializeBusses();
        InitializeSnapshots();

        GameStateController.Instance.onGameStateChanged += OnGameStateChanged;
    }

    private void InitializeBusses()
    {
        Music = FMODController.Instance.GetBus("Music");
        SFX = FMODController.Instance.GetBus("SFX");
        Master = FMODController.Instance.GetMasterBus();
    }

    private void InitializeSnapshots()
    {
        snapshot_menu = FMODController.Instance.CreateSnapshotInstance("Menu");
        snapshot_menu.start();
        snapshot_menu.setParameterByName("Intensity", 0f);
    }

    public void SetMusicVolume(float volume) => Music.setVolume(volume);
    public void SetSFXVolume(float volume) => SFX.setVolume(volume);
    public void SetMasterVolume(float volume) => Master.setVolume(volume);
    public float GetMusicVolume() => FMODController.Instance.GetBusVolume(Music);
    public float GetSFXVolume() => FMODController.Instance.GetBusVolume(SFX);
    public float GetMasterVolume() => FMODController.Instance.GetBusVolume(Master);

    public void SetInMenu(bool in_menu)
    {
        var value = in_menu ? 1f : 0f;
        snapshot_menu.setParameterByName("Intensity", value);
    }

    private void OnGameStateChanged(GameStateType type)
    {
        if(type == GameStateType.MENU)
        {
            SetInMenu(true);
        }
        else if(type == GameStateType.PLAYING)
        {
            SetInMenu(false);
        }
    }
}