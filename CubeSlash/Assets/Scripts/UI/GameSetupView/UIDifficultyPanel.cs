using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDifficultyPanel : MonoBehaviour
{
    [SerializeField] private LeftRightMenuItem menu;
    [SerializeField] private UIDifficulty template_difficulty;
    [SerializeField] private GameObject template_divider;
    [SerializeField] private TMP_Text tmp_unlock;
    [SerializeField] private RectTransform pivot_selected, content;

    private List<UIDifficulty> difficulties = new List<UIDifficulty>();

    private int idx_selected;
    public DifficultyInfo Selected { get; private set; }
    public int SelectedIndex { get { return idx_selected; } }

    private void Start()
    {
        InitializeDifficulties();

        menu.onMove += OnMove;

        var first_run = Save.Game.runs_completed <= 0;
        tmp_unlock.enabled = first_run;
        content.gameObject.SetActive(!first_run);
        menu.interactable = !first_run;

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return null;
            var d = difficulties[idx_selected];
            pivot_selected.anchoredPosition = d.RectTransform.anchoredPosition;
            pivot_selected.sizeDelta = d.RectTransform.sizeDelta;
            SetDifficulty(Save.Game.idx_gamesetup_difficulty);
        }
    }

    private void InitializeDifficulties()
    {
        template_difficulty.gameObject.SetActive(false);
        template_divider.gameObject.SetActive(false);

        var db = Database.Load<DifficultyDatabase>();
        for (int i = 0; i < db.collection.Count; i++)
        {
            var d = db.collection[i];

            var inst = Instantiate(template_difficulty, template_difficulty.transform.parent);
            inst.gameObject.SetActive(true);
            inst.Initialize(d);

            var is_locked = Save.Game.idx_difficulty_completed < (i - 1);
            inst.SetLocked(is_locked);

            if (!is_locked)
            {
                difficulties.Add(inst);
            }

            if (i < db.collection.Count - 1)
            {
                var div = Instantiate(template_divider, template_divider.transform.parent);
                div.gameObject.SetActive(true);
            }
        }
    }

    private void OnMove(int dir)
    {
        var idx_prev = idx_selected;
        SetDifficulty(idx_selected + dir);

        if(idx_selected != idx_prev)
        {
            SoundController.Instance.Play(SoundEffectType.sfx_ui_move);
        }
    }

    private void SetDifficulty(int i)
    {
        idx_selected = Mathf.Clamp(i, 0, difficulties.Count - 1);
        var d = difficulties[idx_selected];
        Selected = d.Difficulty;

        Save.Game.idx_gamesetup_difficulty = idx_selected;

        Lerp.AnchoredPosition(pivot_selected, 0.15f, d.RectTransform.anchoredPosition);
        Lerp.SizeDelta(pivot_selected, 0.15f, d.RectTransform.sizeDelta);

    }
}