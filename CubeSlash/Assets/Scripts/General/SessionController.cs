using UnityEngine;

public class SessionController : Singleton
{
    public static SessionController Instance { get { return Instance<SessionController>(); } }

    public Data CurrentData { get; private set; }
    public class Data
    {
        public int enemies_killed;
        public int levels_gained;
        public float time_start;
        public bool won;
        public bool has_received_currency;

        public int GetCurrencyEarned()
        {
            var enemies = enemies_killed;
            var levels = levels_gained * 100;
            var time = (int)(Time.time - time_start);
            var win = won ? GameSettings.Instance.currency_reward_win : 0;
            var mul_difficulty = GameSettings.Instance.currency_mul_difficulty.Evaluate(DifficultyController.Instance.DifficultyValue);
            var mul_received = has_received_currency ? 0 : 1;
            return (int)((enemies + levels + time + win) * mul_received * mul_difficulty);
        }
    }

    protected override void Initialize()
    {
        base.Initialize();
        GameController.Instance.onGameStart += CreateSession;
        EnemyController.Instance.OnEnemyKilled += () => CurrentData.enemies_killed++;
        GameController.Instance.onPlayerLevelUp += () => CurrentData.levels_gained++;
    }

    public void CreateSession()
    {
        CurrentData = new Data();
        CurrentData.time_start = Time.time;
    }
}