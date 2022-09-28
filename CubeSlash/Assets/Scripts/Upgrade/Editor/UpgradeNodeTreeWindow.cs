using Flawliz.Node.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class UpgradeNodeTreeWindow : NodeEditorWindow
{
    private static UpgradeNodeTreeWindow window;
    private UpgradeNodeTree Tree { get; set; }
    private UpgradeDatabase Database { get; set; }

    private enum State { NONE, CONNECT_CHILD }
    private State state = State.NONE;

    private UpgradeNode selected_node;

    public static void Open(UpgradeNodeTree tree)
    {
        window = (UpgradeNodeTreeWindow)GetWindow(typeof(UpgradeNodeTreeWindow));
        window.Initialize(tree);
    }

    public void Initialize(UpgradeNodeTree tree)
    {
        ClearGUI();
        Tree = tree;
        Database = UpgradeDatabase.LoadAsset();

        // Create root node
        if (Tree.nodes.Count == 0)
        {
            Tree.CreateNodeData();
        }

        // Create GUI nodes
        foreach (var data in Tree.nodes)
        {
            CreateNode(data);
        }

        // Create connections
        foreach (var data in Tree.nodes)
        {
            var parent = nodes.Select(n => (UpgradeNode)n).FirstOrDefault(n => n.data.id == data.id);
            if (parent == null) continue;

            foreach (var id_child in data.children)
            {
                var child = nodes.Select(n => (UpgradeNode)n).FirstOrDefault(n => n.data.id == id_child);
                if (child == null) continue;

                AddChild(parent, child);
            }
        }

        Save();
    }

    protected override void ProcessEvents(Event e)
    {
        base.ProcessEvents(e);

        switch (e.type)
        {
            case EventType.MouseDown:
                var node = nodes.FirstOrDefault(node => node.rect.Contains(e.mousePosition));
                if(e.button == 0)
                {
                    if(state == State.CONNECT_CHILD)
                    {
                        if(node != null && node != selected_node)
                        {
                            AddChild(selected_node, (UpgradeNode)node);
                        }

                        DeselectNode();
                        state = State.NONE;
                        Save();
                    }
                }
                else if (e.button == 1)
                {
                    if (node == null)
                    {
                        ProcessContextMenu();
                    }
                    else
                    {
                        ProcessNodeContextMenu((UpgradeNode)node);
                    }
                }
                break;
        }
    }

    private void ProcessContextMenu()
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Clear"), false, ClearData);
        menu.ShowAsContext();
    }

    private void ProcessNodeContextMenu(UpgradeNode node)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Add child"), false, () => ClickAddChild(node));
        menu.AddItem(new GUIContent("Connect child"), false, () => ClickConnectChild(node));
        menu.AddItem(new GUIContent("Delete node"), false, () => ClickDeleteNode(node));
        menu.ShowAsContext();
    }

    private void ClearData()
    {
        Tree.Clear();
        Initialize(Tree);
    }

    private void ClearGUI()
    {
        RemoveAllNodes();
    }

    private UpgradeNode CreateNode(UpgradeNodeData data)
    {
        var node = new UpgradeNode(data);
        node.isRoot = data.id == 0;
        nodes.Add(node);
        node.onPositionChanged += position => data._editorPosition = position;
        UpdateNodeProperties(node);
        return node;
    }

    private void AddChild(UpgradeNode parent, UpgradeNode child)
    {
        parent.AddChild(child);
        child.AddParent(parent);
        var connection = ConnectNodes(parent, child);

        connection.onClick += () =>
        {
            RemoveChild(parent, child);
        };
    }

    private void RemoveChild(UpgradeNode parent, UpgradeNode child)
    {
        if (child.parents.Count == 1) return;
        parent.RemoveChild(child);
        child.RemoveParent(parent);
        DisconnectNodes(parent, child);
    }

    private void DeleteNode(UpgradeNode node, bool delete_children)
    {
        // Don't delete root node
        if (node.parents.Count == 0) return;

        // Remove from parents
        foreach(var parent in node.parents)
        {
            parent.RemoveChild(node);
            DisconnectNodes(parent, node);
        }

        // Cascade to children
        foreach (var child in node.children.ToList())
        {
            if (delete_children && child.parents.Count == 1)
            {
                DeleteNode(child, delete_children);
            }
            else
            {
                child.RemoveParent(node);
                DisconnectNodes(node, child);
            }
        }

        // Remove from children
        foreach (var child in node.children)
        {
            child.RemoveParent(node);
            DisconnectNodes(node, child);
        }

        // Remove from data
        nodes.Remove(node);
        Tree.nodes.Remove(node.data);
    }

    private void SelectNode(UpgradeNode node)
    {
        DeselectNode();
        selected_node = node;
        node.Select();
    }

    private void DeselectNode()
    {
        if(selected_node != null)
        {
            selected_node.Deselect();
            selected_node = null;
        }
    }

    private void UpdateNodeProperties(UpgradeNode node)
    {
        node.ClearProperties();
        node.AddProperty("ID", node.data.id_name, true, v => 
        {
            node.data.id_name = v;
            UpdateNodeProperties(node);
        });

        var upgrade = Database.upgrades.FirstOrDefault(upgrade => upgrade.id == node.data.id_name);
        if(upgrade != null)
        {
            node.AddProperty("Name", upgrade.name, false);
            foreach(var effect in upgrade.effects)
            {
                node.AddProperty("Effect: ", effect.variable.GetDisplayString(true), false);
            }
            Save();
        }
    }

    private void ClickAddChild(UpgradeNode parent)
    {
        var child_data = Tree.CreateNodeData();
        var child = CreateNode(child_data);
        var position = new Vector2(parent.rect.position.x, parent.rect.position.y + parent.rect.height + 10);
        child.SetPosition(position);
        AddChild(parent, child);
        Save();
    }

    private void ClickConnectChild(UpgradeNode parent)
    {
        SelectNode(parent);
        state = State.CONNECT_CHILD;
    }

    private void ClickDeleteNode(UpgradeNode node)
    {
        DeleteNode(node, true);
        Save();
    }

    private Connection ConnectNodes(Node nodeA, Node nodeB)
    {
        var connection = base.ConnectNodes(nodeA, nodeB);
        connection.anchor_start = new Vector2(0, 0.5f);
        connection.anchor_end = new Vector2(0, -0.5f);
        connection.tangent_start = Vector2.up;
        connection.tangent_end = Vector2.down;
        return connection;
    }

    private void Save()
    {
        EditorUtility.SetDirty(Tree);
        AssetDatabase.SaveAssets();
    }
}