using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIScrollableUpgradeTree : MonoBehaviour
{
    [SerializeField] private ScrollRect scroll_rect;
    [SerializeField] private UIIconButton btn_upgrade_main, template_btn_upgrade_tree;
    [SerializeField] private Image template_img_line;
    [SerializeField] private CanvasGroup cvg_children;

    private UpgradeInfo MainInfo { get; set; }
    public UIIconButton MainButton { get { return btn_upgrade_main; } }

    public event System.Action onMainButtonSelected;
    public event System.Action<UpgradeInfo> onUpgradeSelected;

    private List<UIIconButton> btns_children = new List<UIIconButton>();
    private List<GameObject> lines_children = new List<GameObject>();
    private List<UIIconButton> btns_parents = new List<UIIconButton>();
    private List<GameObject> lines_parents = new List<GameObject>();

    public void Initialize(UpgradeInfo info)
    {
        btn_upgrade_main.Icon = info.upgrade.icon;
        template_btn_upgrade_tree.gameObject.SetActive(false);
        template_img_line.gameObject.SetActive(false);

        btn_upgrade_main.Button.onSelect += OnMainButtonSelected;
        MainInfo = info;

        CreateButtons();

        MainButton.AnimationPivot.localScale = Vector3.zero;
        btns_children.ForEach(btn => btn.AnimationPivot.localScale = Vector3.zero);
        btns_parents.ForEach(btn => btn.AnimationPivot.localScale = Vector3.zero);
        lines_children.ForEach(line => line.transform.localScale = Vector3.zero);
        lines_parents.ForEach(line => line.transform.localScale = Vector3.zero);
    }

    private void CreateButtons()
    {
        CreateChildButtonsRec(MainInfo, btn_upgrade_main.transform.localPosition, 1);
        CreateParentButtonsRec(MainInfo, btn_upgrade_main.transform.localPosition, 1);
    }

    private void CreateChildButtonsRec(UpgradeInfo current_info, Vector3 current_position, int layer)
    {
        var children = UpgradeController.Instance.GetChildUpgrades(current_info);
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var i_max = children.Count - 1;
            var btn = CreateTreeButton(child, i, i_max, layer, current_position, -1);
            var position = btn.transform.localPosition;
            var line = CreateLine(current_position, position);

            btns_children.Add(btn);
            lines_children.Add(line);

            CreateChildButtonsRec(child, position, layer + 1);
        }
    }

    private void CreateParentButtonsRec(UpgradeInfo current_info, Vector3 current_position, int layer)
    {
        var parent_types = current_info.upgrade.upgrades_required;
        for (int i = 0; i < parent_types.Count; i++)
        {
            var parent = UpgradeController.Instance.GetUpgradeInfo(parent_types[i]);
            var i_max = parent_types.Count - 1;
            var btn = CreateTreeButton(parent, i, i_max, layer, current_position, 1);
            var position = btn.transform.localPosition;
            var line = CreateLine(current_position, position);

            btns_parents.Add(btn);
            lines_parents.Add(line);

            CreateParentButtonsRec(parent, position, layer + 1);
        }
    }

    private UIIconButton CreateTreeButton(UpgradeInfo info, int i, int i_max, int layer, Vector3 position, int y_mul)
    {
        var dist_x = 50;
        var dist_y = 75;
        var max_length = dist_x * i_max;
        var x = (dist_x * i) - (max_length * 0.5f);
        var y = (layer == 1 ? 150 : dist_y) * y_mul;

        var btn = Instantiate(template_btn_upgrade_tree, template_btn_upgrade_tree.transform.parent);
        btn.gameObject.SetActive(true);
        btn.transform.localPosition = position + new Vector3(x, y);
        btn.Icon = info.upgrade.icon;

        btn.Button.onSelect += () => OnButtonSelected(btn);
        btn.Button.onSelect += () => OnUpgradeSelected(info);

        return btn;
    }

    private GameObject CreateLine(Vector3 start, Vector3 end)
    {
        var dir = end - start;
        var length = dir.magnitude;
        var width = 8;
        var angle = Vector3.SignedAngle(Vector3.up, dir, Vector3.forward);

        var position = Vector3.Lerp(start, end, 0.5f);
        var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        var sizeDelta = new Vector2(width, length);

        var line = Instantiate(template_img_line, template_img_line.transform.parent);
        line.gameObject.SetActive(true);

        var rt = line.GetComponent<RectTransform>();

        line.transform.localPosition = position;
        line.transform.rotation = rotation;
        rt.sizeDelta = sizeDelta;

        return line.gameObject;
    }

    public void SetChildUpgradesVisible(bool visible, float duration = 0.1f)
    {
        var a = visible ? 1 : 0.25f;
        cvg_children.interactable = visible;

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return LerpEnumerator.Alpha(cvg_children, duration, a).UnscaledTime();
        }
    }

    public void ScrollToPosition(Vector3 position)
    {
        var start_position = scroll_rect.transform.position;
        var dir_to_target = position - start_position;
        var end_position = scroll_rect.content.position - dir_to_target;
        Lerp.Position(scroll_rect.content, 0.25f, end_position)
            .UnscaledTime()
            .Curve(EasingCurves.EaseOutQuad);
    }

    private void OnButtonSelected(UIIconButton btn)
    {
        ScrollToPosition(btn.transform.position);
    }

    private void OnUpgradeSelected(UpgradeInfo info)
    {
        onUpgradeSelected?.Invoke(info);
    }

    private void OnMainButtonSelected()
    {
        OnButtonSelected(MainButton);
        OnUpgradeSelected(MainInfo);
        onMainButtonSelected?.Invoke();
    }

    public void Select()
    {
        MainButton.Button.Select();
    }

    public Coroutine AnimateShowMainButton()
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return LerpEnumerator.LocalScale(btn_upgrade_main.AnimationPivot, 0.25f, Vector3.zero, Vector3.one)
                .UnscaledTime()
                .Curve(EasingCurves.EaseOutBack);
        }
    }

    public Coroutine AnimateShowChildButtons()
    {
        return AnimateShowTreeButtons(btns_children, lines_children);
    }

    public Coroutine AnimateShowParentButtons()
    {
        return AnimateShowTreeButtons(btns_parents, lines_parents);
    }

    private Coroutine AnimateShowTreeButtons(List<UIIconButton> btns, List<GameObject> lines)
    {
        return StartCoroutine(AnimateChildrenCr());
        IEnumerator AnimateChildrenCr()
        {
            Coroutine last = null;
            for (int i = 0; i < btns.Count; i++)
            {
                var btn = btns[i];
                var line = lines[i];
                last = StartCoroutine(AnimateChildCr(btn, line));
                yield return new WaitForSecondsRealtime(0.1f);
            }

            yield return last;
        }

        IEnumerator AnimateChildCr(UIIconButton btn, GameObject line)
        {
            yield return LerpEnumerator.LocalScale(line.transform, 0.25f, Vector3.zero, Vector3.one)
                .UnscaledTime()
                .Curve(EasingCurves.EaseOutQuad);

            yield return LerpEnumerator.LocalScale(btn.AnimationPivot, 0.25f, Vector3.zero, Vector3.one)
                .UnscaledTime()
                .Curve(EasingCurves.EaseOutBack);
        }
    }
}