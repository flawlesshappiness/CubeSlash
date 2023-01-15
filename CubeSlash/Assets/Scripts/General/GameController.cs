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

    public enum GameState { PLAYING, MENU }
    public GameState gameState = GameState.PLAYING;

    public System.Action OnResume { get; set; }
    public bool IsGameStarted { get; private set; }
    public bool IsGameEnded { get; private set; }
    public bool IsPaused { get { return PauseLock.IsLocked; } }
    public MultiLock PauseLock { get; private set; } = new MultiLock();
    public int LevelIndex { get; set; }

    public System.Action OnNextLevel { get; set; }
    public System.Action OnMainMenu { get; set; }

    public const string TAG_ABILITY_VIEW = "Ability";

    private void Awake()
    {
        Instance = this;
        InitializePlayer();
        InitializeData();
        InitializeController();
        ViewController.Instance.ShowView<StartView>(0);

        PauseLock.OnLockChanged += OnPauseChanged;

        ConsoleController.Instance.RegisterCommand("UnlockAllAbilities", AbilityController.Instance.UnlockAllAbilities);
        ConsoleController.Instance.RegisterCommand("Kill", EnemyController.Instance.RemoveActiveEnemies);
        ConsoleController.Instance.RegisterCommand("LevelUp", CheatLevelUp);
        ConsoleController.Instance.RegisterCommand("NextLevel", () => SetLevel(LevelIndex + 1));
        ConsoleController.Instance.RegisterCommand("Equipment", CheatOpenEquipment);
        ConsoleController.Instance.RegisterCommand("ToggleDamage", () => DAMAGE_DISABLED = !DAMAGE_DISABLED);
        ConsoleController.Instance.RegisterCommand("Suicide", () => Player.Instance.Kill());
        ConsoleController.Instance.onToggle += OnToggleConsole;
        ConsoleController.Instance.Enabled = false;
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

    private void InitializePlayer()
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
        Singleton.EnsureExistence<ProgressController>();
        Singleton.EnsureExistence<MusicController>();
        Singleton.EnsureExistence<VignetteController>();
    }

    public void OpenPauseView()
    {
        if (!IsGameStarted) return;
        if (PauseView.Exists) return;
        if (gameState != GameState.PLAYING) return;
        if (PauseLock.IsLocked) return;
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
        gameState = GameState.MENU;
        IsGameStarted = true;
        Player.Instance.ResetValues();
        AbilityController.Instance.Clear();
        UpgradeController.Instance.ClearUpgrades();
        Player.Instance.gameObject.SetActive(true);

        PauseLevel();
        var view_unlock = ViewController.Instance.ShowView<UnlockAbilityView>(0);
        view_unlock.OnAbilitySelected += () =>
        {
            var view_ability = ViewController.Instance.ShowView<AbilityView>(0);
            view_ability.OnContinue += () =>
            {
                ViewController.Instance.ShowView<GameView>(1);
                SetLevel(0);
                ResumeLevel();
                MusicController.Instance.PlayStartMusic();
            };
        };
    }

    private void PauseLevel()
    {
        PauseLock.AddLock(nameof(GameController));
    }

    public void ResumeLevel()
    {
        gameState = GameState.PLAYING;
        PauseLock.RemoveLock(nameof(GameController));
        Player.Instance.ReapplyUpgrades();
        Player.Instance.ReapplyAbilities();
        OnResume?.Invoke();
    }

    private Coroutine _cr_next_level;
    private IEnumerator NextLevelCr()
    {
        while (true)
        {
            yield return new WaitForSeconds(Level.Current.duration);
            Level.Completed();
            OnNextLevel?.Invoke();
        }
    }

    private void SetLevel(int idx)
    {
        if(_cr_next_level != null)
        {
            StopCoroutine(_cr_next_level);
        }

        LevelIndex = idx;
        OnNextLevel?.Invoke();
        _cr_next_level = StartCoroutine(NextLevelCr());
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
            yield return LerpTimeScale(2f, 0f);
            gameState = GameState.MENU;
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
            yield return LerpTimeScale(2f, 0f);
            gameState = GameState.MENU;
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
        StartCoroutine(Cr());

        IEnumerator Cr()
        {
            gameState = GameState.MENU;
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
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            gameState = GameState.MENU;
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
        gameState = GameState.MENU;
        EnemyController.Instance.RemoveActiveEnemies();
        ItemController.Instance.DespawnAllActiveItems();
        Player.Instance.gameObject.SetActive(false);
        ViewController.Instance.ShowView<StartView>(0.25f);
        CameraController.Instance.SetSize(15f);

        OnMainMenu?.Invoke();
    }

    private void CheatLevelUp()
    {
        Player.Instance.Experience.Value = Player.Instance.Experience.Max;
    }

    private void CheatOpenEquipment()
    {
        SetTimeScale(0);
        var view = ViewController.Instance.ShowView<AbilityView>(0, TAG_ABILITY_VIEW);
        view.OnContinue += () =>
        {
            ResumeLevel();
        };
    }

    public void Win()
    {
        MusicController.Instance.StopBGM();
        IsGameEnded = true;
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
