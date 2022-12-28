using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(Area), menuName = "Game/" + nameof(Area), order = 1)]
public class Area : ScriptableObject
{
    public Color color_fog = Color.white;
    public Color color_bg = Color.white;
    public AnimationCurve ac_alpha_fog;
    [Range(0, 1)] public float parallax_min;
    [Range(0, 1)] public float parallax_max;
    public List<BackgroundLayer> background_layers = new List<BackgroundLayer>();

    private void OnValidate()
    {
        for (int i = 0; i < background_layers.Count; i++)
        {
            var layer = background_layers[i];
            layer.name = $"{i}";
        }

        parallax_min = Mathf.Min(parallax_min, parallax_max);
        parallax_max = Mathf.Max(parallax_min, parallax_max);
    }
}