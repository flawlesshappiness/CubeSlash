using System.Collections;
using System.Linq;
using UnityEngine;

public class MusicController : Singleton
{
    public static MusicController Instance { get { return Instance<MusicController>(); } }
    private MusicDatabase Database { get { return MusicDatabase.Load<MusicDatabase>(); } }

    private FMODEventReference current_bgm;

    private Coroutine cr_bgm_delay;

    private void Start()
    {
        GameController.Instance.onMainMenu += OnMainMenu;
        AreaController.Instance.onNextArea += OnNextArea;
    }

    private void OnMainMenu()
    {
        if(cr_bgm_delay != null)
        {
            StopCoroutine(cr_bgm_delay);
            cr_bgm_delay = null;
        }

        StopBGM();
    }

    private void OnNextArea(Area area)
    {
        var delay = GameSettings.Instance.area_duration * GameSettings.Instance.time_bgm_play;
        PlayBGM(area.bgm, GameSettings.Instance.time_bgm_play);
    }

    public void PlayStartMusic()
    {
        PlayBGM(Database.bgm_start_game);
    }

    private void PlayBGM(FMODEventReference bgm, float delay)
    {
        cr_bgm_delay = StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(delay);
            PlayBGM(bgm);
        }
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