using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class UpgradeNodeTree
{
    public List<UpgradeNode> nodes = new List<UpgradeNode>();
    private int id_node = 0;

    public UpgradeNode GetNode(int id) => nodes.FirstOrDefault(node => node.id == id);
    public UpgradeNode CreateNode()
    {
        var node = new UpgradeNode();
        node.id = id_node++;
        node.id_data = "Node" + node.id.ToString("000");
        nodes.Add(node);
        return node;
    }

    public void Clear()
    {
        id_node = 0;
        nodes.Clear();
    }
}