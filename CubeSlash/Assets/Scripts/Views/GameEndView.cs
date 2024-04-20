using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameEndView : View
{
    [SerializeField] private TMP_Text tmp_title_win, tmp_title_lose;
    [SerializeField] private CanvasGroup cvg_title, cvg_input;

    private bool animating = true;
    private List<UnlockItem> unlocked_items = new List<UnlockItem>();

    private class UnlockItem
    {
        public Sprite sprite;
        public string title;
    }

    private void Start()
    {
        cvg_input.alpha = 0;
        cvg_title.alpha = 0;

        var run = RunController.Instance.CurrentRun;
        tmp_title_win.enabled = run.Won;
        tmp_title_lose.enabled = !run.Won;

        UnlockItems();

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return LerpEnumerator.Alpha(cvg_title, 1f, 1f).UnscaledTime();
            Lerp.Alpha(cvg_input, 0.25f, 1f).UnscaledTime();
            animating = false;
        }
    }

    private void OnEnable()
    {
        GameController.Instance.PauseLock.AddLock(nameof(GameEndView));

        PlayerInputController.Instance.Submit.Pressed += ClickContinue;
    }

    private void OnDisable()
    {
        GameController.Instance.PauseLock.RemoveLock(nameof(GameEndView));

        PlayerInputController.Instance.Submit.Pressed -= ClickContinue;
    }

    private void ClickContinue()
    {
        if (animating) return;
        animating = true;

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            SoundController.Instance.Play(SoundEffectType.sfx_ui_marima_001);

            Lerp.Alpha(cvg_title, 0.5f, 0).UnscaledTime();
            Lerp.Alpha(cvg_input, 0.5f, 0).UnscaledTime();
            yield return new WaitForSecondsRealtime(0.5f);

            foreach (var item in unlocked_items)
            {
                var next = false;
                var view = ViewController.Instance.ShowView<UnlockItemView>(0, nameof(UnlockItemView));
                view.SetTitle(item.title);
                view.SetSprite(item.sprite);
                view.OnSubmit += () => next = true;
                view.Animate();

                while (!next)
                {
                    yield return null;
                }
            }

            yield return ReturnToMainMenuCr();
        }
    }

    private IEnumerator ReturnToMainMenuCr()
    {
        GameController.Instance.EndGame();
        var fg_view = ViewController.Instance.ShowView<ForegroundView>(1, "Foreground");
        yield return new WaitForSecondsRealtime(1);
        GameController.Instance.MainMenu();
        yield return new WaitForSecondsRealtime(0.5f);
        fg_view.Close(1f);
        Close(0);
    }

    private void UnlockItems()
    {
        var run = RunController.Instance.CurrentRun;
        if (run.CurrentAreaIndex > 0)
        {
            TryUnlockRandomAbility();
        }

        if (run.CurrentAreaIndex > 0)
        {
            TryUnlockRandomBodypart();
        }

        if (run.EnemiesKilled.ContainsKey(EnemyType.BossPlant))
        {
            TryUnlockBody(PlayerBodyType.Plant);
            SteamIntegration.Instance.UnlockAchievement(AchievementType.ACH_BODY_PLANT);
        }

        if (run.EnemiesKilled.ContainsKey(EnemyType.BossCrystalEyes))
        {
            TryUnlockBody(PlayerBodyType.Meat);
            SteamIntegration.Instance.UnlockAchievement(AchievementType.ACH_BODY_MEAT);
        }

        if (run.Won && run.Gamemode.type == GamemodeType.Normal)
        {
            TryUnlockGamemode(GamemodeType.Medium);
            TryUnlockGamemode(GamemodeType.DoubleBoss);
        }

        if (run.Won && run.Gamemode.type == GamemodeType.Medium)
        {
            TryUnlockGamemode(GamemodeType.Hard);
            TryUnlockGamemode(GamemodeType.DoubleBoss);
        }

        SaveDataController.Instance.Save<GameSaveData>();
    }

    private void TryUnlockRandomAbility()
    {
        var info = AbilityController.Instance.UnlockRandomAbility();

        if (info != null)
        {
            unlocked_items.Add(new UnlockItem
            {
                sprite = info.sprite_icon,
                title = "Ability unlocked!"
            });
        }
    }

    private void TryUnlockRandomBodypart()
    {
        var info = BodypartController.Instance.UnlockRandomPart();

        if (info != null)
        {
            unlocked_items.Add(new UnlockItem
            {
                sprite = info.preview,
                title = "Body part unlocked!"
            });
        }
    }

    private void TryUnlockBody(PlayerBodyType type)
    {
        if (Save.Game.unlocked_player_bodies.Contains(type)) return;
        Save.Game.unlocked_player_bodies.Add(type);
        Save.Game.new_player_bodies.Add(type);

        var db = Database.Load<PlayerBodyDatabase>();
        var info = db.collection.FirstOrDefault(info => info.type == type);

        if (info != null)
        {
            unlocked_items.Add(new UnlockItem
            {
                sprite = info.skins[0],
                title = "Body unlocked!"
            });
        }
    }

    private void TryUnlockGamemode(GamemodeType type)
    {
        if (GamemodeController.Instance.TryUnlockGamemode(type))
        {
            var gamemode = GamemodeController.Instance.GetGamemode(type);
            unlocked_items.Add(new UnlockItem
            {
                sprite = gamemode.icon,
                title = "Gamemode unlocked!"
            });
        }
    }
}