using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameView : View
{
    [SerializeField] private TMP_Text tmp_enemies_killed;
    [SerializeField] private TMP_Text tmp_player_hurt;

    private int count_enemy_kill;
    private int count_player_hurt;

    private void Start()
    {
        Player.Instance.onEnemyKilled += OnEnemyKilled;
        Player.Instance.onHurt += OnPlayerHurt;
        UpdateEnemyKillText();
        UpdatePlayerHurtText();
    }

    private void OnDestroy()
    {
        Player.Instance.onEnemyKilled -= OnEnemyKilled;
        Player.Instance.onHurt -= OnPlayerHurt;
    }

    private void OnEnemyKilled(Enemy enemy)
    {
        count_enemy_kill++;
        UpdateEnemyKillText();
    }

    private void UpdateEnemyKillText()
    {
        tmp_enemies_killed.text = string.Format("ENEMIES KILLED: {0}", count_enemy_kill);
    }

    private void OnPlayerHurt(Enemy enemy)
    {
        count_player_hurt++;
        UpdatePlayerHurtText();
    }

    private void UpdatePlayerHurtText()
    {
        tmp_player_hurt.text = string.Format("PLAYER HURT: {0}", count_player_hurt);
    }
}