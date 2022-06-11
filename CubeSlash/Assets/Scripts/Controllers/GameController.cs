using Flawliz.Console;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] public Transform world;

    public static GameController Instance;

    private LevelAsset level_prev;

    private void Awake()
    {
        Instance = this;
        InitializePlayer();
        InitializeData();
        StartLevel();

        ConsoleController.Instance.RegisterCommand("UnlockAllAbilities", () => Player.Instance.UnlockAllAbilities());
        ConsoleController.Instance.RegisterCommand("KillAll", () => EnemyController.Instance.KillAllEnemies());
    }

    private void InitializePlayer()
    {
        var prefab_player = Resources.Load<GameObject>("Prefabs/Entities/Player");
        Player.Instance = Instantiate(prefab_player, world).GetComponent<Player>();
        Player.Instance.Initialize();
        Player.Instance.Health.onMin += OnPlayerDeath;
        CameraController.Instance.Target = Player.Instance.transform;
    }

    private void InitializeData()
    {
        Data.Game.idx_level = 0;
        Data.Game.count_ability_points = 0;
        Data.SaveGameData();
    }

    // Update is called once per frame
    void Update()
    {
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
            EnemyController.Instance.KillAllEnemies();
        }
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

    public void StartLevel()
    {
        Player.Instance.gameObject.SetActive(true);
        Player.Instance.Experience.Value = 0;
        Player.Instance.InputLock.RemoveLock("NextLevel");
        Player.Instance.AbilityLock.RemoveLock("NextLevel");
        Player.Instance.Health.Value = Player.Instance.Health.Max;
        Player.Instance.InitializeAbilities();

        AITargetController.Instance.ClearArtifacts();
        AITargetController.Instance.SetArtifactOwnerCount(Player.Instance.transform, Level.Current.count_enemy_target_player);

        StartCoroutine(WaitForViewCr());

        IEnumerator WaitForViewCr()
        {
            var view = ViewController.Instance.ShowView<GameView>();
            while (!view.Initialized) yield return null;
            EnemyController.Instance.StartSpawning();
            StartCoroutine(WaitForLevelCompletedCr());
        }
    }

    private IEnumerator WaitForLevelCompletedCr()
    {
        yield return new WaitForSeconds(1);
        while(EnemyController.Instance.EnemiesLeft() > 0)
        {
            yield return null;
        }

        CompleteLevel();
    }

    public void NextLevelTransition()
    {
        // Enable player
        Player.Instance.gameObject.SetActive(true);

        // Transition
        StartCoroutine(NextLevelTransitionCr());
    }

    private IEnumerator NextLevelTransitionCr()
    {
        ViewController.Instance.CloseView();
        yield return new WaitForSeconds(1.0f);
        StartLevel();
    }

    public void CompleteLevel()
    {
        level_prev = Level.Current;
        Level.Completed();

        // Stop enemies
        EnemyController.Instance.StopSpawning();
        EnemyController.Instance.KillActiveEnemies();

        // Stop player
        Player.Instance.AbilityLock.AddLock("NextLevel");

        // Continue
        StartCoroutine(CompleteLevelCr());
    }

    private IEnumerator CompleteLevelCr()
    {
        ViewController.Instance.CloseView();
        yield return new WaitForSeconds(1f);
        if(level_prev != null && level_prev.reward_ability)
        {
            Player.Instance.InputLock.AddLock("NextLevel");
            ViewController.Instance.ShowView<UnlockAbilityView>();
        }
        else
        {
            ViewController.Instance.ShowView<LevelTransitionView>();
        }
    }

    public void AbilityMenuTransition()
    {
        StartCoroutine(AbilityMenuTransitionCr());
    }

    private IEnumerator AbilityMenuTransitionCr()
    {
        ViewController.Instance.CloseView();
        yield return new WaitForSeconds(1f);
        ViewController.Instance.ShowView<AbilityView>();
        Player.Instance.InputLock.AddLock("NextLevel"); // Disable player
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
        yield return new WaitForSeconds(4f);
        Reload();
    }
}
