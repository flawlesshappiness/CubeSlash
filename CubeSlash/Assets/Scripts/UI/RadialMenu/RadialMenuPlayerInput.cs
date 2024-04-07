using UnityEngine;

public class RadialMenuPlayerInput : MonoBehaviour
{
    [SerializeField][Range(0f, 1f)] private float deadzone = 0.25f;
    private RadialMenu menu;

    private bool in_deadzone;

    private Vector2 prev_input;

    private void Start()
    {
        menu = GetComponent<RadialMenu>();
    }

    private void OnEnable()
    {
        PlayerInputController.Instance.Submit.Pressed += Submit;
        PlayerInputController.Instance.Cancel.Pressed += Cancel;
    }

    private void OnDisable()
    {
        PlayerInputController.Instance.Submit.Pressed -= Submit;
        PlayerInputController.Instance.Cancel.Pressed -= Cancel;
    }

    private void Update()
    {
        if (menu == null) return;

        var input = PlayerInputController.Instance.Move.Value;
        if (input.magnitude < deadzone)
        {
            if (!in_deadzone)
            {
                in_deadzone = true;
                menu.SetCurrentElement(null);
            }
        }
        else
        {
            if (DeviceController.Instance.CurrentDevice == DeviceType.KEYBOARD && menu.ElementCount > 8)
            {
                input = in_deadzone ? input : Vector2.Lerp(prev_input, input, Time.deltaTime);
                prev_input = input;
            }

            in_deadzone = false;
            menu.SelectElement(input);
        }
    }

    private void Submit()
    {
        menu.BeginSubmit();
    }

    private void Cancel()
    {
        menu.Cancel();
    }
}