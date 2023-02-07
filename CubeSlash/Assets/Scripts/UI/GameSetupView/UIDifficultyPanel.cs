using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDifficultyPanel : MonoBehaviour
{
    [SerializeField] private LeftRightMenuItem menu;
    [SerializeField] private UIDifficulty template_difficulty;
    [SerializeField] private GameObject template_divider;
    [SerializeField] private RectTransform pivot_selected;
    [SerializeField] private FMODEventReference sfx_move;

    private List<UIDifficulty> difficulties = new List<UIDifficulty>();

    private int idx_selected;
    public DifficultyInfo Selected { get; private set; }

    private void Start()
    {
        InitializeDifficulties();

        menu.onMove += OnMove;

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return null;
            var d = difficulties[idx_selected];
            pivot_selected.anchoredPosition = d.RectTransform.anchoredPosition;
            pivot_selected.sizeDelta = d.RectTransform.sizeDelta;
            SetDifficulty(0);
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
            difficulties.Add(inst);

            if(i < db.collection.Count - 1)
            {
                var div = Instantiate(template_divider, template_divider.transform.parent);
                div.gameObject.SetActive(true);
            }
        }
    }

    private void OnMove(int dir)
    {
        var idx_prev = idx_selected;
        idx_selected = Mathf.Clamp(idx_selected + dir, 0, difficulties.Count - 1);

        SetDifficulty(idx_selected);

        if(idx_selected != idx_prev)
        {
            sfx_move.Play();
        }
    }

    private void SetDifficulty(int i)
    {
        idx_selected = i;
        var d = difficulties[i];
        Selected = d.Difficulty;

        Lerp.AnchoredPosition(pivot_selected, 0.15f, d.RectTransform.anchoredPosition);
        Lerp.SizeDelta(pivot_selected, 0.15f, d.RectTransform.sizeDelta);
    }
}