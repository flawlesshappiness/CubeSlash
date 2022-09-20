using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpgradeNodeData
{
    public int id;
    public string id_name;
    public Vector2 _editorPosition;
    public List<int> children = new List<int>();
}