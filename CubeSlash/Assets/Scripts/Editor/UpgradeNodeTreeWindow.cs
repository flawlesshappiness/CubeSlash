using Flawliz.Node.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class UpgradeNodeTreeWindow : NodeEditorWindow
{
    private static UpgradeNodeTreeWindow window;
    private UpgradeNodeTree Tree { get; set; }

    private Dictionary<UpgradeNode, Node> data_node_map = new Dictionary<UpgradeNode, Node>();
    private Dictionary<Node, UpgradeNode> node_data_map = new Dictionary<Node, UpgradeNode>();

    public static void Open(UpgradeNodeTree tree)
    {
        window = (UpgradeNodeTreeWindow)GetWindow(typeof(UpgradeNodeTreeWindow));
        window.Initialize(tree);
    }

    public void Initialize(UpgradeNodeTree tree)
    {
        ClearGUI();
        Tree = tree;

        // Create root node
        if(Tree.nodes.Count == 0)
        {
            Tree.CreateNode();
        }

        // Create GUI nodes
        foreach(var data in Tree.nodes)
        {
            CreateNode(data);
        }

        // Create connections
        foreach(var data in Tree.nodes)
        {
            var parent = data_node_map[data];
            foreach(var id in data.children)
            {
                var child_data = Tree.GetNode(id);
                if(child_data != null)
                {
                    var child = data_node_map[child_data];
                    ConnectNodes(parent, child);
                }
            }
        }
    }

    protected override void ProcessEvents(Event e)
    {
        base.ProcessEvents(e);

        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 1)
                {
                    var node = nodes.FirstOrDefault(node => node.rect.Contains(e.mousePosition));
                    if (node == null)
                    {
                        ProcessContextMenu();
                    }
                    else
                    {
                        ProcessNodeContextMenu(node);
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

    private void ProcessNodeContextMenu(Node node)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Add child"), false, () => ClickAddChild(node));
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
        node_data_map.Clear();
        data_node_map.Clear();
    }

    private Node CreateNode(UpgradeNode data)
    {
        var node = AddNode(data._editorPosition);
        node.AddProperty("ID", data.id_data, v => data.id_data = v);

        node_data_map.Add(node, data);
        data_node_map.Add(data, node);

        node.onPositionChanged += position => data._editorPosition = position;

        return node;
    }

    private void ClickAddChild(Node parent_node)
    {
        var child_data = Tree.CreateNode();
        var parent_data = node_data_map[parent_node];

        child_data.parent = parent_data;
        parent_data.children.Add(child_data.id);

        var child_node = CreateNode(child_data);
        var connection = ConnectNodes(parent_node, child_node);

        var position = new Vector2(parent_node.rect.position.x, parent_node.rect.position.y + parent_node.rect.height + 10);
        child_node.SetPosition(position);
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
}