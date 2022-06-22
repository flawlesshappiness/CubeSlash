using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilityVariable : MonoBehaviour
{
    [SerializeField] private CanvasGroup cvg;
    [SerializeField] private Button btn;
    [SerializeField] private UIAbilityVariablePoint prefab_point;
    [SerializeField] private Image prefab_highlight;
    [SerializeField] private Image img_arrow_left;
    [SerializeField] private Image img_arrow_right;

    private List<UIAbilityVariablePoint> points = new List<UIAbilityVariablePoint>();
    private List<Image> highlights = new List<Image>();

    private float time_input;

    public System.Action OnSelected { get; set; }
    public System.Action OnDeselected { get; set; }

    public AbilityVariable Variable { get; private set; }
    public bool Interactable { set { cvg.interactable = value; cvg.blocksRaycasts = value; } }
    private bool Selected { get; set; }
    private bool Highlighted { get; set; }

    private int points_adjust;

    private void Start()
    {
        btn.onClick.AddListener(Click);
    }

    private void Update()
    {
        HighlightUpdate();
        InputUpdate();
        ScrollUpdate();
    }

    private void HighlightUpdate()
    {
        var highlight = EventSystemController.Instance.EventSystem.currentSelectedGameObject == btn.gameObject;
        if (!Selected && Highlighted != highlight)
        {
            Highlighted = highlight;
            SetArrowsVisible(highlight);
        }
    }

    private void InputUpdate()
    {
        if (PlayerInputController.Instance.GetJoystickButtonDown(PlayerInputController.JoystickButtonType.EAST))
        {
            Cancel();
        }
    }

    private void ScrollUpdate()
    {
        if (Selected)
        {
            if (Time.unscaledTime < time_input) return;

            var hor = Input.GetAxisRaw("Horizontal");
            if (hor.Abs() > 0.25f)
            {
                if (hor > 0)
                {
                    AdjustPoints(1);
                }
                else
                {
                    AdjustPoints(-1);
                }
                time_input = Time.unscaledTime + 0.25f;
            }
            else
            {
                time_input = Time.unscaledTime;
            }
        }
    }

    private void AdjustPoints(int adjust)
    {
        var can_adjust =
            (adjust < 0 && Variable.Value + points_adjust > 0) ||
            (adjust > 0 && Variable.Value + points_adjust < Variable.Max && Player.Instance.AbilityPoints - points_adjust > 0);

        if (can_adjust)
        {
            points_adjust = points_adjust + adjust;
            UpdatePoints(Variable.Value + points_adjust);
        }
    }

    private void UpdatePoints(int value)
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i].Filled = i < value;
        }
    }

    private void SubmitPoints()
    {
        Player.Instance.AbilityPoints += points_adjust * -1;
        Variable.SetValue(Variable.Value + points_adjust);
        UpdatePoints(Variable.Value);
    }

    public void SetVariable(AbilityVariable variable)
    {
        gameObject.SetActive(variable != null);
        if (variable == null) return;

        Variable = variable;

        // Clear
        points.ForEach(p => Destroy(p.gameObject));
        points.Clear();
        highlights.ForEach(h => Destroy(h.gameObject));
        highlights.Clear();

        // Create
        for (int i = 0; i < variable.Max; i++)
        {
            var p = CreatePoint();
            p.Filled = i < variable.Value;
        }

        img_arrow_left.transform.SetAsFirstSibling();
        img_arrow_right.transform.SetAsLastSibling();

        prefab_highlight.gameObject.SetActive(false);
        prefab_point.gameObject.SetActive(false);

        Highlighted = false;
        SetSelected(false);
        SetArrowsVisible(false);
    }

    private UIAbilityVariablePoint CreatePoint()
    {
        var point = Instantiate(prefab_point.gameObject, prefab_point.transform.parent)
            .GetComponent<UIAbilityVariablePoint>();
        point.gameObject.SetActive(true);
        points.Add(point);

        var highlight = Instantiate(prefab_highlight.gameObject, prefab_highlight.transform.parent)
            .GetComponent<Image>();
        highlight.gameObject.SetActive(true);
        highlights.Add(highlight);
        return point;
    }

    private void Click()
    {
        ToggleSelected();
    }

    private void ToggleSelected()
    {
        SetSelected(!Selected);
    }

    public void SetSelected(bool selected)
    {
        Selected = selected;
        highlights.ForEach(h => h.gameObject.SetActive(selected));
        SetArrowsVisible(!selected);

        if (selected)
        {
            points_adjust = 0;
            OnSelected?.Invoke();
        }
        else
        {
            SubmitPoints();
            OnDeselected?.Invoke();
        }
    }

    private void SetArrowsVisible(bool visible)
    {
        var end = visible ? 1 : 0;
        img_arrow_left.color = img_arrow_left.color.SetA(end);
        img_arrow_right.color = img_arrow_right.color.SetA(end);
    }

    public void Cancel()
    {
        if(Selected)
        {
            points_adjust = 0;
            SetSelected(false);
        }
    }
}