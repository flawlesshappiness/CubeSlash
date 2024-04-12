using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DebugInfoView : View
{
    public TMP_Text temp_text;

    private List<Text> texts = new List<Text>();

    private class Text
    {
        public TMP_Text TMP { get; set; }
        public Func<string> GetTextFunction { get; set; }
        public void UpdateText() => TMP.text = GetTextFunction();
    }

    private void Start()
    {
        temp_text.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        ClearTexts();
        InitializeTexts();
    }

    private void Update()
    {
        foreach (var text in texts)
        {
            text.UpdateText();
        }
    }

    private void ClearTexts()
    {
        foreach (var text in texts)
        {
            Destroy(text.TMP.gameObject);
        }
        texts.Clear();
    }

    private void InitializeTexts()
    {
        var gamemode = GamemodeController.Instance.SelectedGameMode;

        // Run
        if (RunController.Instance.CurrentRun != null)
        {
            var run = RunController.Instance.CurrentRun;
            CreateText(() => $"Run.Won: {run.Won}");
            CreateText(() => $"Run.Gamemode.type: {run.Gamemode.type}");
            CreateText(() => $"Run.CurrentAreaIndex: {run.CurrentAreaIndex}");
            CreateText(() => $"Run.TimeStart: {Time.time - run.StartTime}");
            CreateText(() => $"Run.TimeNextArea: {run.Gamemode.area_duration * (run.CurrentAreaIndex + 1)}");

            foreach (var area in run.Areas)
            {
                CreateText(() => $"Run.Areas: {area.name}");
            }

            var enemy_types = Enum.GetValues(typeof(EnemyType)).Cast<EnemyType>();
            foreach (var enemy_type in enemy_types)
            {
                CreateText(() =>
                {
                    run.EnemiesKilled.TryGetValue(enemy_type, out var i);
                    return $"{enemy_type} killed: {i}";
                });
            }
        }

        // Save
        CreateText(() => $"Save.SelectedGamemode: {Save.Game.gamemode_selected}");
        foreach (var mode in Save.Game.unlocked_gamemodes)
        {
            CreateText(() => $"Save.UnlockedGamemode: {mode}");
        }

        // Gamemode
        CreateText(() => $"EnemySpawnFrequency: {gamemode.EnemySpawnFrequency}");
        CreateText(() => $"EnemySpawnCount: {gamemode.EnemySpawnCount}");
        CreateText(() => $"EnemyCountMax: {gamemode.EnemyCountMax}");
    }

    private void CreateText(Func<string> getText)
    {
        var text = Instantiate(temp_text);
        text.gameObject.SetActive(true);
        text.transform.parent = temp_text.transform.parent;

        texts.Add(new Text
        {
            TMP = text,
            GetTextFunction = getText,
        });
    }
}