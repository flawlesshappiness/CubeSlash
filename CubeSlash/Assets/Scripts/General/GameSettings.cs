using UnityEngine;

[CreateAssetMenu(fileName = nameof(GameSettings), menuName = "Game/" + nameof(GameSettings), order = 1)]
public class GameSettings : ScriptableObject
{
    [Header("AREA")]
    public Area main_menu_area;

    [Header("MUSIC")]
    [Range(0f, 1f)] public float time_bgm_play;

    [Header("REWARD")]
    public int currency_reward_win;
    public AnimationCurve currency_mul_difficulty;

    public static GameSettings Instance => _instance ??= LoadAsset();
    private static GameSettings _instance;

    private static GameSettings LoadAsset() => Resources.Load<GameSettings>(nameof(GameSettings));
}