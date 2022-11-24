using System.Linq;
using UnityEngine;

public class MusicController : Singleton
{
    public static MusicController Instance { get { return Instance<MusicController>(); } }
    private MusicDatabase Database { get { return MusicDatabase.Load<MusicDatabase>(); } }

    private int progress_next;
    private float time_progress_next;

    private FMODEventReference current_bgm;

    private void Start()
    {
        ProgressController.Instance.OnProgress += OnProgress;
        progress_next = 5;
        time_progress_next = Time.time + 1 * 60;
    }

    private void OnProgress(int progress_counter)
    {
        TryPlayProgressMusic(progress_counter);
    }

    private void TryPlayProgressMusic(int progress_counter)
    {
        var valid_progress = progress_counter >= progress_next;
        var valid_time = Time.time >= time_progress_next;
        if (valid_progress && valid_time)
        {
            var bgm = GetProgressBGM();
            PlayBGM(bgm);

            progress_next = progress_counter + 5;
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

        time_progress_next = Time.time + 6 * 60;
    }

    public void StopBGM(FMOD.Studio.STOP_MODE stop_mode = FMOD.Studio.STOP_MODE.IMMEDIATE)
    {
        if(current_bgm != null)
        {
            current_bgm.Stop(stop_mode);
        }
    }

    private FMODEventReference GetProgressBGM()
    {
        return Database.bgms_progress.OrderBy(x => x.PlayCount).First();
    }
}