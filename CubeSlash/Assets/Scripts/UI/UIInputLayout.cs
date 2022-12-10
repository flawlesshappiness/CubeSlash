using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInputLayout : MonoBehaviour
{
    [SerializeField] private ImageInput template_img;
    [SerializeField] private TMP_Text template_tmp;

    private List<Map> maps = new List<Map>();

    private class Map
    {
        public ImageInput Image { get; set; }
        public TMP_Text Text { get; set; }
        public PlayerInput.UIButtonType Type { get; set; }
        public void UpdateSprite()
        {
            Image.SetInputType(Type);
        }
    }

    private void Start()
    {
        template_img.gameObject.SetActive(false);
        template_tmp.transform.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        PlayerInput.OnDeviceChanged += OnDeviceChanged;
    }

    private void OnDisable()
    {
        PlayerInput.OnDeviceChanged -= OnDeviceChanged;
    }

    private void OnDeviceChanged(PlayerInput.DeviceType type)
    {
        foreach(var map in maps)
        {
            map.UpdateSprite();
        }
    }

    public void AddInput(PlayerInput.UIButtonType type, string text)
    {
        var map = new Map();
        map.Image = Instantiate(template_img, template_img.transform.parent);
        map.Image.gameObject.SetActive(true);
        map.Text = Instantiate(template_tmp.transform.gameObject, template_tmp.transform.parent).GetComponent<TMP_Text>();
        map.Text.transform.gameObject.SetActive(true);
        map.Text.text = text;
        map.Type = type;
        map.UpdateSprite();
        maps.Add(map);
    }

    public void Clear()
    {
        foreach(var map in maps)
        {
            Destroy(map.Image.gameObject);
            Destroy(map.Text.transform.gameObject);
        }
        maps.Clear();
    }
}