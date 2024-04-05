using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageInput : MonoBehaviourExtended
{
    public PlayerInput.UIButtonType type_button;
    private Image Image { get { return GetComponentOnce<Image>(ComponentSearchType.THIS); } }
    private TMP_Text Text { get { return GetComponentOnce<TMP_Text>(ComponentSearchType.CHILDREN); } }

    private void OnEnable()
    {
        DeviceController.OnDeviceChanged += OnDeviceChanged;
    }

    private void OnDisable()
    {
        DeviceController.OnDeviceChanged -= OnDeviceChanged;
    }

    private void OnDeviceChanged(DeviceType device)
    {
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        var map = PlayerInput.Database.GetCurrentInputMap(type_button);
        Image.sprite = map.sprite;
        Image.color = map.color;
        Text.text = map.text;
    }

    public void SetInputType(PlayerInput.UIButtonType type)
    {
        this.type_button = type;
        UpdateSprite();
    }
}