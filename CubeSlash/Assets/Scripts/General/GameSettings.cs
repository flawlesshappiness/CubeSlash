using UnityEngine;

[CreateAssetMenu(fileName = nameof(GameSettings), menuName = "Game/" + nameof(GameSettings), order = 1)]
public class GameSettings : ScriptableObject
{
    [Header("GENERAL")]
    public int areas_to_win;

    [Header("AREA")]
    public Area main_menu_area;
    public float area_duration;
    public float area_size;

    [Header("MUSIC")]
    [Range(0f, 1f)] public float time_bgm_play;

    [Header("ENEMIES")]
    [Range(0f, 1f)] public float time_boss_spawn;
    public float endless_duration;
    public AnimationCurve enemy_freq_area;
    public AnimationCurve enemy_freq_game;
    public AnimationCurve enemy_freq_endless;

    [Header("EXPERIENCE")]
    public float frequency_spawn_experience;
    public int count_experience_active;

    public static GameSettings Instance { get { return _instance ?? LoadAsset(); } }
    private static GameSettings _instance;

    public static GameSettings LoadAsset()
    {
        return _instance ?? (_instance = Resources.Load<GameSettings>("GameSettings"));
    }
}