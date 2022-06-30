using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInputDatabase", menuName = "PlayerInputDatabase", order = 1)]
public class PlayerInputDatabase : ScriptableObject
{
    [SerializeField] private List<UIInputMapCollection> input_collections = new List<UIInputMapCollection>();

    [System.Serializable]
    public class UIInputMapCollection
    {
        [HideInInspector] public string name;
        public PlayerInput.UIButtonType type;
        public List<UIInputMap> maps = new List<UIInputMap>();
    }

    [System.Serializable]
    public class UIInputMap
    {
        [HideInInspector] public string name;
        public PlayerInput.DeviceType device;
        public Sprite sprite;
        public Color color = Color.white;
    }

    private void OnValidate()
    {
        foreach(var c in input_collections)
        {
            c.name = c.type.ToString();
            foreach(var m in c.maps)
            {
                m.name = m.device.ToString();
            }
        }
    }

    public UIInputMap GetCurrentInputMap(PlayerInput.UIButtonType type)
    {
        return GetInputMap(PlayerInput.CurrentDevice, type);
    }

    public UIInputMap GetInputMap(PlayerInput.DeviceType device, PlayerInput.UIButtonType type)
    {
        var collection = input_collections.FirstOrDefault(c => c.type == type);
        if (collection == null) return null;
        var map = collection.maps.FirstOrDefault(m => m.device == device);
        if (map == null) return null;
        return map;
    }
}