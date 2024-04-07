using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ImageInput : MonoBehaviourExtended
{
    public InputActionReference input_action_reference;
    private Image Image => GetComponentOnce<Image>(ComponentSearchType.CHILDREN);
    private TMP_Text Text => GetComponentOnce<TMP_Text>(ComponentSearchType.CHILDREN);

    private GamepadIconDatabase DB => Database.Load<GamepadIconDatabase>();

    private void OnEnable()
    {
        DeviceController.Instance.OnDeviceChanged += OnDeviceChanged;
    }

    private void OnDisable()
    {
        DeviceController.Instance.OnDeviceChanged -= OnDeviceChanged;
    }

    private void Start()
    {
        UpdateSprite();
    }

    private void OnDeviceChanged(DeviceType device)
    {
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        Text.text = GetBindingDisplayString(out var path);
        Image.sprite = DB.GetSprite(path);

        Image.enabled = Image.sprite != null && DeviceController.Instance.CurrentDevice != DeviceType.KEYBOARD;
        Text.enabled = !string.IsNullOrEmpty(Text.text) && DeviceController.Instance.CurrentDevice == DeviceType.KEYBOARD;
    }

    private string GetBindingDisplayString(out string path)
    {
        try
        {
            var action = input_action_reference.action;
            var bindings = action.bindings.ToArray();
            var binding_index = action.GetBindingIndex(PlayerInputController.Instance.CurrentControlScheme);
            var display_string = string.Empty;
            action.GetBindingDisplayString(binding_index, out var device, out path);

            // Composite display string
            for (int i = binding_index; i < bindings.Length; i++)
            {
                var binding = bindings[i];
                if (i > binding_index && !binding.isPartOfComposite) break;
                display_string += action.GetBindingDisplayString(i);
            }

            return display_string;
        }
        catch
        {
            path = string.Empty;
            return string.Empty;
        }
    }
}