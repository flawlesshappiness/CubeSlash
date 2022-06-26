using UnityEngine;

[System.Serializable]
public class AbilityVariable
{
    public string Name = "Variable";
    public int Value = 5;
    public int Max = 10;
    public Sprite sprite_icon;
    [TextArea]
    public string Description = "Description";
    public float Percentage { get { return Value / (float)Max; } }

    public void SetValue(int value)
    {
        Value = Mathf.Clamp(value, 0, Max);
    }
}