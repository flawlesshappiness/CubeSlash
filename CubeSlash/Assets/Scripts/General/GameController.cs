using Flawliz.Lerp;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] public Transform world;

    public static GameController Instance;
    public static bool DAMAGE_DISABLED = false;

    public bool FirstTimeBoot { get; private set; } = true;
    public bool IsGameStarted { get; private set; }
    public bool IsGameEnded { get; private set; }
    public bool IsPaused { get { return PauseLock.IsLocked; } }
    public MultiLock PauseLock { get; private set; } = new MultiLock();
    public int LevelIndex { get; set; }

    public System.Action onResume { get; set; }
    public System.Action onGameStart { get; set; }
    public System.Action onGameEnd { get; set; }
    public System.Action onMainMenu { get; set; }
    public System.Action onPlayerLevelUp { get; set; }
    public System.Action onPlayerDeath { get; set; }
    public System.Action onWin { get; set; }

    public const string TAG_ABILITY_VIEW = "Ability";

    private bool _levelling_up;

    private GameStateType GameState { get { return GameStateController.Instance.GameState; } }

    private void Awake()
    {
        LogController.LogMethod();

        Instance = this;
        PlayerInput.Initialize();
        SteamIntegration.Create();
        Singleton.CreateAllSingletons();
        PauseLock.OnLockChanged += OnPauseChanged;
        PlayerInput.OnCurrentDeviceLost += OnDeviceLost;
    }

    private void Start()
    {
        LogController.LogMethod();

        StartCoroutine(StartupCr());
    }

    private IEnumerator StartupCr()
    {
        InitializePlayer();
        BackgroundController.Instance.FadeToArea(GameSettings.Instance.main_menu_area);

        var view = ViewController.Instance.ShowView<SplashView>(0, "Splash");
        yield return view.ShowSplashes();
        view.Close(2f);
        MainMenu();
    }

    public void InitializePlayer()
    {
        LogController.LogMethod();

        var prefab_player = Resources.Load<GameObject>("Prefabs/Entities/Player");
        Player.Instance = Instantiate(prefab_player, world).GetComponent<Player>();
        Player.Instance.Initialize();
        Player.Instance.onDeath += OnPlayerDeath;
        Player.Instance.onLevelUp += OnPlayerLevelUp;
        Player.Instance.gameObject.SetActive(true);
    }

    private void OnDeviceLost(PlayerInput.DeviceType type)
    {
        if (IsGameStarted)
        {
            OpenPauseView();
        }
    }

    public void HomeButtonPressed()
    {
    }

    private void OnSteamOverlayEnabled()
    {
        if (IsGameStarted)
        {
            OpenPauseView();
        }
    }

    public void OpenPauseView()
    {
        if (!IsGameStarted || IsGameEnded) return;
        if (PauseView.Exists) return;
        if (GameState != GameStateType.PLAYING) return;
        if (PauseLock.IsLocked) return;
        if (_levelling_up) return;
        GameStateController.Instance.SetGameState(GameStateType.MENU);
        ViewController.Instance.ShowView<PauseView>(0, nameof(PauseView));
    }

    private void OnPauseChanged(bool paused)
    {
        SetTimeScale(paused ? 0 : 1);
    }

    public void SetTimeScale(float time)
    {
        Time.timeScale = PauseLock.IsLocked ? 0 : time;
    }

    public Lerp LerpTimeScale(float duration, float end)
    {
        var start = Time.timeScale;
        return Lerp.Value("timescale", duration, f => SetTimeScale(Mathf.Lerp(start, end, f)))
            .UnscaledTime();
    }

    public void Quit()
    {
        LogController.LogMethod();

#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    private void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void StartGame()
    {
        LogController.LogMethod();

        StartCoroutine(Cr());

        IEnumerator Cr()
        {
            /*
            var view = ViewController.Instance.ShowView<GameIntroView>(1);
            yield return new WaitForSecondsRealtime(1);
            yield return view.AnimateIntro();
            view.Close(1);
            yield return new WaitForSecondsRealtime(1);
            */

            IsGameStarted = true;

            onGameStart?.Invoke();

            GameStateController.Instance.SetGameState(GameStateType.MENU);
            AreaController.Instance.StartAreaCoroutine();
            ViewController.Instance.ShowView<GameView>(0);

            Player.Instance.ResetValues();

            ResumeLevel();

            yield return null;
        }
    }

    public void EndGame()
    {
        LogController.LogMethod();

        IsGameEnded = true;
        onGameEnd?.Invoke();
    }

    private void PauseLevel()
    {
        PauseLock.AddLock(nameof(GameController));
    }

    public void ResumeLevel()
    {
        GameStateController.Instance.SetGameState(GameStateType.PLAYING);
        PauseLock.RemoveLock(nameof(GameController));
        onResume?.Invoke();
    }

    private void OnPlayerLevelUp()
    {
        LogController.LogMethod();

        _levelling_up = true;

        Player.Instance.ResetExperience();
        onPlayerLevelUp?.Invoke();

        var unlock_ability = Player.Instance.CanGainAbility() && AbilityController.Instance.CanGainAbility();
        var unlock_upgrade = UpgradeController.Instance.HasUnlockableUpgrades();

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            Player.Instance.PlayLevelUpFX();
            Player.PushEnemiesInArea(Player.Instance.transform.position, 12, 500, use_mass: true);

            if (unlock_ability || unlock_upgrade)
            {
                yield return LerpTimeScale(1.0f, 0.25f);
                GameStateController.Instance.SetGameState(GameStateType.MENU);
                PauseLevel();
            }

            if (unlock_ability)
            {
                Player.Instance.ResetLevelsUntilAbility();

                var view_unlock = ViewController.Instance.ShowView<UnlockAbilityView>(0, TAG_ABILITY_VIEW);
                view_unlock.OnAbilitySelected += ResumeLevel;
                view_unlock.OnSkip += ResumeLevel;
            }
            else if (unlock_upgrade)
            {
                var view = ViewController.Instance.ShowView<UnlockUpgradeView>(0, TAG_ABILITY_VIEW);
                view.OnUpgradeSelected += u =>
                {
                    ResumeLevel();
                };
            }

            _levelling_up = false;
        }
    }

    public void ReturnToMainMenu()
    {
        LogController.LogMethod();

        EndGame();
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            PauseLock.AddLock("MainMenu");
            GameStateController.Instance.SetGameState(GameStateType.MENU);
            var bg_view = ViewController.Instance.ShowView<BackgroundView>(0.5f, "Foreground");
            MusicController.Instance.FadeOutBGM(0.5f);
            yield return new WaitForSecondsRealtime(0.5f);
            bg_view.Close(0.5f);
            PauseLock.RemoveLock("MainMenu");
            MainMenu();
        }
    }

    public void MainMenu()
    {
        LogController.LogMethod();

        IsGameStarted = false;
        IsGameEnded = false;
        GameStateController.Instance.SetGameState(GameStateType.MENU);
        CameraController.Instance.SetSize(15f);

        if (FirstTimeBoot)
        {
            FirstTimeBoot = false;
            ViewController.Instance.ShowView<TitleView>(0f);
        }
        else
        {
            ViewController.Instance.ShowView<StartView>(0f);
        }

        GameAttributeController.Instance.Clear();

        onMainMenu?.Invoke();
    }

    private void OnPlayerDeath()
    {
        LogController.LogMethod();

        // Lose
        Save.Game.count_losses++;

        UnlockLoseAchievement();

        // End
        EndGame();
        SessionController.Instance.CurrentData.won = false;
        StartCoroutine(EndGameCr());
        onPlayerDeath?.Invoke();
    }

    public void Win()
    {
        LogController.LogMethod();

        if (IsGameEnded) return;

        // Wins
        Save.Game.count_wins++;

        UnlockWinAchievement();
        UnlockWinAbilityAchievement();

        // End
        IsGameEnded = true;
        SessionController.Instance.CurrentData.won = true;
        MusicController.Instance.StopBGM();
        EnemyController.Instance.KillActiveEnemies();
        onWin?.Invoke();
        StartCoroutine(EndGameCr());
    }

    private IEnumerator EndGameCr()
    {
        Save.Game.runs_completed++;
        IsGameEnded = true;
        yield return new WaitForSeconds(3f);
        GameStateController.Instance.SetGameState(GameStateType.MENU);
        ViewController.Instance.CloseView(0.5f);
        var end_view = ViewController.Instance.ShowView<GameEndView>(0, nameof(GameEndView));
    }

    public void ResumeEndless()
    {
        IsGameEnded = false;
        GameStateController.Instance.SetGameState(GameStateType.PLAYING);
    }

    private void UnlockWinAchievement()
    {
        LogController.LogMethod();

        if (Save.Game.count_wins >= 1)
        {
            SteamIntegration.Instance.UnlockAchievement(AchievementType.ACH_WINS_1);
        }

        if (Save.Game.count_wins >= 5)
        {
            SteamIntegration.Instance.UnlockAchievement(AchievementType.ACH_WINS_5);
        }

        if (Save.Game.count_wins >= 10)
        {
            SteamIntegration.Instance.UnlockAchievement(AchievementType.ACH_WINS_10);
        }
    }

    private void UnlockLoseAchievement()
    {
        LogController.LogMethod();

        if (Save.Game.count_losses >= 1)
        {
            SteamIntegration.Instance.UnlockAchievement(AchievementType.ACH_LOSE_1);
        }

        if (Save.Game.count_losses >= 5)
        {
            SteamIntegration.Instance.UnlockAchievement(AchievementType.ACH_LOSE_5);
        }

        if (Save.Game.count_losses >= 10)
        {
            SteamIntegration.Instance.UnlockAchievement(AchievementType.ACH_LOSE_10);
        }
    }

    private void UnlockWinAbilityAchievement()
    {
        LogController.LogMethod();

        var diff = DifficultyController.Instance.DifficultyIndex + 1;
        for (int i = 0; i < diff; i++)
        {
            var s_diff = i + 1;
            var ability = Save.PlayerBody.primary_ability;
            var ach_name = $"ACH_{ability.ToString().ToUpper()}_{s_diff}";
            var enums = System.Enum.GetValues(typeof(AchievementType)).Cast<AchievementType>().ToList();
            var valid_enum = enums.Any(e => e.ToString() == ach_name);

            if (valid_enum)
            {
                var value = enums.FirstOrDefault(e => e.ToString() == ach_name);
                SteamIntegration.Instance.UnlockAchievement(value);
            }
        }
    }
}
