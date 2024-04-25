using UnityEngine;

[CreateAssetMenu(fileName = nameof(GamemodeSettings), menuName = "Game/" + nameof(GamemodeSettings), order = 1)]
public class GamemodeSettings : ScriptableObject
{
    public GamemodeType type;

    [Header("UI")]
    public string gamemode_name;
    public string gamemode_desc;
    public Sprite icon;

    [Header("AREA")]
    public int area_count;
    public float area_duration;
    public float area_size;

    [Header("ENEMY")]
    public float enemy_spawn_frequency_start;
    public float enemy_spawn_frequency_end;
    public float enemy_spawn_frequency_final;
    public int enemy_spawn_count_start;
    public int enemy_spawn_count_end;
    public int enemy_count_max_start;
    public int enemy_count_max_end;

    [Header("BOSS")]
    [Range(0f, 1f)] public float boss_time_spawn;
    public float boss_size;
    public int boss_spawn_count;

    [Header("EXPERIENCE")]
    public float experience_spawn_frequency;
    public float experience_count_max;
    public float experience_gain_multiplier;

    [Header("CAMERA")]
    public float camera_size_start;
    public float camera_size_end;

    private RunInfo Run => RunInfo.Current;
    public float MaxGameDuration => (area_count + 1) * area_duration;
    public float StartTime => Run.Endless ? Run.EndlessStartTime : Run.StartTime;
    public float CurrentGameDuration => Time.time - StartTime;
    public float T_GameDuration => CurrentGameDuration / MaxGameDuration;
    public float BossSpawnTime => area_duration * boss_time_spawn;
    public int EnemyCountMax => (int)Mathf.Lerp(enemy_count_max_start, enemy_count_max_end, T_GameDuration);
    public int EnemySpawnCount => (int)Mathf.Lerp(enemy_spawn_count_start, enemy_spawn_count_end, T_GameDuration);
    public float EnemySpawnFrequency => EnemyController.Instance.IsFinalBossActive ? enemy_spawn_frequency_final : Mathf.Lerp(enemy_spawn_frequency_start, enemy_spawn_frequency_end, T_GameDuration);
    public float CameraSize => Mathf.Lerp(camera_size_start, camera_size_end, T_GameDuration);
}