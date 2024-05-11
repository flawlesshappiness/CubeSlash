using System;
using UnityEngine;

public class RunController : Singleton
{
    public static RunController Instance => Instance<RunController>();

    public RunInfo CurrentRun { get; private set; }

    public Action onRunStarted;

    protected override void Initialize()
    {
        base.Initialize();
        GameController.Instance.onGameStart += GameStart;
        GameController.Instance.onEndlessStart += EndlessStart;
        GameController.Instance.onMainMenu += MainMenu;
        EnemyController.Instance.OnEnemyKilled += EnemyKilled;
    }

    private void GameStart()
    {
        CurrentRun = GenerateRun();
        onRunStarted?.Invoke();
    }

    private void EndlessStart()
    {
        onRunStarted?.Invoke();
    }

    private void MainMenu()
    {
        if (CurrentRun == null) return;

        CurrentRun.Endless = false;
    }

    public RunInfo GenerateRun()
    {
        var gamemode = GamemodeController.Instance.SelectedGameMode;
        var areas = AreaController.Instance.GetRandomAreas(gamemode.area_count);
        var info = new RunInfo
        {
            Areas = areas,
            Gamemode = gamemode,
            StartTime = Time.time,
        };

        return info;
    }

    private void EnemyKilled(EnemySettings settings)
    {
        if (CurrentRun == null) return;

        if (!CurrentRun.EnemiesKilled.ContainsKey(settings.type))
        {
            CurrentRun.EnemiesKilled.Add(settings.type, 0);
        }

        CurrentRun.EnemiesKilled[settings.type]++;
    }
}