using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeTreeView : View
{
    [SerializeField] private UIUpgradeTreeButton template_btn_upgrade;
    [SerializeField] private Image template_line;
    [SerializeField] private RectTransform rt_panel;
    [SerializeField] private TMP_Text tmp_desc;

    private List<ButtonMap> maps = new List<ButtonMap>();
    private Dictionary<int, List<ButtonMap>> map_depths = new Dictionary<int, List<ButtonMap>>();

    private class ButtonMap
    {
        public UIUpgradeTreeButton button;
        public UpgradeNodeData node;
        public Upgrade upgrade;
    }

    public void SetTree(UpgradeNodeTree tree)
    {
        template_btn_upgrade.gameObject.SetActive(false);
        template_line.gameObject.SetActive(false);

        CreateButtons(tree);
        PositionButtons();
        ConnectButtons(tree);

        EventSystemController.Instance.SetDefaultSelection(maps[0].button.Button.gameObject);
        EventSystemController.Instance.EventSystem.SetSelectedGameObject(maps[0].button.Button.gameObject);
    }

    private void CreateButtons(UpgradeNodeTree tree)
    {
        var visited = new List<UpgradeNodeData>();
        var seen = new List<UpgradeNodeData> { tree.GetRootNode() };
        UpgradeNodeData current = null;

        while(seen.Count > 0)
        {
            current = seen[0];
            seen.RemoveAt(0);
            seen.AddRange(tree.GetNodeChildren(current).Where(child => !seen.Contains(child) && !visited .Contains(child)));
            visited.Add(current);
            var btn = CreateButton(current);
            var depth = tree.GetNodeDepth(current);

            if (!map_depths.ContainsKey(depth))
                map_depths.Add(depth, new List<ButtonMap>());
            map_depths[depth].Add(btn);
        }
    }

    private void PositionButtons()
    {
        var max_depth = map_depths.Keys.OrderByDescending(x => x).First();
        var size = (template_btn_upgrade.transform as RectTransform).sizeDelta;
        var spacing_delta_parent = rt_panel.sizeDelta / max_depth * 0.6f;
        var height_delta_parent = rt_panel.sizeDelta.y / max_depth * 0.3f;
        var size_delta_parent = new Vector2(height_delta_parent, height_delta_parent);

        var x_spacing = spacing_delta_parent.x;
        var y_spacing = spacing_delta_parent.y;
        var spacing = new Vector2(x_spacing, y_spacing);

        var y_start = spacing.y * (max_depth) * 0.5f;

        for (int depth = 0; depth < max_depth + 1; depth++)
        {
            if (!map_depths.ContainsKey(depth)) continue;
            var maps = map_depths[depth];
            var x_start = -spacing.x * (maps.Count - 1) * 0.5f;
            for (int i = 0; i < maps.Count; i++)
            {
                var map = maps[i];
                var rt = map.button.transform as RectTransform;
                rt.anchoredPosition = new Vector2(x_start + i * spacing.x, y_start - depth * spacing.y);
                rt.sizeDelta = size_delta_parent;
            }
        }
    }

    private void ConnectButtons(UpgradeNodeTree tree)
    {
        ConnectRec(tree.GetRootNode());
        void ConnectRec(UpgradeNodeData node)
        {
            var map_parent = maps.First(map => map.node == node);
            foreach (var child in tree.GetNodeChildren(node))
            {
                var map_child = maps.First(map => map.node == child);
                Connect(map_parent.button, map_child.button);
                ConnectRec(child);
            }
        }
    }

    private void Connect(UIUpgradeTreeButton btnA, UIUpgradeTreeButton btnB)
    {
        var line = Instantiate(template_line, template_line.transform.parent);
        line.gameObject.SetActive(true);

        var dir = btnB.transform.position - btnA.transform.position;
        var rt = line.transform as RectTransform;
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, dir.magnitude);

        var angle = Vector3.SignedAngle(Vector3.up, dir, Vector3.forward);
        rt.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        var position = Vector3.Lerp(btnA.transform.position, btnB.transform.position, 0.5f);
        rt.position = position;
    }

    private ButtonMap CreateButton(UpgradeNodeData node)
    {
        var info = UpgradeController.Instance.GetUpgrade(node.id_name);
        var upgrade = info.upgrade;

        var btn = Instantiate(template_btn_upgrade, template_btn_upgrade.transform.parent);
        btn.gameObject.SetActive(true);
        btn.SetUpgrade(info);

        var map = new ButtonMap
        {
            button = btn,
            node = node,
            upgrade = upgrade
        };

        btn.Button.OnSelectedChanged += selected =>
        {
            if (selected)
            {
                DisplayUpgrade(upgrade);
            }
        };

        maps.Add(map);
        return map;
    }

    private void DisplayUpgrade(Upgrade upgrade)
    {
        var text = upgrade.name.Color(ColorPalette.Main.Get(ColorPalette.Type.PRIMARY));

        for (int i = 0; i < upgrade.effects.Count; i++)
        {
            var effect = upgrade.effects[i];
            var color = effect.type_effect == Upgrade.Effect.TypeEffect.POSITIVE ? ColorPalette.Type.HIGHLIGHT : ColorPalette.Type.WRONG;
            text += "\n" + effect.variable.GetDisplayString(true).Color(ColorPalette.Main.Get(color));
        }

        tmp_desc.text = text;
    }
}