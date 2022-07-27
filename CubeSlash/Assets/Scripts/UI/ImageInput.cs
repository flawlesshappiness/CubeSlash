using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageInput : MonoBehaviourExtended
{
    public PlayerInput.UIButtonType type_button;
    private Image Image { get { return GetComponentOnce<Image>(ComponentSearchType.THIS); } }

    private void OnEnable()
    {
        PlayerInput.OnDeviceChanged += OnDeviceChanged;
    }

    private void OnDisable()
    {
        PlayerInput.OnDeviceChanged -= OnDeviceChanged;
    }

    private void OnDeviceChanged(PlayerInput.DeviceType device)
    {
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        var map = PlayerInput.Database.GetCurrentInputMap(type_button);
        Image.sprite = map.sprite;
        Image.color = map.color;
    }

    public void SetInputType(PlayerInput.UIButtonType type)
    {
        this.type_button = type;
        UpdateSprite();
    }
}