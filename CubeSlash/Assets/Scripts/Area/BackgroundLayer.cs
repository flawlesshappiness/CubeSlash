using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BackgroundLayer
{
    [HideInInspector] public string name;
    public float size_sprite_min;
    public float size_sprite_max;
    public float count_sprite;
    public List<Sprite> sprites = new List<Sprite>();
    public List<ParticleSystem> particles = new List<ParticleSystem>();
}