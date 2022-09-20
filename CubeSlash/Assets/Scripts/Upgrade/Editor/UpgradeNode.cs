using System.Collections.Generic;
using UnityEngine;
using Flawliz.Node.Editor;

public class UpgradeNode : Node
{
    public UpgradeNodeData data;
    public List<UpgradeNode> parents = new List<UpgradeNode>();
    public List<UpgradeNode> children = new List<UpgradeNode>();

    public UpgradeNode(UpgradeNodeData data) : base(data._editorPosition)
    {
        this.data = data;
    }

    public void AddChild(UpgradeNode node)
    {
        if (!children.Contains(node))
        {
            children.Add(node);
        }

        if (!data.children.Contains(node.data.id))
        {
            data.children.Add(node.data.id);
        }
    }

    public void RemoveChild(UpgradeNode node)
    {
        if (children.Contains(node))
        {
            children.Remove(node);
        }

        if (data.children.Contains(node.data.id))
        {
            data.children.Remove(node.data.id);
        }
    }

    public void AddParent(UpgradeNode node)
    {
        if (!parents.Contains(node))
        {
            parents.Add(node);
        }
    }

    public void RemoveParent(UpgradeNode node)
    {
        if (parents.Contains(node))
        {
            parents.Remove(node);
        }
    }
}