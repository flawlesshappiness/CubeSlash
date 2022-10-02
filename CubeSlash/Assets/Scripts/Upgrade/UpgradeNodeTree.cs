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
    public int id_node = 0;

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

    public int GetNodeDepth(UpgradeNodeData node)
    {
        return RecSearch(GetRootNode(), 0);
        int RecSearch(UpgradeNodeData current, int depth)
        {
            if (node == current) return depth;
            var result = depth;
            foreach (var child in GetNodeChildren(current))
            {
                var d = RecSearch(child, depth + 1);
                if (d > result) result = d;
            }
            if (result > depth) return result;
            return -1;
        }
    }

    public bool Contains(Upgrade upgrade) => Contains(upgrade.id);
    public bool Contains(string id) => nodes.Any(node => node.id_name == id);
}