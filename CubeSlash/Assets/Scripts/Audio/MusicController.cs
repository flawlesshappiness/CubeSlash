using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class MusicController : Singleton
{
    public static MusicController Instance { get { return Instance<MusicController>(); } }

    private FMODEventInstance current_bgm;

    private Coroutine cr_bgm_delay;

    protected override void Initialize()
    {
        base.Initialize();
        GameController.Instance.onMainMenu += OnMainMenu;
        GameController.Instance.onGameStart += OnGameStart;
        GameController.Instance.onPlayerDeath += OnPlayerDeath;
        AreaController.Instance.onNextArea += OnNextArea;
    }

    private void OnGameStart()
    {
        PlayBGM(SoundEffectType.bgm_start_game);
    }

    private void OnMainMenu()
    {
        if(cr_bgm_delay != null)
        {
            StopCoroutine(cr_bgm_delay);
            cr_bgm_delay = null;
        }

        PlayBGM(SoundEffectType.bgm_menu);
    }

    private void OnPlayerDeath()
    {
        PlayBGM(SoundEffectType.bgm_lose_game);
    }

    private void OnNextArea(Area area)
    {
        var delay = GameSettings.Instance.area_duration * GameSettings.Instance.time_bgm_play;
        PlayBGM(area.bgm_type, delay);
    }

    public void PlayBGM(SoundEffectType type, float delay)
    {
        cr_bgm_delay = StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(delay);
            PlayBGM(type);
        }
    }

    public void PlayBGM(SoundEffectType type)
    {
        StopBGM();
        current_bgm = SoundController.Instance.Play(type);
    }

    public void StopBGM(FMOD.Studio.STOP_MODE stop_mode = FMOD.Studio.STOP_MODE.IMMEDIATE)
    {
        if(current_bgm != null)
        {
            current_bgm.Stop(stop_mode);
        }
    }

    public void FadeOutBGM(float duration)
    {
        if (current_bgm == null) return;
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            var instance = current_bgm;
            current_bgm = null;

            var start = instance.GetVolume();
            var end = 0f;
            yield return LerpEnumerator.Value(duration, f =>
            {
                instance.SetVolume(Mathf.Lerp(start, end, f));
            }).UnscaledTime();
            instance.Stop();
        }
    }
}