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

        public int GetCurrencyEarned()
        {
            var enemies = enemies_killed;
            var levels = levels_gained * 100;
            var time = (int)(Time.time - time_start);
            var win = won ? 5000 : 0;
            return enemies + levels + time + win;
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