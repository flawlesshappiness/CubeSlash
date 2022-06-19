using Flawliz.Console;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] public Transform world;

    public static GameController Instance;

    public bool IsGameStarted { get; private set; }
    public bool IsPaused { get { return PauseLock.IsLocked; } }
    public MultiLock PauseLock { get; private set; } = new MultiLock();

    private void Awake()
    {
        Instance = this;
        InitializePlayer();
        InitializeData();
        InitializeController();
        StartGame();

        PauseLock.OnLockChanged += OnPauseChanged;

        ConsoleController.Instance.RegisterCommand("UnlockAllAbilities", () => Player.Instance.UnlockAllAbilities());
        ConsoleController.Instance.RegisterCommand("Kill", () => EnemyController.Instance.KillActiveEnemies());
    }

    private void InitializePlayer()
    {
        var prefab_player = Resources.Load<GameObject>("Prefabs/Entities/Player");
        Player.Instance = Instantiate(prefab_player, world).GetComponent<Player>();
        Player.Instance.Initialize();
        Player.Instance.Health.onMin += OnPlayerDeath;
        Player.Instance.Experience.onMax += OnPlayerLevelUp;
        CameraController.Instance.Target = Player.Instance.transform;
    }

    private void InitializeData()
    {
        Data.Game.idx_level = 0;
        Data.Game.count_ability_points = 0;
        Data.SaveGameData();
    }

    private void InitializeController()
    {
        Singleton.EnsureExistence<EnemyController>();
        Singleton.EnsureExistence<ExperienceController>();
    }

    // Update is called once per frame
    void Update()
    {
        LevelUpdate();
        CheatUpdate();
    }

    private void CheatUpdate()
    {
        if(Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.Tab))
        {
            Reload();
        }
        else if (Input.GetKeyDown(KeyCode.Q) && Input.GetKey(KeyCode.Tab))
        {
            Quit();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1) && Input.GetKey(KeyCode.Tab))
        {
            Player.Instance.Experience.Value = Player.Instance.Experience.Max;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && Input.GetKey(KeyCode.Tab))
        {
            EnemyController.Instance.KillActiveEnemies();
        }
    }

    private void OnPauseChanged(bool paused)
    {
        SetTimeScale(paused ? 0 : 1);
    }

    private void SetTimeScale(float time)
    {
        Time.timeScale = PauseLock.IsLocked ? 0 : time;
    }

    private void Quit()
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
        Player.Instance.gameObject.SetActive(true);
        Player.Instance.Experience.Value = 0;
        Player.Instance.Health.Value = Player.Instance.Health.Max;
        Player.Instance.InitializeAbilities();

        Data.Game.idx_level = 0;
        time_level = Time.time + Level.Current.duration;
        ViewController.Instance.ShowView<GameView>();
    }

    private float time_level;
    private void LevelUpdate()
    {
        if (!IsGameStarted) return;
        if (Time.time < time_level) return;
        Level.Completed();
        time_level = Time.time + Level.Current.duration;
        print("Next level " + Data.Game.idx_level);
    }

    private void OnPlayerLevelUp()
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return Lerp.Value(1f, 1f, 0f, "LevelUpSlowdown", f =>
            {
                SetTimeScale(f);
            }).UnscaledTime().GetCoroutine();

            PauseLock.AddLock(nameof(GameController));
            Player.Instance.Experience.Value = Player.Instance.Experience.Min;
            ViewController.Instance.ShowView<AbilityView>(0, "Ability");
        }
    }

    public void ResumeLevel()
    {
        PauseLock.RemoveLock(nameof(GameController));
    }

    private void OnPlayerDeath()
    {
        Player.Instance.Kill();
        StartCoroutine(RestartGameCr());
    }

    private IEnumerator RestartGameCr()
    {
        yield return new WaitForSeconds(1f);
        ViewController.Instance.ShowView<DeathView>(2f);
    }
}
