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

        PlayerInput.Controls.UI.Submit.started += _ => Submit();
        PlayerInput.Controls.UI.Cancel.started += _ => Cancel();
    }

    private void Update()
    {
        if (menu == null) return;

        var input = PlayerInput.MoveDirection.normalized;
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
            if (PlayerInput.CurrentDevice == PlayerInput.DeviceType.KEYBOARD)
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