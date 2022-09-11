using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpgradeNode
{
    public int id;
    public string id_data;
    public Vector2 _editorPosition;
    public UpgradeNode parent;
    public List<int> children = new List<int>();
}