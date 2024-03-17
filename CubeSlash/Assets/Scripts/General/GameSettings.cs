using UnityEngine;

[CreateAssetMenu(fileName = nameof(GameSettings), menuName = "Game/" + nameof(GameSettings), order = 1)]
public class GameSettings : ScriptableObject
{
    [Header("AREA")]
    public Area main_menu_area;
    public float area_duration;
    public float area_size;
    public AnimationCurve area_count_difficulty;

    [Header("MUSIC")]
    [Range(0f, 1f)] public float time_bgm_play;

    [Header("ENEMY")]
    [Range(0f, 1f)] public float time_boss_spawn;
    public float endless_duration;
    public AnimationCurve enemy_freq_game;
    public AnimationCurve enemy_freq_difficulty;
    public AnimationCurve enemy_max_game;
    public AnimationCurve enemy_max_difficulty;
    public AnimationCurve enemy_count_game;
    public AnimationCurve boss_size_difficulty;

    [Header("EXPERIENCE")]
    public float frequency_spawn_experience;
    public int count_experience_active;
    public AnimationCurve experience_mul_difficulty;

    [Header("REWARD")]
    public int currency_reward_win;
    public AnimationCurve currency_mul_difficulty;

    [Header("CAMERA")]
    public float camera_size_start;
    public AnimationCurve camera_size_game;

    public static GameSettings Instance { get { return _instance ?? LoadAsset(); } }
    private static GameSettings _instance;

    public static GameSettings LoadAsset()
    {
        return _instance ?? (_instance = Resources.Load<GameSettings>("GameSettings"));
    }

    public float CurrentGameDuration => Time.time - SessionController.Instance.CurrentData.time_start;
    public float T_GameDuration => CurrentGameDuration / MaxGameDuration;
    public float Difficulty => DifficultyController.Instance.DifficultyValue;
    public int MaxAreaCount => (int)area_count_difficulty.Evaluate(Difficulty);
    public float MaxGameDuration => (MaxAreaCount + 1) * area_duration;
    public int EnemySpawnCount => (int)enemy_count_game.Evaluate(T_GameDuration);
    public float EnemySpawnFrequencyGame => enemy_freq_game.Evaluate(T_GameDuration);
    public float EnemySpawnFrequencyDifficulty => enemy_freq_difficulty.Evaluate(Difficulty);
    public float EnemySpawnFrequency =>
        EnemyController.Instance.IsFinalBossActive ? 1.2f :
        Mathf.Max(0.1f, EnemySpawnFrequencyGame + EnemySpawnFrequencyDifficulty);
    public int EnemyMaxCountGame => (int)enemy_max_game.Evaluate(T_GameDuration);
    public int EnemyMaxCountDifficulty => (int)enemy_max_difficulty.Evaluate(Difficulty);
    public int EnemyMaxCount => EnemyMaxCountGame + EnemyMaxCountDifficulty;
}