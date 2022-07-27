using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorPalette", menuName = "Game/ColorPalette", order = 1)]
public class ColorPalette : ScriptableObject
{
    private static ColorPalette _main;
    public static ColorPalette Main { get { return _main ?? LoadMain(); } }

    private static ColorPalette LoadMain()
    {
        if (Application.isPlaying)
        {
            _main = Resources.Load<ColorPalette>("Color/Main");
        }
        else
        {
#if UNITY_EDITOR
            _main = AssetDatabase.LoadAssetAtPath<ColorPalette>("Assets/Resources/Color/Main.asset");
#endif
        }
        return _main;
    }

    public enum Type
    {
        PRIMARY,
        SECONDARY,
        HIGHLIGHT,
        BACKGROUND,
        CORRECT,
        WRONG,
    }

    private void OnValidate()
    {
        foreach(var map in maps)
        {
            map.name = map.type.ToString();
        }
    }

    [System.Serializable]
    public class Map
    {
        [HideInInspector] public string name;
        public Type type;
        public Color color = Color.white;
    }

    public List<Map> maps = new List<Map>();

    public Color Get(Type type)
    {
        var m = maps.FirstOrDefault(m => m.type == type);
        return m != null ? m.color : Color.white;
    }
}