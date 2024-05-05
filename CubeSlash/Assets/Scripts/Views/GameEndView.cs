using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameEndView : View
{
    [SerializeField] private TMP_Text tmp_title_win, tmp_title_lose;
    [SerializeField] private TMP_Text tmp_endless_current, tmp_endless_highscore;
    [SerializeField] private RadialMenu radial;
    [SerializeField] private CanvasGroup cvg_title, cvg_input, cvg_endless;
    [SerializeField] private Sprite sp_endless, sp_exit;

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
        cvg_endless.alpha = 0;

        tmp_endless_current.text = RunInfo.Current.EndlessDuration.ToString("0.0");
        tmp_endless_highscore.text = Save.Game.endless_highscore.ToString("0.0");

        var run = RunController.Instance.CurrentRun;
        tmp_title_win.enabled = run.Won;
        tmp_title_lose.enabled = !run.Won;

        radial.Clear();
        radial.gameObject.SetActive(false);

        UnlockItems();

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            animating = true;
            yield return LerpEnumerator.Alpha(cvg_title, 1f, 1f).UnscaledTime();
            Lerp.Alpha(cvg_input, 0.25f, 1f).UnscaledTime();

            if (RunInfo.Current.Endless)
            {
                Lerp.Alpha(cvg_endless, 0.25f, 1f).UnscaledTime();
            }

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
            Lerp.Alpha(cvg_endless, 0.5f, 0).UnscaledTime();
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

            var run = RunController.Instance.CurrentRun;
            if (run.Won)
            {
                ShowWinRadial();
            }
            else
            {
                ReturnToMainMenu();
            }
        }
    }

    private void ReturnToMainMenu()
    {
        animating = true;
        GameController.Instance.ReturnToMainMenu();
        Close(0);
    }

    private void StartEndless()
    {
        GameController.Instance.StartEndless();
        Close(0);
    }

    private void UnlockItems()
    {
        var run = RunController.Instance.CurrentRun;

        // Ability
        if (run.CurrentAreaIndex > 0)
        {
            TryUnlockRandomAbility();
        }

        // Bodypart
        if (run.CurrentAreaIndex > 0)
        {
            TryUnlockRandomBodypart();
        }

        // Plant body
        if (run.EnemiesKilled.ContainsKey(EnemyType.BossPlant))
        {
            TryUnlockBody(PlayerBodyType.Plant);
            SteamIntegration.Instance.UnlockAchievement(AchievementType.ACH_BODY_PLANT);
        }

        // Meat body
        if (run.EnemiesKilled.ContainsKey(EnemyType.BossCrystalEyes))
        {
            TryUnlockBody(PlayerBodyType.Meat);
            SteamIntegration.Instance.UnlockAchievement(AchievementType.ACH_BODY_MEAT);
        }

        // Jelly body
        if (run.EnemiesKilled.ContainsKey(EnemyType.BossJelly))
        {
            TryUnlockBody(PlayerBodyType.Jelly);
            SteamIntegration.Instance.UnlockAchievement(AchievementType.ACH_BODY_JELLY);
        }

        // Root body
        if (run.EnemiesKilled.ContainsKey(EnemyType.BossRoot))
        {
            TryUnlockBody(PlayerBodyType.Root);
            SteamIntegration.Instance.UnlockAchievement(AchievementType.ACH_BODY_ROOT);
        }

        // Medium & Double gamemode
        if (run.Won && run.Gamemode.type == GamemodeType.Normal)
        {
            TryUnlockGamemode(GamemodeType.Medium);
            TryUnlockGamemode(GamemodeType.DoubleBoss);
        }

        // Hard & Double gamemode
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

    private void ShowWinRadial()
    {
        var options = new List<RadialMenuOption>
        {
            new RadialMenuOption
            {
                Title = "Endless",
                Description = "Mode",
                Sprite = sp_endless,
                OnSubmitComplete = () => StartEndless()
            },

            new RadialMenuOption
            {
                Title = "Main",
                Description = "Menu",
                Sprite = sp_exit,
                OnSubmitComplete = () => ReturnToMainMenu()
            }
        };

        radial.gameObject.SetActive(true);
        radial.Clear();
        radial.AddOptions(options);
        radial.AnimateShowElements(true, 0.05f);
    }
}