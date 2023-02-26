using Flawliz.Lerp;
using System.Collections;
using UnityEngine;
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

    public void Initialize(UpgradeInfo info)
    {
        btn_upgrade_main.Icon = info.upgrade.icon;
        template_btn_upgrade_tree.gameObject.SetActive(false);
        template_img_line.gameObject.SetActive(false);

        btn_upgrade_main.Button.onSelect += OnMainButtonSelected;
        MainInfo = info;

        CreateButtons();
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
            CreateChildButtonsRec(child, position, layer + 1);
            CreateLine(current_position, position);
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
            CreateParentButtonsRec(parent, position, layer + 1);
            CreateLine(current_position, position);
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
}