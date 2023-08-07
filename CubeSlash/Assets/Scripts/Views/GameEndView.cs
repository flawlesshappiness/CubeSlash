using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameEndView : View
{
    [SerializeField] private TMP_Text tmp_title_win, tmp_title_lose;
    [SerializeField] private CanvasGroup cvg_title;
    [SerializeField] private UIInputLayout input;

    private bool animating = true;
    private List<UnlockItem> unlocked_items = new List<UnlockItem>();

    private class UnlockItem
    {
        public Sprite sprite;
        public string title;
    }

    private void Start()
    {
        input.Clear();
        input.AddInput(PlayerInput.UIButtonType.SOUTH, "Continue");
        input.CanvasGroup.alpha = 0;

        cvg_title.alpha = 0;

        var data = SessionController.Instance.CurrentData;
        tmp_title_win.enabled = data.won;
        tmp_title_lose.enabled = !data.won;

        UnlockItems();

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return LerpEnumerator.Alpha(cvg_title, 1f, 1f).UnscaledTime();
            Lerp.Alpha(input.CanvasGroup, 0.25f, 1f).UnscaledTime();
            animating = false;
        }
    }

    private void OnEnable()
    {
        GameController.Instance.PauseLock.AddLock(nameof(GameEndView));

        PlayerInput.Controls.UI.Submit.started += ClickContinue;
    }

    private void OnDisable()
    {
        GameController.Instance.PauseLock.RemoveLock(nameof(GameEndView));

        PlayerInput.Controls.UI.Submit.started -= ClickContinue;
    }

    private void ClickContinue(InputAction.CallbackContext ctx)
    {
        if (animating) return;
        animating = true;

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            SoundController.Instance.Play(SoundEffectType.sfx_ui_marima_001);

            Lerp.Alpha(cvg_title, 0.5f, 0).UnscaledTime();
            Lerp.Alpha(input.CanvasGroup, 0.5f, 0).UnscaledTime();
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
        var data = SessionController.Instance.CurrentData;
        if (data.areas_completed >= 2 || data.levels_gained > 8)
        {
            UnlockRandomAbility();
        }

        if (data.areas_completed >= 2 || data.levels_gained > 4)
        {
            UnlockRandomBodypart();
        }

        if (data.bosses_killed.Contains(EnemyType.BossPlant))
        {
            UnlockBody(PlayerBodyType.Plant);
        }

        if (data.bosses_killed.Contains(EnemyType.BossCrystalEyes))
        {
            UnlockBody(PlayerBodyType.Meat);
        }

        if (data.won && Save.Game.idx_difficulty_completed < DifficultyController.Instance.DifficultyIndex)
        {
            UnlockDifficulty();
        }
    }

    private void UnlockRandomAbility()
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

    private void UnlockRandomBodypart()
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

    private void UnlockBody(PlayerBodyType type)
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
                title = "Body unlocked"
            });
        }
    }

    private void UnlockDifficulty()
    {
        Save.Game.idx_difficulty_completed++;
        var info = DifficultyController.Instance.GetInfo(Save.Game.idx_difficulty_completed + 1);

        if (info != null)
        {
            unlocked_items.Add(new UnlockItem
            {
                sprite = info.difficulty_sprite,
                title = "Difficulty unlocked"
            });
        }
    }
}