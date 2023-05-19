using UnityEngine;

public class RadialMenuOption
{
    public string Title { get; set; }
    public string Description { get; set; }
    public Sprite Sprite { get; set; }
    public System.Action OnSubmitBegin { get; set; }
    public System.Action OnSubmitComplete { get; set; }
}