using System.Linq;
using UnityEngine;

public class MusicController : Singleton
{
    public static MusicController Instance { get { return Instance<MusicController>(); } }
    private MusicDatabase Database { get { return MusicDatabase.Load<MusicDatabase>(); } }

    private FMODEventReference current_bgm;

    private void Start()
    {
        GameController.Instance.OnNextLevel += OnNextLevel;
    }

    private void OnNextLevel()
    {
        var level = Level.Current;
        if (level.bgm.Exists)
        {
            PlayBGM(level.bgm);
        }
    }

    public void PlayStartMusic()
    {
        PlayBGM(Database.bgm_start_game);
    }

    private void PlayBGM(FMODEventReference bgm)
    {
        StopBGM();
        current_bgm = bgm;
        bgm.Play();
    }

    public void StopBGM(FMOD.Studio.STOP_MODE stop_mode = FMOD.Studio.STOP_MODE.IMMEDIATE)
    {
        if(current_bgm != null)
        {
            current_bgm.Stop(stop_mode);
        }
    }
}