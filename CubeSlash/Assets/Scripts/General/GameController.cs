using Flawliz.Console;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Flawliz.Lerp;

public class GameController : MonoBehaviour
{
    [SerializeField] public Transform world;

    public static GameController Instance;
    public static bool DAMAGE_DISABLED = false;

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

    public const string TAG_ABILITY_VIEW = "Ability";

    private GameStateType GameState { get { return GameStateController.Instance.GameState; } }

    private void Awake()
    {
        Instance = this;
        Singleton.CreateAllSingletons();
        InitializePlayer();
        ViewController.Instance.ShowView<StartView>(0);

        PauseLock.OnLockChanged += OnPauseChanged;
    }

    public void InitializePlayer()
    {
        var prefab_player = Resources.Load<GameObject>("Prefabs/Entities/Player");
        Player.Instance = Instantiate(prefab_player, world).GetComponent<Player>();
        Player.Instance.Initialize();
        Player.Instance.onDeath += OnPlayerDeath;
        Player.Instance.onLevelUp += OnPlayerLevelUp;
        CameraController.Instance.Target = Player.Instance.transform;
        Player.Instance.gameObject.SetActive(false);
    }

    public void OpenPauseView()
    {
        if (!IsGameStarted) return;
        if (PauseView.Exists) return;
        if (GameState != GameStateType.PLAYING) return;
        if (PauseLock.IsLocked) return;
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
        IsGameStarted = true;

        GameStateController.Instance.SetGameState(GameStateType.MENU);
        AreaController.Instance.StartAreaCoroutine();
        MusicController.Instance.PlayStartMusic();
        ViewController.Instance.ShowView<GameView>(1);

        onGameStart?.Invoke();
        ResumeLevel();
    }

    public void EndGame()
    {
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
        Player.Instance.ReapplyUpgrades();
        Player.Instance.ReapplyAbilities();
        onResume?.Invoke();
    }

    private void OnPlayerLevelUp()
    {
        Player.Instance.ResetExperience();
        onPlayerLevelUp?.Invoke();

        var unlock_ability = Player.Instance.CanGainAbility() && AbilityController.Instance.CanGainAbility();
        var unlock_upgrade = UpgradeController.Instance.CanUnlockUpgrade();

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            Player.Instance.PlayLevelUpFX();
            Player.PushEnemiesInArea(Player.Instance.transform.position, 12, 500);
            yield return LerpTimeScale(1.0f, 0.25f);
            GameStateController.Instance.SetGameState(GameStateType.MENU);
            PauseLevel();

            if (unlock_ability)
            {
                var view_unlock = ViewController.Instance.ShowView<UnlockAbilityView>(0, TAG_ABILITY_VIEW);
                view_unlock.OnAbilitySelected += () =>
                {
                    var view_ability = ViewController.Instance.ShowView<AbilityView>(0, TAG_ABILITY_VIEW);
                    view_ability.OnContinue += () =>
                    {
                        Player.Instance.ResetLevelsUntilAbility();
                        ResumeLevel();
                    };
                };
            }
            else if (unlock_upgrade)
            {
                var view = ViewController.Instance.ShowView<UnlockUpgradeView>(0, TAG_ABILITY_VIEW);
                view.OnUpgradeSelected += u =>
                {
                    ResumeLevel();
                    Player.Instance.OnUpgradeSelected(u);
                };
            }
        }
    }

    public void ReturnToMainMenu()
    {
        EndGame();
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            PauseLock.AddLock("MainMenu");
            GameStateController.Instance.SetGameState(GameStateType.MENU);
            var bg_view = ViewController.Instance.ShowView<BackgroundView>(0.5f, "Foreground");
            yield return new WaitForSecondsRealtime(0.5f);
            bg_view.Close(0.5f);
            PauseLock.RemoveLock("MainMenu");
            MainMenu();
        }
    }

    public void MainMenu()
    {
        IsGameStarted = false;
        IsGameEnded = false;
        GameStateController.Instance.SetGameState(GameStateType.MENU);
        Player.Instance.gameObject.SetActive(false);
        ViewController.Instance.ShowView<StartView>(0.25f);
        CameraController.Instance.SetSize(15f);

        onMainMenu?.Invoke();
    }

    private void OnPlayerDeath()
    {
        EndGame();
        SessionController.Instance.CurrentData.won = false;
        MusicController.Instance.StopBGM();
        FMODEventReferenceDatabase.Load().lose_game.Play();
        StartCoroutine(EndGameCr());
    }

    public void Win()
    {
        // Difficulty
        if(Save.Game.idx_difficulty_completed < DifficultyController.Instance.DifficultyIndex)
        {
            Save.Game.idx_difficulty_completed = DifficultyController.Instance.DifficultyIndex;
        }

        // End
        SessionController.Instance.CurrentData.won = true;
        MusicController.Instance.StopBGM();
        EnemyController.Instance.KillActiveEnemies();
        StartCoroutine(EndGameCr());
    }

    private IEnumerator EndGameCr()
    {
        Save.Game.runs_completed++;
        IsGameEnded = true;
        yield return new WaitForSeconds(1f);
        GameStateController.Instance.SetGameState(GameStateType.MENU);
        var end_view = ViewController.Instance.ShowView<EndView>(0, "End");
    }

    public void ResumeEndless()
    {
        IsGameEnded = false;
        GameStateController.Instance.SetGameState(GameStateType.PLAYING);
    }
}
