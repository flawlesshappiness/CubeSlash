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
    public float TimeGameStart { get; private set; }
    public float TimeGameEnd { get; private set; }

    public System.Action onResume { get; set; }
    public System.Action onGameEnd { get; set; }
    public System.Action onMainMenu { get; set; }

    public const string TAG_ABILITY_VIEW = "Ability";

    private GameStateType GameState { get { return GameStateController.Instance.GameState; } }

    private void Awake()
    {
        Instance = this;
        InitializePlayer();
        InitializeData();
        InitializeController();
        ViewController.Instance.ShowView<StartView>(0);

        PauseLock.OnLockChanged += OnPauseChanged;
    }

    private void OnToggleConsole(bool toggle)
    {
        if (toggle)
        {
            PauseLock.AddLock("Console");
        }
        else
        {
            PauseLock.RemoveLock("Console");
        }
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

    private void InitializeData()
    {
        Data.SaveGameData();
    }

    private void InitializeController()
    {
        Singleton.EnsureExistence<DebugConsoleHandler>();
        Singleton.EnsureExistence<EnemyController>();
        Singleton.EnsureExistence<ItemController>();
        Singleton.EnsureExistence<PlayerInputController>();
        Singleton.EnsureExistence<BackgroundController>();
        Singleton.EnsureExistence<MusicController>();
        Singleton.EnsureExistence<VignetteController>();
        Singleton.EnsureExistence<ObjectSpawnController>();
        Singleton.EnsureExistence <AudioController>();
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

    private void SetTimeScale(float time)
    {
        Time.timeScale = PauseLock.IsLocked ? 0 : time;
    }

    private Lerp LerpTimeScale(float duration, float end)
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
        TimeGameStart = Time.time;

        GameStateController.Instance.SetGameState(GameStateType.MENU);
        AreaController.Instance.StartAreaCoroutine();
        MusicController.Instance.PlayStartMusic();
        ViewController.Instance.ShowView<GameView>(1);
        ResumeLevel();
    }

    private void EndGame()
    {
        IsGameEnded = true;
        TimeGameEnd = Time.time;
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

        //Debug.Log($"{Player.Instance.LevelsUntilAbility} {Player.Instance.CanGainAbility()} {AbilityController.Instance.CanUnlockAbility()}");
        if (Player.Instance.CanGainAbility() && AbilityController.Instance.CanUnlockAbility())
        {
            Player.Instance.ResetLevelsUntilAbility();
            UnlockAbility();
        }
        else if (UpgradeController.Instance.CanUnlockUpgrade())
        {
            UnlockUpgrade();
        }
    }

    private void UnlockUpgrade()
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return LerpTimeScale(2f, 0.25f);
            GameStateController.Instance.SetGameState(GameStateType.MENU);
            PauseLevel();
            var view = ViewController.Instance.ShowView<UnlockUpgradeView>(0, TAG_ABILITY_VIEW);
            view.OnUpgradeSelected += u =>
            {
                ResumeLevel();
                Player.Instance.OnUpgradeSelected(u);
            };
        }
    }

    private void UnlockAbility()
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return LerpTimeScale(2f, 0.25f);
            GameStateController.Instance.SetGameState(GameStateType.MENU);
            PauseLevel();
            var view_unlock = ViewController.Instance.ShowView<UnlockAbilityView>(0, TAG_ABILITY_VIEW);
            view_unlock.OnAbilitySelected += () =>
            {
                var view_ability = ViewController.Instance.ShowView<AbilityView>(0, TAG_ABILITY_VIEW);
                view_ability.OnContinue += () =>
                {
                    ResumeLevel();
                };
            };
        }
    }

    private void OnPlayerDeath()
    {
        EndGame();
        StartCoroutine(Cr());

        IEnumerator Cr()
        {
            GameStateController.Instance.SetGameState(GameStateType.MENU);
            MusicController.Instance.StopBGM();
            FMODEventReferenceDatabase.Load().lose_game.Play();
            yield return new WaitForSeconds(0.5f);
            var death_view = ViewController.Instance.ShowView<DeathView>(2f, "Death");
            death_view.AnimateScaleTitle(6);
            yield return new WaitForSeconds(2.0f);
            var bg_view = ViewController.Instance.ShowView<BackgroundView>(2.0f, "Background");
            yield return new WaitForSeconds(3.0f);
            death_view.Close(1.0f);
            yield return new WaitForSeconds(1.0f);
            bg_view.Close(0.5f);
            MainMenu();
        }
    }

    public void ReturnToMainMenu()
    {
        EndGame();
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            GameStateController.Instance.SetGameState(GameStateType.MENU);
            var bg_view = ViewController.Instance.ShowView<BackgroundView>(0.5f, "Background");
            yield return new WaitForSeconds(0.5f);
            bg_view.Close(0.5f);
            MainMenu();
        }
    }

    private void MainMenu()
    {
        IsGameStarted = false;
        IsGameEnded = false;
        GameStateController.Instance.SetGameState(GameStateType.MENU);
        Player.Instance.gameObject.SetActive(false);
        ViewController.Instance.ShowView<StartView>(0.25f);
        CameraController.Instance.SetSize(15f);

        onMainMenu?.Invoke();
    }

    public void Win()
    {
        EndGame();
        MusicController.Instance.StopBGM();
        EnemyController.Instance.KillActiveEnemies();
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            CameraController.Instance.AnimateSize(8f, 25f);
            yield return new WaitForSeconds(2f);
            // Show view
            var win_view = ViewController.Instance.ShowView<WinView>(2.0f, "Win");
            win_view.AnimateScaleTitle(6);
            yield return new WaitForSeconds(2f);
            var bg_view = ViewController.Instance.ShowView<BackgroundView>(2.0f, "Background");
            yield return new WaitForSeconds(3f);
            win_view.Close(2.0f);
            yield return new WaitForSeconds(2f);
            bg_view.Close(0.5f);
            MainMenu();
        }
    }
}
