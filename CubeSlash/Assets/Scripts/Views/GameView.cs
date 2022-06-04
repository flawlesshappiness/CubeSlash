using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameView : View
{
    [SerializeField] private Image img_experience;
    [SerializeField] private UIOffscreenTargetSeeker prefab_target_seeker;

    public bool Initialized { get; private set; }

    private List<UIOffscreenTargetSeeker> target_seekers;

    private bool HasSpawnedSeekers { get; set; }

    private void Start()
    {
        prefab_target_seeker.gameObject.SetActive(false);

        Player.Instance.Experience.onValueChanged += OnExperienceChanged;
        UpdateExperience(false);

        // Audio
        AudioController.Instance.TransitionTo(AudioController.Snapshot.MAIN, 0.5f);

        // Events
        EnemyController.Instance.OnEnemyKilled += OnEnemyKilled;
        EnemyController.Instance.OnEnemySpawned += OnEnemySpawned;

        // Initialized
        Initialized = true;
    }

    private void OnDestroy()
    {
        Player.Instance.Experience.onValueChanged -= OnExperienceChanged;
        EnemyController.Instance.OnEnemyKilled -= OnEnemyKilled;
        EnemyController.Instance.OnEnemySpawned -= OnEnemySpawned;
    }

    private void OnExperienceChanged()
    {
        UpdateExperience(true);
    }

    private void UpdateExperience(bool animate)
    {
        var exp = Player.Instance.Experience;
        var t = (float)exp.Value / exp.Max;
        if (animate)
        {
            Lerp.Value(0.25f, img_experience.fillAmount, t, f => img_experience.fillAmount = f, img_experience.gameObject, "fill_" + img_experience.GetInstanceID())
                .Curve(Lerp.Curve.EASE_END);
        }
        else
        {
            img_experience.fillAmount = t;
        }
    }

    private void OnEnemyKilled()
    {
        if(EnemyController.Instance.EnemiesLeft() <= 5 && !HasSpawnedSeekers)
        {
            HasSpawnedSeekers = true;
            var enemies = EnemyController.Instance.ActiveEnemies;
            foreach(var enemy in enemies)
            {
                CreateTargetSeeker(enemy.transform);
            }
        }
    }

    private void OnEnemySpawned(Enemy enemy)
    {
        if(EnemyController.Instance.EnemiesLeft() <= 5 && HasSpawnedSeekers)
        {
            CreateTargetSeeker(enemy.transform);
        }
    }

    private void CreateTargetSeeker(Transform target)
    {
        var seeker = Instantiate(prefab_target_seeker.gameObject, prefab_target_seeker.transform.parent).GetComponent<UIOffscreenTargetSeeker>();
        seeker.gameObject.SetActive(true);
        seeker.Target = target;
    }
}