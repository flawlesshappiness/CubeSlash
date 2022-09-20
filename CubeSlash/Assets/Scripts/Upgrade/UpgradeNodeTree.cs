using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/"+nameof(UpgradeNodeTree), fileName = nameof(UpgradeNodeTree), order = 0)]
[System.Serializable]
public class UpgradeNodeTree : ScriptableObject
{
    public bool require_ability;
    public Ability.Type ability_type_required;
    public List<UpgradeNodeData> nodes = new List<UpgradeNodeData>();
    private int id_node = 0;

    public UpgradeNodeData GetNode(int id) => nodes.FirstOrDefault(node => node.id == id);
    public UpgradeNodeData CreateNodeData()
    {
        var node = new UpgradeNodeData();
        node.id = id_node++;
        node.id_name = "Node" + node.id.ToString("000");
        nodes.Add(node);
        return node;
    }

    public void Clear()
    {
        id_node = 0;
        nodes.Clear();
    }

    public UpgradeNodeData GetRootNode()
    {
        return nodes.FirstOrDefault(node => node.id == 0);
    }

    public List<UpgradeNodeData> GetNodeChildren(UpgradeNodeData node)
    {
        return node.children.Select(id => GetNode(id)).ToList();
    }

    public bool Contains(Upgrade upgrade) => Contains(upgrade.id);
    public bool Contains(string id) => nodes.Any(node => node.id_name == id);
}